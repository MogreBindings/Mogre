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
    /// classes: <see cref="DefClass"/>, <see cref="DefEnum"/>, <see cref="DefInternal"/>,
    /// <see cref="DefTypeDef"/>, <see cref="DefString"/>, and <see cref="DefUtfString"/>.
    /// </summary>
    public abstract class DefType : AttributeHolder
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

        private DefType _replaceByType;
        public virtual DefType ReplaceByType
        {
            get
            {
                if (_replaceByType == null && HasAttribute<ReplaceByAttribute>())
                {
                    string name = GetAttribute<ReplaceByAttribute>().Name;
                    if (SurroundingClass != null)
                        _replaceByType = SurroundingClass.FindType<DefType>(name, false);
                    else
                        _replaceByType = NameSpace.FindType<DefType>(name, false);
                }

                return _replaceByType;
            }
        }

        /// <summary>
        /// Creates an instance of this class from the specified xml element. This method will
        /// create an instance from an apropriate subclass (e.g. <see cref="DefClass"/> for a class).
        /// </summary>
        /// <returns>Returns the new instance or "null" in case of a global variable.</returns>
        public static DefType CreateType(XmlElement elem)
        {
            switch (elem.Name)
            {
                case "class":
                    return new DefClass(elem);
                case "struct":
                    return new DefStruct(elem);
                case "typedef":
                    return new DefTypeDef(elem);
                case "enumeration":
                    return new DefEnum(elem);
                case "variable":
                    //It's global variables, ignore them
                    return null;
                default:
                    throw new Exception("Type unknown: '" + elem.Name + "'");
            }
        }

        public DefType CreateExplicitType()
        {
            if (this.ReplaceByType != null)
            {
                return this.ReplaceByType;
            }

            if (this is DefTypeDef)
            {
                return DefTypeDef.CreateExplicitType(this as DefTypeDef);
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

        public virtual void GetNativeParamConversion(DefParam param, out string preConversion, out string conversion, out string postConversion)
        {
            preConversion = postConversion = null;
            conversion = param.Type.GetNativeCallConversion(param.Name, param);
        }

        public virtual void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            throw new Exception("Unexpected");
        }

        public abstract string GetCLRTypeName(ITypeMember m);
        public abstract string GetCLRParamTypeName(DefParam param);

        public virtual string GetNativeCallConversion(string expr, ITypeMember m)
        {
            return expr;
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
        public virtual string ProducePreCallParamConversionCode(DefParam param, out string newname)
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
        public virtual string ProducePostCallParamConversionCleanupCode(DefParam param) {
            return "";
        }

        public virtual bool IsBaseForSubclassing
        {
            get
            {
                return this.HasWrapType(WrapTypes.Overridable);
            }
            //get { return this.HasAttribute<BaseForSubclassingAttribute>(); }
        }

        public virtual bool AllowVirtuals
        {
            get { return IsBaseForSubclassing; }
        }

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

        public DefNameSpace GetNameSpace()
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

        public DefNameSpace NameSpace;
        public ProtectionLevel ProtectionLevel;
        /// <summary>
        /// The class this type is nested within or <c>null</c> if this type is not nested.
        /// </summary>
        /// <seealso cref="IsNested"/>
        public DefClass SurroundingClass;

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
                        return DefClass.GetName(_elem.ParentNode as XmlElement);
                    case "namespace":
                        return DefNameSpace.GetFullName(_elem.ParentNode as XmlElement);
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
                        return DefClass.GetFullName(_elem.ParentNode as XmlElement);
                    case "namespace":
                        return DefNameSpace.GetFullName(_elem.ParentNode as XmlElement);
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

        protected DefType()
        {
        }

        protected DefType(XmlElement elem)
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
            if (!(this is DefTypeDef))
                return false;

            if (this.IsSharedPtr)
                return false;

            DefTypeDef explicitType = this.IsNested ? this.SurroundingClass.FindType<DefTypeDef>(this.Name)
                                                    : this.NameSpace.FindType<DefTypeDef>(this.Name);
            if (explicitType.IsSTLContainer)
                return false;

            if (explicitType.BaseType is DefInternal)
                return true;

            return false;
        }
    }
}
