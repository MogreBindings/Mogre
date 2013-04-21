using System;
using System.Collections.Generic;
using System.Xml;

namespace AutoWrap.Meta
{
    public class MemberMethodDefinition : MemberDefinitionBase
    {
        public List<ParamDefinition> Parameters = new List<ParamDefinition>();
        public VirtualLevel VirtualLevel;

        private bool? _isConst;
        public override bool IsConst
        {
            get
            {
                if (_isConst == null)
                {
                    string def = " " + Definition.Substring(0, Definition.IndexOf(Name));
                    _isConst = def.Contains(" const ");
                }

                return (bool) _isConst;
            }
        }

        private bool _isConstFunctionCall;
        public bool IsConstFunctionCall
        {
            get { return _isConstFunctionCall; }
        }

        private string _signature;
        public string Signature
        {
            get
            {
                if (_signature == null)
                {
                    _signature = IsVirtual.ToString() + ProtectionLevel + Name;
                    foreach (ParamDefinition param in Parameters)
                    {
                        _signature += "|" + param.TypeName + "#" + param.PassedByType + "#" + param.Container + "#" + param.Array;
                    }
                }

                return _signature;
            }
        }

        private string _signatureNameAndParams;
        public string SignatureNameAndParams
        {
            get
            {
                if (_signatureNameAndParams == null)
                {
                    _signatureNameAndParams = Name;
                    foreach (ParamDefinition param in Parameters)
                    {
                        _signatureNameAndParams += "|" + param.TypeName + "#" + param.PassedByType + "#" + param.Container + "#" + param.Array;
                    }
                }

                return _signatureNameAndParams;
            }
        }

        public override bool IsIgnored
        {
            get
            {
                if (base.IsIgnored)
                    return true;

                foreach (ParamDefinition param in Parameters)
                {
                    if (param.Type.IsIgnored)
                        return true;
                }

                return false;
            }
        }

        private bool? _isOverride;
        private MemberMethodDefinition _baseFunc;

        public bool IsOverride
        {
            get
            {
                if (_isOverride == null)
                {
                    if (IsVirtual && !ContainingClass.IsInterface && ContainingClass.BaseClass != null)
                    {
                        if (ContainingClass.BaseClass.ContainsFunctionSignature(Signature, true, out _baseFunc)
                            || ContainingClass.BaseClass.ContainsInterfaceFunctionSignature(Signature, true, out _baseFunc))
                            _isOverride = true;
                        else
                            _isOverride = false;
                    }
                    else
                        _isOverride = false;
                }

                return (bool) _isOverride;
            }
        }

        public MemberMethodDefinition BaseFunction
        {
            get
            {
                if (_baseFunc != null)
                    return _baseFunc;

                if (IsOverride)
                    return _baseFunc;

                if (ContainingClass.ContainsInterfaceFunctionSignature(Signature, true, out _baseFunc))
                    return _baseFunc;

                return null;
            }
        }

        public override bool HasAttribute<T>()
        {
            if (base.HasAttribute<T>())
                return true;

            if (BaseFunction != null)
                return BaseFunction.HasAttribute<T>();
            
            return false;
        }

        public override T GetAttribute<T>()
        {
            T res = base.GetAttribute<T>();
            if (res != null)
                return res;

            if (BaseFunction != null)
                return BaseFunction.GetAttribute<T>();
            
            return default(T);
        }

        private bool? _isVirtualInterfaceMethod;

        public bool IsVirtualInterfaceMethod
        {
            get
            {
                if (_isVirtualInterfaceMethod == null)
                {
                    if (IsVirtual)
                    {
                        if (ContainingClass.IsInterface)
                            _isVirtualInterfaceMethod = true;
                        else
                            _isVirtualInterfaceMethod = ContainingClass.ContainsInterfaceFunctionSignature(Signature, true);
                    }
                    else
                        _isVirtualInterfaceMethod = false;
                }

                return (bool) _isVirtualInterfaceMethod;
            }
        }

        public bool IsVirtual
        {
            get { return VirtualLevel != VirtualLevel.NotVirtual; }
        }

        public bool IsAbstract
        {
            get { return VirtualLevel == VirtualLevel.Abstract; }
        }

        private bool? _isDeclarableFunction;

        public bool IsDeclarableFunction
        {
            get
            {
                if (_isDeclarableFunction == null)
                    _isDeclarableFunction = !(IsConstructor || IsListenerAdder || IsListenerRemover
                                              || IsOperatorOverload || !IsFunctionAllowed(this));

                return (bool) _isDeclarableFunction;
            }
        }

        public bool IsConstructor
        {
            get { return (Name == ContainingClass.Name); }
        }

        public bool IsOperatorOverload
        {
            get
            {
                return (Name.StartsWith("operator")
                        && !Char.IsLetterOrDigit(Name["operator".Length]));
            }
        }

        public bool IsListenerAdder
        {
            get
            {
                return (Name.StartsWith("add")
                        && Name.EndsWith("Listener")
                        && Parameters.Count == 1
                        && Parameters[0].Type.HasWrapType(WrapTypes.NativeDirector));
            }
        }

        public bool IsListenerRemover
        {
            get
            {
                return (Name.StartsWith("remove")
                        && Name.EndsWith("Listener")
                        && Parameters.Count == 1
                        && Parameters[0].Type.HasWrapType(WrapTypes.NativeDirector));
            }
        }

