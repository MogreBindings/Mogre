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
    public abstract class DefType : AttributeHolder
    {
        public static DefType CreateExplicitType(DefType type)
        {
            if (type.ReplaceByType != null)
                return type.ReplaceByType;

            if (type is DefTypeDef)
                return DefTypeDef.CreateExplicitType(type as DefTypeDef);
            else
                return type;
        }

        public static DefType CreateType(XmlElement elem)
        {
            switch (elem.Name)
            {
                case "class":
                case "struct":
                    return CreateClass(elem);
                case "typedef":
                    return CreateTypeDef(elem);
                case "enumeration":
                    return CreateEnum(elem);
                case "variable":
                    //It's global variables, ignore them
                    return null;
                default:
                    throw new Exception("Type unknown: '" + elem.Name + "'");
            }
        }

        private static DefClass CreateClass(XmlElement elem)
        {
            if (elem.Name != "class"
                && elem.Name != "struct")
                throw new Exception("Not class/struct element");

            DefClass cls = (elem.Name == "class") ? new DefClass(elem) : new DefStruct(elem);
            return cls;
        }

        private static DefTypeDef CreateTypeDef(XmlElement elem)
        {
            if (elem.Name != "typedef")
                throw new Exception("Not typedef element");

            DefTypeDef td = new DefTypeDef(elem);
            return td;
        }

        private static DefEnum CreateEnum(XmlElement elem)
        {
            if (elem.Name != "enumeration")
                throw new Exception("Not enumeration element");

            DefEnum en = new DefEnum(elem);
            return en;
        }

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

        public virtual string GetPreCallParamConversion(DefParam param, out string newname)
        {
            newname = param.Name;
            return string.Empty;
        }
        public virtual string GetPostCallParamConversionCleanup(DefParam param)
        {
            return string.Empty;
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

        public DefType()
        {
        }
        public DefType(XmlElement elem)
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
