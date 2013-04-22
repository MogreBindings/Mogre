using System;
using System.Collections.Generic;
using System.Xml;

namespace AutoWrap.Meta
{
    public class MemberMethodDefinition : MemberDefinitionBase
    {
        public override string CLRName
        {
            get
            {
                if (IsOperatorOverload)
                {
                    // operator - don't convert it.
                    return base.CLRName;
                }
                
                // regular method
                return MetaDef.CodeStyleDef.ConvertMethodName(base.CLRName, this);
            }
        }

    

        private bool _isConst;
        /// <summary>
        /// Denotes whether the return value of this method is C++ <c>const.</c> Note:
        /// Don't confuse this with <see cref="IsConstMethod"/>.
        /// </summary>
        public override bool IsConst
        {
            get { return _isConst; }
        }

        public readonly VirtualLevel VirtualLevel;

        public bool IsVirtual
        {
            get { return VirtualLevel != VirtualLevel.NotVirtual; }
        }

        public bool IsAbstract
        {
            get { return VirtualLevel == VirtualLevel.Abstract; }
        }

        /// <summary>
        /// Denotes whether this method is C++ <c>const</c>, i.e. whether it can change
        /// the inner state of the containing class or not. Note that this value is
        /// different from <see cref="IsConst"/> in that <c>IsConst</c> describes whether
        /// the <i>return value</i> is <c>const</c> or not.
        /// </summary>
        public readonly bool IsConstMethod;

        private MethodSignature _signature;
        public MethodSignature Signature
        {
            get
            {
                if (_signature == null)
                    _signature = new MethodSignature(this);

                return _signature;
            }
        }

        private readonly List<ParamDefinition> _parameters = new List<ParamDefinition>();
        /// <summary>
        /// Contains the parameters of this method.
        /// </summary>
        public readonly IList<ParamDefinition> Parameters;

