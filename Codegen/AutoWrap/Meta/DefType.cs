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
    public enum ProtectionType {Public, Private, Protected }

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
                    if (ParentClass != null)
                        _replaceByType = ParentClass.FindType<DefType>(name, false);
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
                if (ParentClass != null && ParentClass.IsIgnored)
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
                return ParentClass.GetNameSpace();
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

            return (this.IsNested) ? ParentClass.FindType<T>(name, raiseException) : NameSpace.FindType<T>(name, raiseException);
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
        public ProtectionType ProtectionType;
        public DefClass ParentClass;

        public virtual bool IsNested
        {
            get { return ParentClass != null; }
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
                if (ParentClass != null)
                    return ParentClass.RealFullNativeName + "::" + Name;
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
                if (ParentClass != null)
                {
                    if (!ParentClass.IsInterface)
                    {
                        return ParentClass.FullCLRName + "::" + CLRName;
                    }
                    else
                    {
                        string name = ParentClass.FullCLRName.Replace("::" + ParentClass.CLRName, "::" + ParentClass.Name);
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
            this.ProtectionType = GetProtectionEnum(elem.GetAttribute("protection"));
        }

        public static ProtectionType GetProtectionEnum(string prot)
        {
            if (prot == "")
                return ProtectionType.Public;
            else
                return (ProtectionType)Enum.Parse(typeof(ProtectionType), prot, true);
        }
    }

    public class DefClass : DefType
    {
        public List<DefMember> Members = new List<DefMember>();
        public List<DefType> NestedTypes = new List<DefType>();
        public string[] Derives;
        public string[] Inherits;

        public override void GetNativeParamConversion(DefParam param, out string preConversion, out string conversion, out string postConversion)
        {
            switch (param.PassedByType)
            {
                case PassedByType.PointerPointer:
                    preConversion = FullCLRName + "^ out_" + param.Name + ";";
                    conversion = "out_" + param.Name;
                    postConversion = "if (" + param.Name + ") *" + param.Name + " = out_" + param.Name + ";";
                    return;
            }

            base.GetNativeParamConversion(param, out preConversion, out conversion, out postConversion);
        }

        public override void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            if (param.DefaultValue == null)
                throw new Exception("Unexpected");

            preConversion = postConversion = "";
            dependancyType = null;
            switch (param.PassedByType)
            {
                case PassedByType.Pointer:
                    if (param.DefaultValue == "NULL" || param.DefaultValue == "0")
                    {
                        conversion = "nullptr";
                        return;
                    }
                    else
                        throw new Exception("Unexpected");
                case PassedByType.PointerPointer:
                    throw new Exception("Unexpected");
                default:
                    conversion = param.DefaultValue;
                    break;
            }
        }

        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            if (param.Type.IsSharedPtr)
            {
                newname = "(" + param.NativeTypeName + ")" + param.Name;
                return String.Empty;
            }

            if (HasAttribute<NativeValueContainerAttribute>())
            {
                switch (param.PassedByType)
                {
                    case PassedByType.Pointer:
                        newname = "o_" + param.Name;
                        return param.NativeTypeName + " o_" + param.Name + " = reinterpret_cast<" + param.NativeTypeName + ">(" + param.Name + ");\n";
                }
            }

            string nativetype = FullNativeName;

            if (HasAttribute<PureManagedClassAttribute>())
            {
                string first = GetAttribute<PureManagedClassAttribute>().FirstMember;
                if (String.IsNullOrEmpty(first))
                {
                    switch (param.PassedByType)
                    {
                        case PassedByType.Reference:
                        case PassedByType.Value:
                            newname = "(" + nativetype + ")" + param.Name;
                            return null;
                        default:
                            throw new Exception("Unexpected");
                    }
                }
                else
                {
                    switch (param.PassedByType)
                    {
                        case PassedByType.Reference:
                        case PassedByType.Value:
                            newname = "*p_" + param.Name;
                            return "pin_ptr<" + nativetype + "> p_" + param.Name + " = interior_ptr<" + nativetype + ">(&" + param.Name + "->" + first + ");\n";
                        case PassedByType.Pointer:
                            if (param.IsConst)
                            {
                                string name = param.Name;
                                string expr = nativetype + "* arr_" + name + " = new " + nativetype + "[" + name + "->Length];\n";
                                expr += "for (int i=0; i < " + name + "->Length; i++)\n";
                                expr += "{\n";
                                expr += "\tarr_" + name + "[i] = *(reinterpret_cast<interior_ptr<" + nativetype + "&>>(&" + name + "[i]->" + first + "));\n";
                                expr += "}\n";
                                newname = "arr_" + name;
                                return expr;
                            }
                            else
                                throw new Exception("Unexpected");
                        default:
                            throw new Exception("Unexpected");
                    }
                }
            }

            switch (param.PassedByType)
            {
                case PassedByType.Pointer:
                    if (IsValueType)
                    {
                        if (!param.HasAttribute<ArrayTypeAttribute>()
                            && !HasWrapType(WrapTypes.NativePtrValueType))
                        {
                            newname = "o_" + param.Name;
                            return param.NativeTypeName + " o_" + param.Name + " = reinterpret_cast<" + param.NativeTypeName + ">(" + param.Name + ");\n";
                        }
                        else
                            return base.GetPreCallParamConversion(param, out newname);
                    }
                    else
                        return base.GetPreCallParamConversion(param, out newname);
                case PassedByType.PointerPointer:
                    newname = "&out_" + param.Name;
                    return (param.IsConst ? "const " : "") + FullNativeName + "* out_" + param.Name + ";\n";
                default:
                    return base.GetPreCallParamConversion(param, out newname);
            }
        }

        public override string GetPostCallParamConversionCleanup(DefParam param)
        {
            if (HasAttribute<NativeValueContainerAttribute>())
            {
                switch (param.PassedByType)
                {
                    case PassedByType.Pointer:
                        return "";
                }
            }

            if (param.Type.HasAttribute<PureManagedClassAttribute>())
            {
                switch (param.PassedByType)
                {
                    case PassedByType.Pointer:
                        if (param.IsConst)
                        {
                            return "delete[] arr_" + param.Name + ";\n";
                        }
                        else
                            throw new Exception("Unexpected");
                    default:
                        return base.GetPostCallParamConversionCleanup(param);
                }
            }

            switch (param.PassedByType)
            {
                case PassedByType.PointerPointer:
                    return param.Name + " = out_" + param.Name + ";\n";
                default:
                    return base.GetPostCallParamConversionCleanup(param);
            }
        }

        public override string GetCLRParamTypeName(DefParam param)
        {
            if (HasAttribute<NativeValueContainerAttribute>())
            {
                switch (param.PassedByType)
                {
                    case PassedByType.Pointer:
                        //return "array<" + FullCLRName + "^>^";
                        return GetCLRTypeName(param);
                }
            }

            switch (param.PassedByType)
            {
                case PassedByType.PointerPointer:
                    return "[Out] " + FullCLRName + (IsValueType ? "%" : "^%");
                default:
                    return GetCLRTypeName(param);
            }
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            if (HasAttribute<NativeValueContainerAttribute>())
            {
                switch (m.PassedByType)
                {
                    case PassedByType.Pointer:
                        //return "array<" + FullCLRName + "^>^";
                        string name = FullCLRName + "::NativeValue*";
                        if (m.IsConst) name = "const " + name;
                        return name;
                }
            }

            switch (m.PassedByType)
            {
                case PassedByType.Pointer:
                    if (IsValueType)
                    {
                        if (m.HasAttribute<ArrayTypeAttribute>())
                        {
                            return "array<" + FullCLRName + ">^";
                        }
                        else if (HasWrapType(WrapTypes.NativePtrValueType))
                        {
                            return FullCLRName;
                        }
                        else
                        {
                            string name = FullCLRName + "*";
                            if (m.IsConst) name = "const " + name;
                            return name;
                        }
                    }
                    else
                        return FullCLRName + "^";
                case PassedByType.Reference:
                case PassedByType.Value:
                    if (IsSharedPtr)
                        return FullCLRName + "^";
                    else if (IsValueType)
                        return FullCLRName;
                    else
                        return FullCLRName + "^";
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetNativeCallConversion(string expr, ITypeMember m)
        {
            if (HasAttribute<NativeValueContainerAttribute>())
            {
                switch (m.PassedByType)
                {
                    case PassedByType.Pointer:
                        return "reinterpret_cast<" + GetCLRTypeName(m) + ">(" + expr + ")";
                }
            }

            switch (m.PassedByType)
            {
                case PassedByType.Pointer:
                    if (IsValueType)
                    {
                        if (m.HasAttribute<ArrayTypeAttribute>())
                        {
                            int len = m.GetAttribute<ArrayTypeAttribute>().Length;
                            return "GetValueArrayFromNativeArray<" + FullCLRName + ", " + FullNativeName + ">( " + expr + " , " + len + " )";
                        }
                        else if (HasWrapType(WrapTypes.NativePtrValueType))
                        {
                            return expr;
                        }
                        else
                            return "reinterpret_cast<" + GetCLRTypeName(m) + ">(" + expr + ")";
                    }
                    else
                        return base.GetNativeCallConversion(expr, m);
                default:
                    return base.GetNativeCallConversion(expr, m);
            }
        }

        #region Function & Fields
        public IEnumerable PublicNestedTypes
        {
            get
            {
                foreach (DefType type in NestedTypes)
                {
                    if (type.ProtectionType == ProtectionType.Public)
                        yield return type;
                }
            }
        }

        public IEnumerable Functions
        {
            get
            {
                foreach (DefMember m in Members)
                {
                    if (m is DefFunction)
                        yield return m;
                }
            }
        }

        public IEnumerable Fields
        {
            get
            {
                foreach (DefMember m in Members)
                {
                    if (m is DefField)
                        yield return m;
                }
            }
        }

        public IEnumerable PublicMethods
        {
            get
            {
                foreach (DefFunction func in Functions)
                {
                    if (!func.IsProperty
                        && func.ProtectionType == ProtectionType.Public)
                        yield return func;
                }
            }
        }

        public IEnumerable DeclarableMethods
        {
            get
            {
                foreach (DefFunction func in Functions)
                {
                    if (func.IsDeclarableFunction
                        && !func.IsProperty)
                        yield return func;
                }
            }
        }

        public IEnumerable ProtectedMethods
        {
            get
            {
                foreach (DefFunction func in Functions)
                {
                    if (!func.IsProperty
                        && func.ProtectionType == ProtectionType.Protected)
                        yield return func;
                }
            }
        }

        public IEnumerable PublicFields
        {
            get
            {
                foreach (DefField f in Fields)
                {
                    if (f.ProtectionType == ProtectionType.Public)
                        yield return f;
                }
            }
        }

        public IEnumerable ProtectedFields
        {
            get
            {
                foreach (DefField f in Fields)
                {
                    if (f.ProtectionType == ProtectionType.Protected)
                        yield return f;
                }
            }
        }
        #endregion

        public override bool AllowVirtuals
        {
            get
            {
                return base.AllowVirtuals || IsInterface;
            }
        }

        public override bool IsBaseForSubclassing
        {
            get
            {
                return base.IsBaseForSubclassing ||
                    (this.BaseClass != null && this.BaseClass.IsBaseForSubclassing);
            }
        }

        public virtual bool IsNativeAbstractClass
        {
            get
            {
                return AllAbstractFunctions.Length > 0;
            }
        }

        public override bool IsIgnored
        {
            get
            {
                if (base.IsIgnored)
                    return true;

                if (this.ParentClass != null)
                    return this.ParentClass.IsIgnored;
                else
                    return false;
            }
        }

        public override bool IsSharedPtr
        {
            get
            {
                return (BaseClass != null && BaseClass.Name == "SharedPtr");
            }
        }

        public virtual bool IsSingleton
        {
            get
            {
                if (this.Inherits == null)
                    return false;

                foreach (string tn in this.Inherits)
                {
                    if (tn.StartsWith("Singleton<"))
                        return true;
                }
                
                return false;
            }
        }

        public override string CLRName
        {
            get
            {
                if (HasWrapType(WrapTypes.NativePtrValueType))
                {
                    return base.CLRName + "_NativePtr";
                }

                if (IsInterface)
                    return "I" + base.CLRName;
                else
                    return base.CLRName;
            }
        }

        public virtual bool IsInterface
        {
            get
            {
                return HasWrapType(WrapTypes.Interface);
            }
        }

        private DefFunction[] _allAbstractFunctions = null;
        /// <summary>
        /// All abstract functions
        /// </summary>
        public virtual DefFunction[] AllAbstractFunctions
        {
            get
            {
                if (_allAbstractFunctions == null)
                {
                    List<DefFunction> list = new List<DefFunction>();

                    if (BaseClass != null)
                    {
                        foreach (DefFunction func in BaseClass.AllAbstractFunctions)
                        {
                            if (!ContainsFunctionSignature(func.Signature, false))
                                list.Add(func);
                        }
                    }

                    foreach (DefFunction func in this.Functions)
                    {
                        if (func.IsAbstract)
                            list.Add(func);
                    }

                    _allAbstractFunctions = list.ToArray();
                }

                return _allAbstractFunctions;
            }
        }

        private DefFunction[] _abstractFunctions = null;
        /// <summary>
        /// Only declarable abstract functions
        /// </summary>
        public virtual DefFunction[] AbstractFunctions
        {
            get
            {
                if (_abstractFunctions == null)
                {
                    List<DefFunction> list = new List<DefFunction>();

                    if (BaseClass != null)
                    {
                        foreach (DefFunction func in BaseClass.AbstractFunctions)
                        {
                            if (!ContainsFunctionSignature(func.Signature, false))
                                list.Add(func);
                        }
                    }

                    foreach (DefFunction func in this.Functions)
                    {
                        if (func.IsDeclarableFunction && func.IsAbstract)
                            list.Add(func);
                    }

                    _abstractFunctions = list.ToArray();
                }

                return _abstractFunctions;
            }
        }

        private DefProperty[] _abstractProperties = null;
        public virtual DefProperty[] AbstractProperties
        {
            get
            {
                if (_abstractProperties == null)
                {
                    List<DefProperty> list = new List<DefProperty>();

                    if (BaseClass != null)
                    {
                        foreach (DefProperty prop in BaseClass.AbstractProperties)
                        {
                            if (!ContainsFunctionSignature(prop.Function.Signature, false))
                                list.Add(prop);
                        }
                    }

                    foreach (DefProperty prop in this.GetProperties())
                    {
                        if (prop.Function.IsDeclarableFunction && prop.Function.IsAbstract)
                            list.Add(prop);
                    }

                    _abstractProperties = list.ToArray();
                }

                return _abstractProperties;
            }
        }

        public virtual DefFunction GetFunctionWithSignature(string signature)
        {
            foreach (DefFunction func in this.Functions)
            {
                if (func.Signature == signature)
                    return func;
            }

            return null;
        }

        public virtual bool ContainsFunctionSignature(string signature, bool inherit)
        {
            DefFunction f;
            return ContainsFunctionSignature(signature, inherit, out f);
        }
        public virtual bool ContainsFunctionSignature(string signature, bool inherit, out DefFunction basefunc)
        {
            basefunc = null;
            if (inherit && BaseClass != null)
            {
                BaseClass.ContainsFunctionSignature(signature, inherit, out basefunc);
            }

            if (basefunc != null)
                return true;

            foreach (DefFunction func in this.Functions)
            {
                if (func.Signature == signature)
                {
                    basefunc = func;
                    return true;
                }
            }

            return false;
        }

        public virtual bool ContainsInterfaceFunctionSignature(string signature, bool inherit)
        {
            DefFunction f;
            return ContainsInterfaceFunctionSignature(signature, inherit, out f);
        }
        public virtual bool ContainsInterfaceFunctionSignature(string signature, bool inherit, out DefFunction basefunc)
        {
            basefunc = null;
            if (inherit && BaseClass != null)
            {
                BaseClass.ContainsInterfaceFunctionSignature(signature, inherit, out basefunc);
            }

            if (basefunc != null)
                return true;

            foreach (DefClass iface in GetInterfaces())
            {
                if (iface.ContainsFunctionSignature(signature, true, out basefunc))
                    return true;
            }

            return false;
        }

        private bool _isDirectSubclassOfCLRObject = false;
        public bool IsDirectSubclassOfCLRObject
        {
            get { return _isDirectSubclassOfCLRObject; }
        }

        protected bool _baseClassSearched = false;
        protected DefClass _baseClass = null;
        public virtual DefClass BaseClass
        {
            get
            {
                if (_baseClassSearched)
                    return _baseClass;

                _baseClassSearched = true;

                if (HasAttribute<BaseClassAttribute>())
                {
                    string basename = GetAttribute<BaseClassAttribute>().Name;
                    _baseClass = this.FindType<DefClass>(basename);
                }
                else
                {
                    if (this.Inherits == null)
                        return null;

                    if (this.IsDirectSubclassOfCLRObject)
                        return null;

                    List<DefClass> inherits = new List<DefClass>();
                    foreach (string tn in this.Inherits)
                    {
                        string n = tn;
                        if (n.Contains("<"))
                        {
                            if (n.StartsWith("Singleton<"))
                                continue;

                            n = n.Substring(0, n.IndexOf("<")).Trim();
                        }

                        inherits.Add(this.FindType<DefClass>(n));
                    }

                    foreach (DefClass t in inherits)
                    {
                        if (!t.IsInterface && !t.HasAttribute<IgnoreAttribute>())
                        {
                            _baseClass = t;
                            break;
                        }

                    }
                }

                return _baseClass;
            }
        }

        protected DefClass[] _interfaces = null;
        public virtual DefClass[] GetInterfaces()
        {
            if (_interfaces != null)
                return _interfaces;

            if (this.Inherits == null)
            {
                _interfaces = new DefClass[] { };
                return _interfaces;
            }

            List<DefClass> inherits = new List<DefClass>();
            foreach (string tn in this.Inherits)
            {
                string n = tn;
                if (n.Contains("<"))
                {
                    if (n.StartsWith("Singleton<"))
                        continue;

                    n = n.Substring(0, n.IndexOf("<")).Trim();
                }

                inherits.Add(this.FindType<DefClass>(n));
            }

            for (int i = 0; i < inherits.Count; i++)
            {
                if (!inherits[i].IsInterface)
                {
                    inherits.RemoveAt(i);
                    i--;
                }
            }

            _interfaces = inherits.ToArray();
            return _interfaces;
        }

        protected DefClass[] _derives = null;
        public virtual DefClass[] GetDerives()
        {
            if (_derives == null)
            {
                List<DefClass> list = new List<DefClass>();
                foreach (string cls in this.Derives)
                {
                    try
                    {
                        list.Add(FindType<DefClass>(cls));
                    }
                    catch
                    {
                    }
                }

                _derives = list.ToArray();
            }

            return _derives;
        }

        public virtual DefProperty GetProperty(string name, bool inherit)
        {
            DefProperty prop = null;
            foreach (DefProperty p in this.GetProperties())
            {
                if (p.Name == name)
                {
                    prop = p;
                    break;
                }
            }

            if (prop == null && inherit && BaseClass != null)
                return BaseClass.GetProperty(name, inherit);
            else
                return prop;
        }

        private DefFunction[] _constructors;
        public virtual DefFunction[] Constructors
        {
            get
            {
                if (_constructors == null)
                {
                    List<DefFunction> list = new List<DefFunction>();
                    foreach (DefFunction f in Functions)
                    {
                        if (f.IsConstructor)
                            list.Add(f);
                    }

                    _constructors = list.ToArray();
                }

                return _constructors;
            }
        }

        private DefProperty[] _properties;
        public virtual DefProperty[] GetProperties()
        {
            if (_properties != null)
                return _properties;

            SortedList<string, DefProperty> props = new SortedList<string, DefProperty>();

            foreach (DefFunction f in this.Functions)
            {
                if (f.IsProperty && !f.IsIgnored)
                {
                    DefProperty p = null;

                    if (props.ContainsKey(f.CLRName))
                        p = props[f.CLRName];
                    else
                    {
                        p = new DefProperty();
                        p.Name = f.CLRName;
                        if (f.IsGetProperty)
                        {
                            p.TypeName = f.TypeName;
                            p.PassedByType = f.PassedByType;
                        }
                        else
                        {
                            p.TypeName = f.Parameters[0].TypeName;
                            p.PassedByType = f.Parameters[0].PassedByType;
                        }

                        props.Add(f.CLRName, p);
                    }

                    if (f.IsGetProperty)
                    {
                        p.CanRead = true;
                        p.GetterFunction = f;
                    }
                    else if (f.IsSetProperty)
                    {
                        p.CanWrite = true;
                        p.SetterFunction = f;
                    }
                }
            }

            if (GetInterfaces().Length > 0)
            {
                foreach (DefProperty prop in props.Values)
                {
                    if (!prop.CanWrite)
                    {
                        foreach (DefClass iface in GetInterfaces())
                        {
                            DefProperty ip = iface.GetProperty(prop.Name, true);
                            if (ip != null && ip.CanWrite)
                            {
                                prop.CanWrite = true;
                                prop.SetterFunction = ip.SetterFunction;
                                break;
                            }
                        }
                    }

                    if (!prop.CanRead)
                    {
                        foreach (DefClass iface in GetInterfaces())
                        {
                            DefProperty ip = iface.GetProperty(prop.Name, true);
                            if (ip != null && ip.CanRead)
                            {
                                prop.CanRead = true;
                                prop.GetterFunction = ip.GetterFunction;
                                break;
                            }
                        }
                    }
                }
            }

            DefProperty[] parr = new DefProperty[props.Count];
            for (int i = 0; i < props.Count; i++)
                parr[i] = props.Values[i];

            _properties = parr;
            return _properties;
        }

        public DefField GetField(string name)
        {
            return GetField(name, true);
        }

        public DefField GetField(string name, bool raiseException)
        {
            DefField field = null;
            foreach (DefField f in Fields)
            {
                if (f.Name == name)
                {
                    field = f;
                    break;
                }
            }

            if (field == null && raiseException)
                throw new Exception(String.Format("DefField not found for '{0}'", name));

            return field;
        }

        public DefFunction GetFunction(string name)
        {
            return GetFunction(name, false, true);
        }

        public DefFunction GetFunction(string name, bool inherit, bool raiseException)
        {
            DefFunction func = null;
            foreach (DefFunction f in Functions)
            {
                if (f.Name == name)
                {
                    func = f;
                    break;
                }
            }

            if (func == null && inherit && BaseClass != null)
                func = BaseClass.GetFunction(name, inherit, raiseException);

            if (func == null && raiseException)
                throw new Exception("DefFunction not found");

            return func;
        }

        public DefType GetNestedType(string name)
        {
            return GetNestedType(name, true);
        }
        public DefType GetNestedType(string name, bool raiseException)
        {
            foreach (DefType t in NestedTypes)
            {
                if (t.Name == name)
                    return t;
            }

            if (raiseException)
                throw new Exception(String.Format("DefType not found for '{0}'", name));
            else
                return null;
        }

        public DefMember GetMember(string name)
        {
            return GetMember(name, true);
        }
        public DefMember GetMember(string name, bool raiseException)
        {
            foreach (DefMember m in Members)
            {
                if (m.Name == name)
                    return m;
            }

            if (raiseException)
                throw new Exception(String.Format("DeMember not found for '{0}'", name));
            else
                return null;
        }

        public DefMember[] GetMembers(string name)
        {
            return GetMembers(name, true);
        }
        public DefMember[] GetMembers(string name, bool raiseException)
        {
            List<DefMember> list = new List<DefMember>();

            foreach (DefMember m in Members)
            {
                if (m.Name == name)
                    list.Add(m);
            }

            if (list.Count == 0 && raiseException)
                throw new Exception(String.Format("DefMembers not found for '{0}'", name));

            return list.ToArray();
        }

        public virtual bool HasAttribute<T>(bool inherit) where T : AutoWrapAttribute
        {
            bool local = HasAttribute<T>();

            if (inherit)
            {
                if (local)
                    return true;
                if (BaseClass == null)
                    return false;
                return BaseClass.HasAttribute<T>(inherit);
            }
            else
                return local;
        }

        public virtual T GetAttribute<T>(bool inherit)
        {
            T attr = GetAttribute<T>();

            if (inherit)
            {
                if (attr != null)
                    return attr;
                if (BaseClass == null)
                    return default(T);
                return BaseClass.GetAttribute<T>(inherit);
            }
            else
                return attr;
        }

        public override T FindType<T>(string name, bool raiseException)
        {
            if (name.StartsWith(Globals.NativeNamespace + "::"))
            {
                name = name.Substring(name.IndexOf("::") + 2);
                return GetNameSpace().FindType<T>(name, raiseException);
            }

            List<DefType> list = new List<DefType>();

            foreach (DefType t in NestedTypes)
            {
                if (t is T && t.Name == name)
                {
                    list.Add(t);
                }
            }

            if (list.Count == 0)
            {
                if (BaseClass != null)
                {
                    T t = BaseClass.FindType<T>(name, false);
                    if (!(t is DefInternal))
                        return t;
                }

                if (ParentClass != null)
                    return ParentClass.FindType<T>(name, raiseException);
                else
                    return NameSpace.FindType<T>(name, raiseException);
            }
            else if (list.Count > 1)
                throw new Exception("Found more than one type");
            else
            {
                return (T)(object)DefType.CreateExplicitType(list[0]);
            }
        }

        public DefClass(XmlElement elem) : base(elem)
        {
            if (this.GetType() == typeof(DefClass)
                && elem.Name != "class")
                throw new Exception("Not class element");

            foreach (XmlElement child in elem.ChildNodes)
            {
                switch (child.Name)
                {
                    case "function":
                        DefFunction func = new DefFunction(child);
                        func.Class = this;
						if (func.Name != "DECLARE_INIT_CLROBJECT_METHOD_OVERRIDE" && !func.Name.StartsWith("OGRE_"))
							AddNewFunction(func);
                		break;

                    case "variable":
                        DefField field = new DefField(child);
						if (field.Name != this.Name && !field.Name.StartsWith("OGRE_"))
                        {
                            field.Class = this;
                            Members.Add(field);
                        }
                        break;

                    case "derives":
                        List<string> list = new List<string>();
                        foreach (XmlElement sub in child.ChildNodes)
                        {
                            if (sub.Name != "subClass")
                                throw new Exception("Unknown element; expected 'subClass'");
                            list.Add(sub.InnerText);
                        }
                        Derives = list.ToArray();
                        break;

                    case "inherits":
                        List<string> ilist = new List<string>();
                        foreach (XmlElement sub in child.ChildNodes)
                        {
                            if (sub.Name != "baseClass")
                                throw new Exception("Unknown element; expected 'baseClass'");
                            if (sub.InnerText != "")
                            {
                                if (sub.InnerText == "CLRObject")
                                {
                                    this.Attributes.Add(new CLRObjectAttribute());
                                    this._isDirectSubclassOfCLRObject = true;
                                }
                                else
                                    ilist.Add(sub.InnerText);
                            }
                        }
                        Inherits = ilist.ToArray();
                        break;

                    default:
                        DefType type = CreateType(child);
                        type.ParentClass = this;
                        type.NameSpace = this.NameSpace;
                        NestedTypes.Add(type);
                        break;
                }
            }
        }

        private void AddNewFunction(DefFunction func)
        {
            DefMember[] members = GetMembers(func.Name, false);
            DefFunction prevf = null;
            foreach (DefMember m in members)
            {
                if (m is DefFunction)
                {
                    if ((m as DefFunction).SignatureNameAndParams == func.SignatureNameAndParams)
                    {
                        prevf = m as DefFunction;
                        break;
                    }
                }
            }

            if (prevf != null)
            {
                if ((prevf.IsConstFunctionCall && func.IsConstFunctionCall)
                    || (!prevf.IsConstFunctionCall && !func.IsConstFunctionCall))
                {
                    //throw new Exception("couldn't pick a function to keep");
                    //Add it and sort them out later
                    Members.Add(func);
                }

                if ((prevf.ProtectionType == func.ProtectionType
                        && prevf.IsConstFunctionCall)
                    || (prevf.ProtectionType == ProtectionType.Protected
                        && func.ProtectionType == ProtectionType.Public))
                {
                    Members.Remove(prevf);
                    Members.Add(func);
                }
                else
                {
                    // Don't add func to Members;
                }
            }
            else
                Members.Add(func);
        }

        public static string GetName(XmlElement elem)
        {
            if (elem.Name != "class")
                throw new Exception("Wrong element; expected 'class'.");

            return elem.GetAttribute("name");
        }

        public static string GetFullName(XmlElement elem)
        {
            if (elem.Name != "class")
                throw new Exception("Wrong element; expected 'class'.");

            string pfullname = elem.GetAttribute("fullName");
            if (pfullname == "")
                throw new Exception("class element with no 'fullName' attribute");

            return pfullname;
        }
    }

    public class DefStruct : DefClass
    {
        public DefStruct(XmlElement elem) : base(elem)
        {
            if (elem.Name != "struct")
                throw new Exception("Not struct element");
        }
    }

    public class DefTypeDef : DefType
    {
        public override void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            preConversion = postConversion = "";
            dependancyType = null;
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
                conversion = param.DefaultValue;
            else
                throw new Exception("Unexpected");
        }

        public override void GetNativeParamConversion(DefParam param, out string preConversion, out string conversion, out string postConversion)
        {
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
                BaseType.GetNativeParamConversion(param, out preConversion, out conversion, out postConversion);
            else
                base.GetNativeParamConversion(param, out preConversion, out conversion, out postConversion);
        }

        public override string GetCLRParamTypeName(DefParam param)
        {
            return BaseType.GetCLRParamTypeName(param).Replace(BaseType.FullCLRName, FullCLRName).Replace(BaseType.FullNativeName, FullNativeName);
        }
        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
                return BaseType.GetPreCallParamConversion(param, out newname).Replace(BaseType.FullCLRName, FullCLRName).Replace(BaseType.FullNativeName, FullNativeName);
            else
                return base.GetPreCallParamConversion(param, out newname);
        }
        public override string GetPostCallParamConversionCleanup(DefParam param)
        {
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
                return BaseType.GetPostCallParamConversionCleanup(param).Replace(BaseType.FullCLRName, FullCLRName).Replace(BaseType.FullNativeName, FullNativeName);
            else
                return base.GetPostCallParamConversionCleanup(param);
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            return BaseType.GetCLRTypeName(m).Replace(BaseType.FullCLRName, FullCLRName).Replace(BaseType.FullNativeName, FullNativeName);
        }
        public override string GetNativeCallConversion(string expr, ITypeMember m)
        {
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
                return BaseType.GetNativeCallConversion(expr, m).Replace(BaseType.FullCLRName, FullCLRName).Replace(BaseType.FullNativeName, FullNativeName);
            else
                return base.GetNativeCallConversion(expr, m);
        }

        public static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            DefTypeDef expl = null;

        	if (typedef.BaseTypeName.Contains("<") ||
				typedef.BaseTypeName.Contains("std::") ||
				Mogre17.IsCollection(typedef.BaseTypeName) )
            {
                if (typedef.BaseTypeName == "std::vector" || typedef.BaseTypeName == "std::list")
                {
                    expl = DefTemplateOneType.CreateExplicitType(typedef);
                }
                else
                {
                    switch (typedef.TypeNames.Length)
                    {
                        case 1:
                            expl = DefTemplateOneType.CreateExplicitType(typedef);
                            break;
                        case 2:
							expl = DefTemplateTwoTypes.CreateExplicitType(typedef);	
                            break;
                        default:
                            throw new Exception("Unexpected");
                    }
                }
            }
            else if (typedef.Name == "String")
            {
            	expl = new DefStringTypeDef(typedef.Element);
            }

            if (expl != null)
            {
                expl.ParentClass = typedef.ParentClass;
                expl.NameSpace = typedef.NameSpace;
                expl.Attributes = typedef.Attributes;
                return expl;
            }
            else
                return typedef;
        }

    	public override bool IsIgnored
        {
            get
            {
                if (base.IsIgnored)
                    return true;

                foreach (ITypeMember m in TypeMembers)
                    if (m.Type.IsIgnored || m.Type.HasWrapType(WrapTypes.NativeDirector))
                        return true;

                return false;
            }
        }

        public override bool IsValueType
        {
            get
            {
                return BaseType.IsValueType;
            }
        }

        private DefType _baseType;
        public virtual DefType BaseType
        {
            get
            {
                if (_baseType == null)
                {
                    string basename = BaseTypeName;
                    if (basename.Contains("<"))
                        basename = basename.Substring(0, basename.IndexOf("<")).Trim();

                    _baseType = FindType<DefType>(basename, false);
                }

                return _baseType;
            }
        }

        public bool IsTypedefOfInternalType
        {
            get { return BaseType is DefInternal || (BaseType is DefTypeDef && (BaseType as DefTypeDef).IsTypedefOfInternalType); }
        }

        public string BaseTypeName;
        public string[] TypeNames;
        
        protected PassedByType[] _passed;

        private ITypeMember[] _types;

    	public virtual ITypeMember[] TypeMembers
        {
            get
            {
                if (_types == null)
                {
                    _types = new ITypeMember[TypeNames.Length];
                    for (int i = 0; i < TypeNames.Length; i++)
                    {
                        bool isConst = false;
                        string name = TypeNames[i];
                        if (name.StartsWith("const "))
                        {
                            isConst = true;
                            name = name.Substring("const ".Length);
                        }
                        _types[i] = new DefTypeMember(FindType<DefType>(name, false), _passed[i], isConst);
                    }
                }

                return _types;
            }
        }

        public override bool IsSharedPtr
        {
            get
            {
                return BaseTypeName.StartsWith("SharedPtr");
            }
        }

        public DefTypeDef(XmlElement elem)
            : base(elem)
        {
            if (elem.Name != "typedef")
                throw new Exception("Not typedef element");

            this.BaseTypeName = elem.GetAttribute("basetype");
            string type = elem.GetAttribute("type");
            if (type == "")
            {
                this.TypeNames = new string[elem.ChildNodes.Count];
                this._passed = new PassedByType[elem.ChildNodes.Count];
                for (int i = 0; i < elem.ChildNodes.Count; i++)
                {
                    this.TypeNames[i] = elem.ChildNodes[i].InnerText.Trim();
                    string pass = (elem.ChildNodes[i] as XmlElement).GetAttribute("passedBy");
                    _passed[i] = (pass == "") ? PassedByType.Value : (PassedByType)Enum.Parse(typeof(PassedByType), pass, true);
                }
            }
            else
            {
                this.TypeNames = new string[1];
                this.TypeNames[0] = type.Trim();
                this._passed = new PassedByType[1];
                string pass = elem.GetAttribute("passedBy");
                _passed[0] = (pass == "") ? PassedByType.Value : (PassedByType)Enum.Parse(typeof(PassedByType), pass, true);
            }
        }
    }

    public class DefEnum : DefType
    {
        public override bool IsValueType
        {
            get
            {
                return true;
            }
        }

        public override string FullNativeName
        {
            get
            {
                if (ProtectionType == ProtectionType.Protected)
                {
                    return NativeProtectedTypesProxy.GetProtectedTypesProxyName(ParentClass) + "::" + Name;
                }
                else
                    return base.FullNativeName;
            }
        }

        public override void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            preConversion = postConversion = "";
            dependancyType = null;
            switch (param.PassedByType)
            {
                case PassedByType.Pointer:
                    preConversion = FullCLRName + " out_" + param.Name + ";";
                    conversion = "out_" + param.Name;
                    return;
                default:
                    conversion = FullCLRName + "::" + param.DefaultValue;
                    return;
            }
        }

        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                    newname = "*(" + FullNativeName + "*)p_" + param.Name;
                    return "pin_ptr<" + FullCLRName + "> p_" + param.Name + " = &" + param.Name + ";\n";
                case PassedByType.Pointer:
                    newname = "(" + FullNativeName + "*)p_" + param.Name;
                    return "pin_ptr<" + FullCLRName + "> p_" + param.Name + " = &" + param.Name + ";\n";
                case PassedByType.Value:
                    newname = "(" + FullNativeName + ")" + param.Name;
                    return string.Empty;
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetCLRParamTypeName(DefParam param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Pointer:
                    return "[Out] " + FullCLRName + "%";
                default:
                    return GetCLRTypeName(param);
            }
        }

        public override string GetNativeCallConversion(string expr, ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                    return "(" + FullCLRName + ")" + expr;
                case PassedByType.Reference:
                case PassedByType.Pointer:
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                    return FullCLRName;
                case PassedByType.Reference:
                case PassedByType.Pointer:
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override void GetNativeParamConversion(DefParam param, out string preConversion, out string conversion, out string postConversion)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Pointer:
                    preConversion = FullCLRName + " out_" + param.Name + ";";
                    conversion = "out_" + param.Name;
                    postConversion = "if (" + param.Name + ") *" + param.Name + " = (" + FullNativeName + ")out_" + param.Name + ";";
                    return;
            }

            base.GetNativeParamConversion(param, out preConversion, out conversion, out postConversion);
        }

        private string[] _nativeEnumValues;
        public string[] NativeEnumValues
        {
            get
            {
                if (_nativeEnumValues == null)
                    FillEnumValues();

                return _nativeEnumValues;
            }
        }

        private string[] _CLREnumValues;
        public string[] CLREnumValues
        {
            get
            {
                if (_CLREnumValues == null)
                    FillEnumValues();

                return _CLREnumValues;
            }
        }

        private void FillEnumValues()
        {
            this._nativeEnumValues = new string[_elem.ChildNodes.Count];
            this._CLREnumValues = new string[_elem.ChildNodes.Count];

            for (int i = 0; i < _elem.ChildNodes.Count; i++)
            {
                string parentName = this.FullNativeName;
                parentName = parentName.Substring(0, parentName.LastIndexOf("::"));
                string nativeName = (_elem.ChildNodes[i] as XmlElement).GetAttribute("name");
                //if (ProtectionType == ProtectionType.Protected)
                //    nativeName = "PUBLIC_" + nativeName;

                this._nativeEnumValues[i] = parentName + "::" + nativeName;
                this._CLREnumValues[i] = (_elem.ChildNodes[i] as XmlElement).GetAttribute("name");
            }
        }

        public DefEnum(XmlElement elem)
            : base(elem)
        {
            if (elem.Name != "enumeration")
                throw new Exception("Not enumeration element");

            #region Code to convert enumeration strings to CamelCase form (commented out)

            // Convert the enumeration strings from UPPER_CASE form to CamelCase

            //string prefix = null;
            //int index = this.CLREnums[0].IndexOf("_");
            //if (index != -1)
            //{
            //    prefix = this.CLREnums[0].Substring(0, index + 1);
                
            //    foreach (string enumstr in CLREnums)
            //    {
            //        if (!enumstr.StartsWith(prefix)
            //            || !Char.IsLetter(enumstr[prefix.Length]))
            //        {
            //            prefix = null;
            //            break;
            //        }
            //    }
            //}

            //if (prefix != null)
            //{
            //    //Remove the prefix string
            //    for (int i=0; i < CLREnums.Length; i++)
            //        CLREnums[i] = CLREnums[i].Substring(prefix.Length);
            //}

            //for (int i=0; i < CLREnums.Length; i++)
            //{
            //    if (CLREnums[i] == CLREnums[i].ToUpper())
            //    {
            //        string[] words = CLREnums[i].Split('_');
            //        for (int k = 0; k < words.Length; k++)
            //        {
            //            string w = words[k].ToLower();
            //            if (w == "")
            //                continue;

            //            words[k] = Char.ToUpper(w[0]) + w.Substring(1);
            //        }

            //        string res = String.Join("", words);
            //        if (Char.IsLetter(res[0]))
            //            CLREnums[i] = res;
            //    }
            //}
            #endregion
        }
    }
}
