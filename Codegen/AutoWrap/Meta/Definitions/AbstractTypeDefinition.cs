#region GPL license
/*
 * This source file is part of the AutoWrap code generator of the
 * MOGRE project (http://mogre.sourceforge.net).
 * 
 * Copyright (C) 2006-2007 Argiris Kirtzidis
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AutoWrap.Meta
{
    /// <summary>
    /// This abstract class describes a C++ type. The following constructs are supported: 
    /// classes, structs, typedefs, and enumerations. This class has the following child
    /// classes: <see cref="ClassDefinition"/>, <see cref="EnumDefinition"/>, <see cref="DefInternal"/>,
    /// <see cref="TypedefDefinition"/>, <see cref="DefString"/>, and <see cref="DefUtfString"/>.
    /// </summary>
    public abstract class AbstractTypeDefinition : AttributeSet
    {
        public bool IsSTLContainer
        {
            get { return STLContainer != null; }
        }

        public virtual bool IsUnnamedSTLContainer
        {
            get { return false; }
        }

        public virtual string STLContainer
        {
            get { return null; }
        }

        public virtual string FullSTLContainerTypeName
        {
            get { return null; }
        }

        public bool IsTemplate
        {
            get { return _elem.GetAttribute("template") == "true"; }
        }

        /// <summary>
        /// Denotes whether subclasses of this class can be created.
        /// </summary>
        public virtual bool AllowSubClassing
        {
            get { return this.HasWrapType(WrapTypes.Overridable); }
        }

        public virtual bool AllowVirtuals
        {
            get { return AllowSubClassing; }
        }

        /// <summary>
        /// Indicates whether this type is ignored.
        /// </summary>
        public virtual bool IsIgnored
        {
            get
            {
                if (SurroundingClass != null && SurroundingClass.IsIgnored)
                    return true;

                return this.HasAttribute<IgnoreAttribute>();
            }
        }

        public virtual bool IsPureManagedClass
        {
            get { return HasAttribute<PureManagedClassAttribute>(); }
        }

        public virtual bool IsValueType
        {
            get { return HasAttribute<ValueTypeAttribute>(); }
        }

        private AbstractTypeDefinition _replaceByType;
        public virtual AbstractTypeDefinition ReplaceByType
        {
            get
            {
                if (_replaceByType == null && HasAttribute<ReplaceByAttribute>())
                {
                    string name = GetAttribute<ReplaceByAttribute>().Name;
                    if (SurroundingClass != null)
                        _replaceByType = SurroundingClass.FindType<AbstractTypeDefinition>(name, false);
                    else
                        _replaceByType = NameSpace.FindType<AbstractTypeDefinition>(name, false);
                }

                return _replaceByType;
            }
        }

        /// <summary>
        /// Creates an instance of this class from the specified xml element. This method will
        /// create an instance from an apropriate subclass (e.g. <see cref="ClassDefinition"/> for a class).
        /// </summary>
        /// <returns>Returns the new instance or "null" in case of a global variable.</returns>
        public static AbstractTypeDefinition CreateType(XmlElement elem)
        {
            switch (elem.Name)
            {
                case "class":
                    return new ClassDefinition(elem);
                case "struct":
                    return new StructDefinition(elem);
                case "typedef":
                    return new TypedefDefinition(elem);
                case "enumeration":
                    return new EnumDefinition(elem);
                case "variable":
                    //It's global variables, ignore them
                    return null;
                default:
                    throw new Exception("Type unknown: '" + elem.Name + "'");
            }
        }

        public AbstractTypeDefinition CreateExplicitType()
        {
            if (this.ReplaceByType != null)
            {
                return this.ReplaceByType;
            }

            if (this is TypedefDefinition)
            {
                return TypedefDefinition.CreateExplicitType(this as TypedefDefinition);
            } else
            {
                return this;
            }
        }


        public virtual string GetNativeTypeName(bool isConst, PassedByType passed)
        {
            string postfix = null;
            switch (passed)
            {
                case PassedByType.Pointer:
                    postfix = "*";
                    break;
                case PassedByType.PointerPointer:
                    postfix = "**";
                    break;
                case PassedByType.Reference:
                    postfix = "&";
                    break;
                case PassedByType.Value:
                    postfix = "";
                    break;
                default:
                    throw new Exception("Unexpected");
            }
            return (isConst ? "const " : "") + FullNativeName + postfix;
        }

        public abstract string GetCLRTypeName(ITypeMember m);
        public abstract string GetCLRParamTypeName(ParamDefinition param);

        #region Code Generation Methods

        public virtual string ProduceNativeCallConversionCode(string expr, ITypeMember m)
        {
            return expr;
        }

        public virtual void ProduceNativeParamConversionCode(ParamDefinition param, out string preConversion, out string conversion, out string postConversion)
        {
            preConversion = postConversion = null;
            conversion = param.Type.ProduceNativeCallConversionCode(param.Name, param);
        }

        public virtual void ProduceDefaultParamValueConversionCode(ParamDefinition param, out string preConversion, out string conversion, out string postConversion, out AbstractTypeDefinition dependancyType)
        {
            throw new Exception("Unexpected");
        }

        /// <summary>
        /// Produces the code to convert a single parameter from a CLR type to its native
        /// counterpart. This code then will be inserted before the call to the method that
        /// uses this parameter.
        /// </summary>
        /// <param name="param">the parameter to pass</param>
        /// <param name="newname">the name of the converted (i.e. native) parameter; should
        /// be <c>param.Name</c> if no conversion is done.</param>
        /// <returns>the conversion code; should be an empty string, if no conversion is done</returns>
        public virtual string ProducePreCallParamConversionCode(ParamDefinition param, out string newname)
        {
            newname = param.Name;
            return "";
        }

        /// <summary>
        /// Produces the code to convert a single parameter from a native type to its CLR
        /// counterpart. This code then will be inserted after the call to the method that
        /// uses this parameter. The code may also contain cleanup code.
        /// </summary>
        /// <param name="param">the parameter that was passed</param>
        /// <returns>the conversion code; should be an empty string, if no conversion is done</returns>
        public virtual string ProducePostCallParamConversionCleanupCode(ParamDefinition param)
        {
            return "";
        }

        #endregion

        public NamespaceDefinition GetNameSpace()
        {
            if (this.NameSpace == null)
                return SurroundingClass.GetNameSpace();
            else
                return this.NameSpace;
        }

        public T FindType<T>(string name)
        {
            return FindType<T>(name, true);
        }
        public virtual T FindType<T>(string name, bool raiseException)
        {
            if (name.StartsWith(Globals.NativeNamespace + "::"))
            {
                name = name.Substring(name.IndexOf("::") + 2);
                return GetNameSpace().FindType<T>(name, raiseException);
            }

            return (this.IsNested) ? SurroundingClass.FindType<T>(name, raiseException) : NameSpace.FindType<T>(name, raiseException);
        }

        protected XmlElement _elem;

        public XmlElement Element
        {
            get { return _elem; }
        }

        public virtual string Name
        {
            get { return _elem.GetAttribute("name"); }
        }

        public override string ToString()
        {
            return Name;
        }

        public virtual string CLRName
        {
            get
            {
                if (HasAttribute<RenameAttribute>())
                    return GetAttribute<RenameAttribute>().Name;
                else
                    return Name;
            }
        }

        public virtual string IncludeFile
        {
            get
            {
                if (_elem == null)
                    return null;
                else
                    return _elem.GetAttribute("includeFile");
            }
        }

        public virtual bool IsSharedPtr
        {
            get { return false; }
        }

        public virtual bool IsReadOnly
        {
            get { return HasAttribute<ReadOnlyAttribute>(); }
        }

        public NamespaceDefinition NameSpace;
        public ProtectionLevel ProtectionLevel;
        /// <summary>
        /// The class this type is nested within or <c>null</c> if this type is not nested.
        /// </summary>
        /// <seealso cref="IsNested"/>
        public ClassDefinition SurroundingClass;

        /// <summary>
        /// Denotes whether this type is nested within a surrounding class.
        /// </summary>
        public virtual bool IsNested
        {
            get { return SurroundingClass != null; }
        }

        public virtual string ParentNativeName
        {
            get
            {
                switch (_elem.ParentNode.Name)
                {
                    case "class":
                        return ClassDefinition.GetName(_elem.ParentNode as XmlElement);
                    case "namespace":
                        return this.NameSpace.NativeName;
                    default:
                        throw new Exception("Unknown parent type '" + _elem.ParentNode.Name + "'");
                }
            }
        }

        public virtual string ParentFullNativeName
        {
            get
            {
                switch (_elem.ParentNode.Name)
                {
                    case "class":
                        return ClassDefinition.GetFullName(_elem.ParentNode as XmlElement);
                    case "namespace":
                        return this.NameSpace.CLRName;
                    default:
                        throw new Exception("Unknown parent type '" + _elem.ParentNode.Name + "'");
                }
            }
        }

        public virtual string RealFullNativeName
        {
            get
            {
                if (SurroundingClass != null)
                    return SurroundingClass.RealFullNativeName + "::" + Name;
                else
                    return NameSpace.NativeName + "::" + Name;
            }
        }

        public virtual string FullNativeName
        {
            get { return RealFullNativeName; }
        }

        public virtual string FullCLRName
        {
            get
            {
                if (SurroundingClass != null)
                {
                    if (!SurroundingClass.IsInterface)
                    {
                        return SurroundingClass.FullCLRName + "::" + CLRName;
                    }
                    else
                    {
                        string name = SurroundingClass.FullCLRName.Replace("::" + SurroundingClass.CLRName, "::" + SurroundingClass.Name);
                        return name + "::" + CLRName;
                    }
                }
                else
                {
                    return NameSpace.CLRName + "::" + CLRName;
                }
            }
        }

        public virtual bool HasWrapType(WrapTypes wrapType)
        {
            return HasAttribute<WrapTypeAttribute>() && GetAttribute<WrapTypeAttribute>().WrapType == wrapType;
        }

        protected AbstractTypeDefinition()
        {
        }

        protected AbstractTypeDefinition(XmlElement elem)
        {
            this._elem = elem;
            this.ProtectionLevel = GetProtectionEnum(elem.GetAttribute("protection"));
        }

        public static ProtectionLevel GetProtectionEnum(string prot)
        {
            if (prot == "")
                return ProtectionLevel.Public;
            else
                return (ProtectionLevel)Enum.Parse(typeof(ProtectionLevel), prot, true);
        }


        public bool IsInternalTypeDef()
        {
            if (!(this is TypedefDefinition))
                return false;

            if (this.IsSharedPtr)
                return false;

            TypedefDefinition explicitType = this.IsNested ? this.SurroundingClass.FindType<TypedefDefinition>(this.Name) 
                                                            : this.NameSpace.FindType<TypedefDefinition>(this.Name);
            if (explicitType.IsSTLContainer)
                return false;

            if (explicitType.BaseType is DefInternal)
                return true;

            return false;
        }
    }
}