        public override bool IsIgnored
        {
            get
            {
                if (base.IsIgnored)
                    return true;

                // If one of the parameters is ignore, then so is this methods.
                foreach (ParamDefinition param in _parameters)
                {
                    if (param.Type.IsIgnored)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates whether this method returns something (i.e. whether its return type is 
        /// <c>void</c> - but not a <c>void*</c>).
        /// </summary>
        public virtual bool HasReturnValue
        {
            get
            {
                return (MemberTypeName == "void" || MemberTypeName == "const void") && PassedByType == PassedByType.Value;
            }
        }

        private bool? _isOverriding;
        private MemberMethodDefinition _baseFunc;

        /// <summary>
        /// Denotes whether this method overrides a method in one of the class' base classes. If 
        /// this is true, the method is accessible through <see cref="BaseMethod"/>.
        /// </summary>
        public bool IsOverriding
        {
            get
            {
                if (_isOverriding == null)
                {
                    if (IsVirtual && !ContainingClass.IsInterface && ContainingClass.BaseClass != null)
                    {
                        if (   ContainingClass.BaseClass.ContainsFunctionSignature(Signature, true, out _baseFunc)
                            || ContainingClass.BaseClass.ContainsInterfaceFunctionSignature(Signature, true, out _baseFunc))
                            _isOverriding = true;
                        else
                            _isOverriding = false;
                    }
                    else
                        _isOverriding = false;
                }

                return (bool)_isOverriding;
            }
        }

        /// <summary>
        /// The method this method overrides (see <see cref="IsOverriding"/>) or implements. The 
        /// method is contained in one of the class' base classes or implemented interfaces.
        /// Note that <see cref="IsOverriding"/> may be <c>false</c> even though this property
        /// has a value. This is the case when this class implements an interface directly (i.e.
        /// it's not implemented by any base class). In this case the implemented method doesn't
        /// override any base method but has a base method (i.e. the one in the implemented 
        /// interface).
        /// </summary>
        public MemberMethodDefinition BaseMethod
        {
            get
            {
                if (_baseFunc != null)
                    return _baseFunc;

                if (IsOverriding)
                    return _baseFunc;

                // Check whether the class directly implements an interface and whether one of this
                // interface's methods is implemented by this method.
                if (ContainingClass.ContainsInterfaceFunctionSignature(Signature, true, out _baseFunc))
                    return _baseFunc;

                return null;
            }
        }

        public override bool HasAttribute<T>()
        {
            if (base.HasAttribute<T>())
                return true;

            if (BaseMethod != null)
                return BaseMethod.HasAttribute<T>();
            
            return false;
        }

        public override T GetAttribute<T>()
        {
            T res = base.GetAttribute<T>();
            if (res != null)
                return res;

            if (BaseMethod != null)
                return BaseMethod.GetAttribute<T>();

            return null;
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

        private bool? _isDeclarableFunction;

        public bool IsDeclarableFunction
        {
            get
            {
                if (_isDeclarableFunction == null)
                    // TODO by manski: For some strange reasons "IsFunctionAllowed" must be the last element in the
                    //   condition. Explore this issue further as this might be a bug or document the behaviour.
                    _isDeclarableFunction = !IsConstructor && !IsListenerAdder
                        && !IsListenerRemover && !IsOperatorOverload && IsFunctionAllowed(this);

                return (bool) _isDeclarableFunction;
            }
        }

        /// <summary>
        /// Denotes whether this method is a constructor.
        /// </summary>
        public bool IsConstructor
        {
            get { return (NativeName == ContainingClass.Name); }
        }

        /// <summary>
        /// Denotes whether this method describes an operator (such as <c>==</c> or <c>&lt;=</c>).
        /// </summary>
        public bool IsOperatorOverload
        {
            get
            {
                return (NativeName.StartsWith("operator")
                    && !Char.IsLetterOrDigit(NativeName["operator".Length]));
            }
        }

        /// <summary>
        /// Denotes whether this method is used to add an event listener.
        /// </summary>
        public bool IsListenerAdder
        {
            get
            {
                return (NativeName.StartsWith("add")
                        && NativeName.EndsWith("Listener")
                        && _parameters.Count == 1
                        && _parameters[0].Type.HasWrapType(WrapTypes.NativeDirector));
            }
        }

        /// <summary>
        /// Denotes whether this method is used to remove an event listener.
        /// </summary>
        public bool IsListenerRemover
        {
            get
            {
                return (NativeName.StartsWith("remove")
                        && NativeName.EndsWith("Listener")
                        && _parameters.Count == 1
                        && _parameters[0].Type.HasWrapType(WrapTypes.NativeDirector));
            }
        }

        /// <summary>
        /// Denotes whether this method is a property accessor (i.e. getter or setter method).
        /// </summary>
        public bool IsProperty
        {
            get { return IsPropertyGetAccessor || IsSetProperty; }
        }

        private bool? _isPropertyGetAccessor;
        /// <summary>
        /// Denotes whether this method a get accessor (getter) for a property.
        /// </summary>
        public bool IsPropertyGetAccessor
        {
            get
            {
                if (_isPropertyGetAccessor == null)
                {
                    if (IsOverriding)
                    {
                        // Check this before checking possible attributes
                        _isPropertyGetAccessor = BaseMethod.IsPropertyGetAccessor;
                    }
                    else if (IsConstructor || MemberTypeName == "void" || _parameters.Count > 0)
                    {
                        // Check this before checking possible attributes
                        _isPropertyGetAccessor = false;
                    }
                    else if (HasAttribute<PropertyAttribute>())
                        _isPropertyGetAccessor = true;
                    else if (HasAttribute<MethodAttribute>())
                        _isPropertyGetAccessor = false;
                    else
                    {
                        // Auto-detected whether this method is a property accessor
                        bool? ret = CheckFunctionForGetProperty();
                        if (ret != null)
                        {
                            _isPropertyGetAccessor = ret;
                        } else
                        {
                            string name = HasAttribute<RenameAttribute>() ? GetAttribute<RenameAttribute>().Name : NativeName;

                            if (name.StartsWith("get") && Char.IsUpper(name[3]))
                            {
                                string pname = name.Substring(3);
                                //check if property name collides with a nested type
                                AbstractTypeDefinition type = ContainingClass.GetNestedType(pname, false);
                                if (type != null)
                                    _isPropertyGetAccessor = false;
                                else
                                {
                                    //check if property name collides with a method
                                    MemberMethodDefinition func = ContainingClass.GetMethodByNativeName(Char.ToLower(pname[0]) + pname.Substring(1), true, false);
                                    if (func != null && !func.HasAttribute<RenameAttribute>())
                                        _isPropertyGetAccessor = false;
                                    else
                                        _isPropertyGetAccessor = true;
                                }
                            }
                            else
                                _isPropertyGetAccessor = false;
                        }
                    }
                }

                return (bool) _isPropertyGetAccessor;
            }
        }

        private bool? _isSetProperty;

        public bool IsSetProperty
        {
            get
            {
                if (_isSetProperty == null)
                {
                    if (IsOverriding)
                        _isSetProperty = BaseMethod.IsSetProperty;
                    else if (IsConstructor || MemberTypeName != "void" || _parameters.Count != 1)
                    {
                        _isSetProperty = false;
                    }
                    else if (HasAttribute<PropertyAttribute>())
                        _isSetProperty = true;
                    else if (HasAttribute<MethodAttribute>())
                        _isSetProperty = false;
                    else
                    {
                        string name = HasAttribute<RenameAttribute>() ? GetAttribute<RenameAttribute>().Name : NativeName;

                        if (name.StartsWith("set") && Char.IsUpper(name[3]))
                        {
                            // Check to see if there is a "get" function
                            MemberMethodDefinition func = ContainingClass.GetMethodByNativeName("get" + name.Substring(3), false, false);
                            _isSetProperty = (func != null && func.IsPropertyGetAccessor && func.MemberTypeName == Parameters[0].TypeName
                                              && (!ContainingClass.AllowVirtuals || (func.IsVirtual == IsVirtual && func.IsOverriding == IsOverriding)));
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
                throw new InvalidOperationException("Wrong element; expected 'function'.");

            Parameters = _parameters.AsReadOnly();

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

            IsConstMethod = bool.Parse(elem.GetAttribute("const"));
            this._isConst = (" " + Definition.Substring(0, Definition.IndexOf(NativeName))).Contains(" const ");
        }

        protected override void InterpretChildElement(XmlElement child)
        {
            if (child.Name != "parameters")
                throw new Exception("Unknown child of function: '" + child.Name + "'");

            int count = 1;
            foreach (XmlElement param in child.ChildNodes)
            {
                ParamDefinition paraDef = new ParamDefinition(MetaDef, param);
                if (paraDef.Name == null && (paraDef.TypeName != "void" || paraDef.PassedByType != PassedByType.Value))
                {
                    // Auto-generate a name for unnamed parameters, unless the parameter is of type "void"
                    // (but not "void&" or "void*) in which case the parameter is simply ignored.
                    paraDef.Name = "param" + count;
                }

                if (paraDef.Name != null)
                {
                    paraDef.Function = this;
                    _parameters.Add(paraDef);
                }
                count++;
            }
        }
    

        protected virtual bool CheckFunctionForGetProperty()
        {
            if (HasAttribute<CustomIncDeclarationAttribute>() || HasAttribute<CustomCppDeclarationAttribute>())
            {
                return false;
            }

            string name = this.GetRenameName();

            if (MemberTypeName == "bool"
                && ((name.StartsWith("is") && Char.IsUpper(name[2]) && MetaDef.CodeStyleDef.AllowIsInPropertyName) 
                    || (name.StartsWith("has") && Char.IsUpper(name[3])))
                && _parameters.Count == 0)
            {
                return true;
            }

            if (!MemberType.IsValueType
                && (MemberType.IsSharedPtr || MemberType is DefTemplateOneType || MemberType is DefTemplateTwoTypes))
            {
                return false;
            }

            if (MemberType.HasAttribute<ReturnOnlyByMethodAttribute>())
            {
                // Invalid type for a property
                return false;
            }

            string propName;
            if (name.StartsWith("get") && Char.IsUpper(name[3]))
            {
                propName = name.Substring(3);
            } else if (name.StartsWith("is") && Char.IsUpper(name[2]))
            {
                propName = name.Substring(2);
            } else
            {
            // Not a valid getter prefix.
                return false;
            }

            // Check if the property's name collides with the name of a nested type. In this 
            // case we can't convert the method into a property.
            AbstractTypeDefinition type = ContainingClass.GetNestedType(propName, false);
            if (type != null) {
                return false;
            } 

            // Check if the property's name collides with the name of a method. In this 
            // case we can't convert the method into a property.
            MemberMethodDefinition method = ContainingClass.GetMethodByCLRName(propName, true, false);

            return (method == null);
        }
    }
}