        public bool IsProperty
        {
            get { return IsGetProperty || IsSetProperty; }
        }

        public override string CLRName
        {
            get
            {
                if (IsProperty)
                {
                    string name = HasAttribute<RenameAttribute>() ? GetAttribute<RenameAttribute>().Name : Name;

                    if (name.StartsWith("get"))
                        return name.Substring(3);
                    
                    if (name.StartsWith("set"))
                        return name.Substring(3);
                    
                    return ToCamelCase(base.CLRName);
                }
                
                if (IsOperatorOverload)
                    return base.CLRName;
                
                return ToCamelCase(base.CLRName);
            }
        }

        /// <summary>
        /// Indicates whether this method returns something (i.e. whether its return type is 
        /// <c>void</c> - but not a <c>void*</c>).
        /// </summary>
        public virtual bool HasReturnValue {
            get
            {
                return (MemberTypeName == "void" || MemberTypeName == "const void") && PassedByType == PassedByType.Value;
            }
        }

        private bool? _isGetProperty;

        public bool IsGetProperty
        {
            get
            {
                if (_isGetProperty == null)
                {
                    if (IsOverride)
                    {
                        _isGetProperty = BaseFunction.IsGetProperty;
                    }
                    else if (IsConstructor || MemberTypeName == "void" || Parameters.Count > 0)
                        _isGetProperty = false;
                    else if (HasAttribute<PropertyAttribute>())
                        _isGetProperty = true;
                    else if (HasAttribute<MethodAttribute>())
                        _isGetProperty = false;
                    else
                    {
                        bool? ret = CheckFunctionForGetProperty(this);
                        if (ret != null)
                            _isGetProperty = ret;
                        else
                        {
                            string name = HasAttribute<RenameAttribute>() ? GetAttribute<RenameAttribute>().Name : Name;

                            if (name.StartsWith("get") && Char.IsUpper(name[3]))
                            {
                                string pname = name.Substring(3);
                                //check if property name collides with a nested type
                                AbstractTypeDefinition type = ContainingClass.GetNestedType(pname, false);
                                if (type != null)
                                    _isGetProperty = false;
                                else
                                {
                                    //check if property name collides with a method
                                    MemberMethodDefinition func = ContainingClass.GetFunction(Char.ToLower(pname[0]) + pname.Substring(1), true, false);
                                    if (func != null && !func.HasAttribute<RenameAttribute>())
                                        _isGetProperty = false;
                                    else
                                        _isGetProperty = true;
                                }
                            }
                            else
                                _isGetProperty = false;
                        }
                    }
                }

                return (bool) _isGetProperty;
            }
        }

        private bool? _isSetProperty;

        public bool IsSetProperty
        {
            get
            {
                if (_isSetProperty == null)
                {
                    if (IsOverride)
                    {
                        _isSetProperty = BaseFunction.IsSetProperty;
                    }
                    else if (IsConstructor || MemberTypeName != "void" || Parameters.Count != 1)
                    {
                        _isSetProperty = false;
                    }
                    else if (HasAttribute<PropertyAttribute>())
                        _isSetProperty = true;
                    else if (HasAttribute<MethodAttribute>())
                        _isSetProperty = false;
                    else
                    {
                        string name = HasAttribute<RenameAttribute>() ? GetAttribute<RenameAttribute>().Name : Name;

                        if (name.StartsWith("set") && Char.IsUpper(name[3]))
                        {
                            // Check to see if there is a "get" function
                            MemberMethodDefinition func = ContainingClass.GetFunction("get" + name.Substring(3), false, false);
                            _isSetProperty = (func != null && func.IsGetProperty && func.MemberTypeName == Parameters[0].TypeName
                                              && (!ContainingClass.AllowVirtuals || (func.IsVirtual == IsVirtual && func.IsOverride == IsOverride)));
                        }
                        else
                            _isSetProperty = false;
                    }
                }

                return (bool) _isSetProperty;
            }
        }

        public MemberMethodDefinition(XmlElement elem, ClassDefinition containingClass)
            : base(elem, containingClass)
        {
            if (elem.Name != "function")
                throw new Exception("Wrong element; expected 'function'.");

            switch (elem.GetAttribute("virt"))
            {
                case "virtual":
                    VirtualLevel = VirtualLevel.Virtual;
                    break;
                case "non-virtual":
                    VirtualLevel = VirtualLevel.NotVirtual;
                    break;
                case "pure-virtual":
                    VirtualLevel = VirtualLevel.Abstract;
                    break;
                default:
                    throw new Exception("unexpected");
            }
            _isConstFunctionCall = bool.Parse(elem.GetAttribute("const"));
        }

        protected override void InterpretChildElement(XmlElement child)
        {
            switch (child.Name)
            {
                case "parameters":
                    int count = 1;
                    foreach (XmlElement param in child.ChildNodes)
                    {
                        ParamDefinition p = new ParamDefinition(MetaDef, param);
                        if (p.Name == null && (p.TypeName != "void" || p.PassedByType != PassedByType.Value))
                            p.Name = "param" + count;

                        if (p.Name != null)
                        {
                            p.Function = this;
                            Parameters.Add(p);
                        }
                        count++;
                    }
                    break;

                default:
                    throw new Exception("Unknown child of function: '" + child.Name + "'");
            }
        }
    }
}