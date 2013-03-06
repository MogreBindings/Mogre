using System;
using System.Collections.Generic;
using System.Xml;
using System.Collections;

namespace AutoWrap.Meta
{
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

        public override string ProducePreCallParamConversionCode(DefParam param, out string newname)
        {
            if (param.Type.IsSharedPtr)
            {
                newname = "(" + param.MemberTypeNativeName + ")" + param.Name;
                return String.Empty;
            }

            if (HasAttribute<NativeValueContainerAttribute>())
            {
                switch (param.PassedByType)
                {
                    case PassedByType.Pointer:
                        newname = "o_" + param.Name;
                        return param.MemberTypeNativeName + " o_" + param.Name + " = reinterpret_cast<" + param.MemberTypeNativeName + ">(" + param.Name + ");\n";
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
                            return param.MemberTypeNativeName + " o_" + param.Name + " = reinterpret_cast<" + param.MemberTypeNativeName + ">(" + param.Name + ");\n";
                        }
                        else
                            return base.ProducePreCallParamConversionCode(param, out newname);
                    }
                    else
                        return base.ProducePreCallParamConversionCode(param, out newname);
                case PassedByType.PointerPointer:
                    newname = "&out_" + param.Name;
                    return (param.IsConst ? "const " : "") + FullNativeName + "* out_" + param.Name + ";\n";
                default:
                    return base.ProducePreCallParamConversionCode(param, out newname);
            }
        }

        public override string ProducePostCallParamConversionCleanupCode(DefParam param)
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
                        return base.ProducePostCallParamConversionCleanupCode(param);
                }
            }

            switch (param.PassedByType)
            {
                case PassedByType.PointerPointer:
                    return param.Name + " = out_" + param.Name + ";\n";
                default:
                    return base.ProducePostCallParamConversionCleanupCode(param);
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
                        if (m.IsConst)
                            name = "const " + name;
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
                            if (m.IsConst)
                                name = "const " + name;
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
                    if (type.ProtectionLevel == ProtectionLevel.Public)
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
                        && func.ProtectionType == ProtectionLevel.Public)
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
                        && func.ProtectionType == ProtectionLevel.Protected)
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
                    if (f.ProtectionType == ProtectionLevel.Public)
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
                    if (f.ProtectionType == ProtectionLevel.Protected)
                        yield return f;
                }
            }
        }

        #endregion

        public override bool AllowVirtuals
        {
            get { return base.AllowVirtuals || IsInterface; }
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
            get { return AllAbstractFunctions.Length > 0; }
        }

        public override bool IsIgnored
        {
            get
            {
                if (base.IsIgnored)
                    return true;

                if (SurroundingClass != null)
                    return SurroundingClass.IsIgnored;
                else
                    return false;
            }
        }

        public override bool IsSharedPtr
        {
            get { return (BaseClass != null && BaseClass.Name == "SharedPtr"); }
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
            get { return HasWrapType(WrapTypes.Interface); }
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
                            if (!prop.IsContainedIn(this, false))
                                list.Add(prop);
                        }
                    }

                    foreach (DefProperty prop in this.GetProperties())
                    {
                        if (prop.IsAbstract)
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

        /// <summary>
        /// Indicates whether a method with the specified signature is part of this class.
        /// </summary>
        /// <param name="signature">the signature</param>
        /// <param name="allowInheritedSignature">if this is <c>false</c> only this class will be
        /// checked for the signature. Otherwise all base classes will be checked as well.</param>
        public virtual bool ContainsFunctionSignature(string signature, bool allowInheritedSignature)
        {
            DefFunction f;
            return ContainsFunctionSignature(signature, allowInheritedSignature, out f);
        }

        public virtual bool ContainsFunctionSignature(string signature, bool allowInheritedSignature, out DefFunction basefunc)
        {
            basefunc = null;
            if (allowInheritedSignature && BaseClass != null)
            {
                BaseClass.ContainsFunctionSignature(signature, allowInheritedSignature, out basefunc);
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
                _interfaces = new DefClass[] {};
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
                if (f.IsProperty && f.IsDeclarableFunction)
                {
                    DefProperty p = null;

                    if (props.ContainsKey(f.CLRName))
                        p = props[f.CLRName];
                    else
                    {
                        p = new DefProperty(f.CLRName);
                        if (f.IsGetProperty)
                        {
                            p.MemberTypeName = f.TypeName;
                            p.PassedByType = f.PassedByType;
                        }
                        else
                        {
                            p.MemberTypeName = f.Parameters[0].TypeName;
                            p.PassedByType = f.Parameters[0].PassedByType;
                        }

                        props.Add(f.CLRName, p);
                    }

                    if (f.IsGetProperty)
                        p.GetterFunction = f;
                    else if (f.IsSetProperty)
                        p.SetterFunction = f;
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

                if (SurroundingClass != null)
                    return SurroundingClass.FindType<T>(name, raiseException);
                else
                    return NameSpace.FindType<T>(name, raiseException);
            }
            else if (list.Count > 1)
                throw new Exception("Found more than one type");
            else
            {
                return (T)(object)list[0].CreateExplicitType();
            }
        }

        public DefClass(XmlElement elem)
            : base(elem)
        {
            if (this.GetType() == typeof (DefClass)
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
                        type.SurroundingClass = this;
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
                    || (prevf.ProtectionType == ProtectionLevel.Protected
                        && func.ProtectionType == ProtectionLevel.Public))
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
}