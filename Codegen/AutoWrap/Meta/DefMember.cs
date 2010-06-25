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
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AutoWrap.Meta
{
    public interface ITypeMember
    {
        string TypeName { get; }
        PassedByType PassedByType { get; }
        DefClass Class { get; }
        DefType Type { get; }
        string CLRTypeName { get; }
        string NativeTypeName { get; }
        bool IsConst { get; }
        bool HasAttribute<T>() where T : AutoWrapAttribute;
        T GetAttribute<T>() where T : AutoWrapAttribute;
    }

    public abstract class DefMember : AttributeHolder, ITypeMember
    {
        string ITypeMember.TypeName
        {
            get { return this.TypeName; }
        }
        PassedByType ITypeMember.PassedByType
        {
            get { return this.PassedByType; }
        }
        DefClass ITypeMember.Class
        {
            get { return this.Class; }
        }
        DefType ITypeMember.Type
        {
            get { return this.Type; }
        }
        bool ITypeMember.HasAttribute<T>()
        {
            return HasAttribute<T>();
        }
        T ITypeMember.GetAttribute<T>()
        {
            return this.GetAttribute<T>();
        }

        public abstract bool IsProperty { get; }

        public virtual bool IsConst
        {
            get { return false; }
        }

        public virtual bool IsIgnored
        {
            get
            {
                //TODO: Find a more general way to handle templates and get rid of this hacky way
                if (Definition.StartsWith("Controller<"))
                    return true;

                return (Type.IsIgnored || this.HasAttribute<IgnoreAttribute>());
            }
        }

        string _clrTypeName;
        public virtual string CLRTypeName
        {
            get
            {
                if (_clrTypeName == null)
                    _clrTypeName = Type.GetCLRTypeName(this);

                return _clrTypeName;
            }
        }

        public virtual string NativeTypeName
        {
            get { return (this as ITypeMember).Type.GetNativeTypeName(IsConst, (this as ITypeMember).PassedByType); }
        }

        DefType _type = null;
        public virtual DefType Type
        {
            get
            {
                if (_type == null)
                {
                    if (Container != "")
                    {
                        _type = CreateExplicitContainerType(Container, ContainerKey, (ContainerValue != "") ? ContainerValue : TypeName);
                        _type.ParentClass = Class;
                    }
                    else
                        _type = Class.FindType<DefType>(TypeName, false);
                }

                return _type;
            }
        }

        protected virtual void InterpretChildElement(XmlElement child)
        {
            throw new Exception("Unknown child of member: '" + child.Name + "'");
        }

        protected XmlElement _elem;

        public DefClass Class;

        protected string _name;
        public virtual string Name
        {
            get { return _name; }
        }

        protected string _container;
        public virtual string Container
        {
            get { return _container; }
        }

        protected string _containerKey;
        public virtual string ContainerKey
        {
            get { return _containerKey; }
        }

        protected string _containerValue;
        public virtual string ContainerValue
        {
            get { return _containerValue; }
        }

        public string TypeName = null;
        public string Definition;
        public ProtectionType ProtectionType;
        public PassedByType PassedByType;

        public virtual bool IsVoid
        {
            get { return (TypeName == "void" || TypeName == "const void")
                && PassedByType == PassedByType.Value; }
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

        public bool IsStatic
        {
            get { return _elem.GetAttribute("static") == "yes"; }
        }

        public XmlElement Element
        {
            get { return _elem; }
        }

        public DefMember(XmlElement elem)
        {
            this._elem = elem;
            this.ProtectionType = DefType.GetProtectionEnum(elem.GetAttribute("protection"));
            this.PassedByType = (PassedByType)Enum.Parse(typeof(PassedByType), elem.GetAttribute("passedBy"), true);

            foreach (XmlElement child in elem.ChildNodes)
            {
                switch (child.Name)
                {
                    case "name":
                        _name = child.InnerText;
                        break;
                    case "type":
                        this.TypeName = child.InnerText;
                        this._container = child.GetAttribute("container");
                        this._containerKey = child.GetAttribute("containerKey");
                        this._containerValue = child.GetAttribute("containerValue");
                        break;
                    case "definition":
                        this.Definition = child.InnerText;
                        break;
                    default:
                        InterpretChildElement(child);
                        break;
                }
            }
        }
    }

    public enum VirtualType { Virtual, NonVirtual, PureVirtual }
    public enum PassedByType { Value, Reference, Pointer, PointerPointer }

    public class DefFunction : DefMember
    {
        public List<DefParam> Parameters = new List<DefParam>();
        public VirtualType VirtualType;

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

                return (bool)_isConst;
            }
        }

        private bool _isConstFunctionCall;
        public bool IsConstFunctionCall
        {
            get { return _isConstFunctionCall; }
        }

        private string _signature = null;
        public string Signature
        {
            get
            {
                if (_signature == null)
                {
                    _signature = IsVirtual.ToString() + ProtectionType + Name;
                    foreach (DefParam param in Parameters)
                    {
                        _signature += "|" + param.TypeName + "#" + param.PassedByType + "#" + param.Container + "#" + param.Array;
                    }
                }

                return _signature;
            }
        }

        private string _signatureNameAndParams = null;
        public string SignatureNameAndParams
        {
            get
            {
                if (_signatureNameAndParams == null)
                {
                    _signatureNameAndParams = Name;
                    foreach (DefParam param in Parameters)
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

                foreach (DefParam param in Parameters)
                {
                    if (param.Type.IsIgnored)
                        return true;
                }

                return false;
            }
        }

        private bool? _isOverride = null;
        private DefFunction _baseFunc;
        public bool IsOverride
        {
            get
            {
                if (_isOverride == null)
                {
                    if (IsVirtual && !Class.IsInterface && Class.BaseClass != null)
                    {
                        if (Class.BaseClass.ContainsFunctionSignature(Signature, true, out _baseFunc)
                            || Class.BaseClass.ContainsInterfaceFunctionSignature(Signature, true, out _baseFunc))
                            _isOverride = true;
                        else
                            _isOverride = false;
                    }
                    else
                        _isOverride = false;
                }

                return (bool)_isOverride; 
            }
        }

        public DefFunction BaseFunction
        {
            get
            {
                if (_baseFunc != null)
                    return _baseFunc;

                if (IsOverride)
                    return _baseFunc;

                if (Class.ContainsInterfaceFunctionSignature(Signature, true, out _baseFunc))
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
            else
                return false;
        }

        public override T GetAttribute<T>()
        {
            T res = base.GetAttribute<T>();
            if (res != null)
                return res;

            if (BaseFunction != null)
                return BaseFunction.GetAttribute<T>();
            else
                return default(T);
        }

        private bool? _isVirtualInterfaceMethod = null;
        public bool IsVirtualInterfaceMethod
        {
            get
            {
                if (_isVirtualInterfaceMethod == null)
                {
                    if (IsVirtual)
                    {
                        if (this.Class.IsInterface)
                            _isVirtualInterfaceMethod = true;
                        else
                            _isVirtualInterfaceMethod = this.Class.ContainsInterfaceFunctionSignature(this.Signature, true);
                    }
                    else
                        _isVirtualInterfaceMethod = false;
                }

                return (bool)_isVirtualInterfaceMethod;
            }
        }

        public bool IsVirtual
        {
            get { return VirtualType != VirtualType.NonVirtual; }
        }

        public bool IsAbstract
        {
            get { return VirtualType == VirtualType.PureVirtual; }
        }

        private bool? _isDeclarableFunction;
        public bool IsDeclarableFunction
        {
            get
            {
                if (_isDeclarableFunction == null)
                    _isDeclarableFunction = !(IsConstructor || IsListenerAdder || IsListenerRemover
                            || IsOperatorOverload || !AllowFunction(this) );

                return (bool)_isDeclarableFunction;
            }
        }

        public bool IsConstructor
        {
            get
            {
                return (this.Name == this.Class.Name);
            }
        }

        public bool IsOperatorOverload
        {
            get
            {
                return (this.Name.StartsWith("operator")
                        && !Char.IsLetterOrDigit(this.Name["operator".Length]));
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

        public override bool IsProperty
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
                    else if (name.StartsWith("set"))
                        return name.Substring(3);
                    else
                        return ToCamelCase(base.CLRName);
                }
                else if (IsOperatorOverload)
                    return base.CLRName;
                else
                    return ToCamelCase(base.CLRName);
            }
        }

        private bool? _isGetProperty;
        public bool IsGetProperty
        {
            get
            {
                if (_isGetProperty == null)
                {
                    if (this.IsOverride)
                    {
                        _isGetProperty = BaseFunction.IsGetProperty;
                    }
                    else if (IsConstructor || this.TypeName == "void" || this.Parameters.Count > 0)
                        _isGetProperty = false;
                    else if (this.HasAttribute<PropertyAttribute>())
                        _isGetProperty = true;
                    else if (this.HasAttribute<MethodAttribute>())
                        _isGetProperty = false;
                    else
                    {
                        bool? ret = CheckFunctionForGetProperty(this);
                        if (ret != null)
                            _isGetProperty = (bool)ret;
                        else
                        {
                            string name = HasAttribute<RenameAttribute>() ? GetAttribute<RenameAttribute>().Name : Name;

                            if (name.StartsWith("get") && Char.IsUpper(name[3]))
                            {
                                string pname = name.Substring(3);
                                //check if property name collides with a nested type
                                DefType type = Class.GetNestedType(pname, false);
                                if (type != null)
                                    _isGetProperty = false;
                                else
                                {
                                    //check if property name collides with a method
                                    DefFunction func = Class.GetFunction(Char.ToLower(pname[0]) + pname.Substring(1), true, false);
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

                return (bool)_isGetProperty;
            }
        }

        private bool? _isSetProperty;
        public bool IsSetProperty
        {
            get
            {
                if (_isSetProperty == null)
                {
                    if (this.IsOverride)
                    {
                        _isSetProperty = BaseFunction.IsSetProperty;
                    }
                    else if (IsConstructor || this.TypeName != "void" || this.Parameters.Count != 1)
                    {
                        _isSetProperty = false;
                    }
                    else if (this.HasAttribute<PropertyAttribute>())
                        _isSetProperty = true;
                    else if (this.HasAttribute<MethodAttribute>())
                        _isSetProperty = false;
                    else
                    {
                        string name = HasAttribute<RenameAttribute>() ? GetAttribute<RenameAttribute>().Name : Name;

                        if (name.StartsWith("set") && Char.IsUpper(name[3]))
                        {
                            // Check to see if there is a "get" function
                            DefFunction func = this.Class.GetFunction("get" + name.Substring(3), false, false);
                            _isSetProperty = (func != null && func.IsGetProperty && func.TypeName == this.Parameters[0].TypeName
                                            && ( !Class.AllowVirtuals || (func.IsVirtual == this.IsVirtual && func.IsOverride == this.IsOverride) ) );
                        }
                        else
                            _isSetProperty = false;
                    }
                }

                return (bool)_isSetProperty;
            }
        }

        public DefFunction(XmlElement elem)
            : base(elem)
        {
            if (elem.Name != "function")
                throw new Exception("Wrong element; expected 'function'.");

            switch (elem.GetAttribute("virt"))
            {
                case "virtual":
                    VirtualType = VirtualType.Virtual;
                    break;
                case "non-virtual":
                    VirtualType = VirtualType.NonVirtual;
                    break;
                case "pure-virtual":
                    VirtualType = VirtualType.PureVirtual;
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
                        DefParam p = new DefParam(param);
                        if (p.Name == null && (p.TypeName != "void" || p.PassedByType != PassedByType.Value))
                            p.Name = "param" + count;

                        if (p.Name != null)
                        {
                            p.Function = this;
                            this.Parameters.Add(p);
                        }
                        count++;
                    }
                    break;

                default:
                    throw new Exception("Unknown child of function: '" + child.Name + "'");
            }
        }
    }

    public class DefField : DefMember
    {
        public override bool IsProperty
        {
            get { return false; }
        }

        public override bool IsConst
        {
            get { return this.Definition.StartsWith("const "); }
        }

        private string _fullNativeName;
        public virtual string FullNativeName
        {
            get { return _fullNativeName; }
        }

        private string _arraySize = null;
        public virtual string ArraySize
        {
            get { return _arraySize; }
        }

        public virtual bool IsNativeArray
        {
            get { return (_arraySize != null); }
        }

        public DefField(XmlElement elem)
            : base(elem)
        {
            if (elem.Name != "variable")
                throw new Exception("Wrong element; expected 'variable'.");

            _fullNativeName = this.Definition.Substring(this.Definition.LastIndexOf(" ") + 1);
            if (_fullNativeName.Contains("["))
            {
                //It's native array
                int index = _fullNativeName.IndexOf("[");
                int last = _fullNativeName.IndexOf("]");
                string size = _fullNativeName.Substring(index + 1, last-index - 1);
                if (size != "")
                {
                    _arraySize = size;
                    _fullNativeName = _fullNativeName.Substring(0, index);
                }
            }
        }
    }

    public class DefParam : AttributeHolder, ITypeMember
    {
        string ITypeMember.TypeName
        {
            get { return this.TypeName; }
        }
        PassedByType ITypeMember.PassedByType
        {
            get { return this.PassedByType; }
        }
        DefClass ITypeMember.Class
        {
            get { return this.Function.Class; }
        }
        DefType ITypeMember.Type
        {
            get { return this.Type; }
        }
        bool ITypeMember.HasAttribute<T>()
        {
            return HasAttribute<T>();
        }
        T ITypeMember.GetAttribute<T>()
        {
            return this.GetAttribute<T>();
        }

        string _clrTypeName;
        public virtual string CLRTypeName
        {
            get
            {
                if (_clrTypeName == null)
                    _clrTypeName = Type.GetCLRTypeName(this);

                return _clrTypeName;
            }
        }

        private string _CLRDefaultValuePreConversion;
        public string CLRDefaultValuePreConversion
        {
            get
            {
                DefType depend;
                if (_CLRDefaultValuePreConversion == null)
                    Type.GetDefaultParamValueConversion(this, out _CLRDefaultValuePreConversion, out _CLRDefaultValue, out _CLRDefaultValuePostConversion, out depend);

                return _CLRDefaultValuePreConversion;
            }
        }

        private string _CLRDefaultValue;
        public string CLRDefaultValue
        {
            get
            {
                DefType depend;
                if (_CLRDefaultValue == null)
                    Type.GetDefaultParamValueConversion(this, out _CLRDefaultValuePreConversion, out _CLRDefaultValue, out _CLRDefaultValuePostConversion, out depend);

                return _CLRDefaultValue;
            }
        }

        private string _CLRDefaultValuePostConversion;
        public string CLRDefaultValuePostConversion
        {
            get
            {
                DefType depend;
                if (_CLRDefaultValuePostConversion == null)
                    Type.GetDefaultParamValueConversion(this, out _CLRDefaultValuePreConversion, out _CLRDefaultValue, out _CLRDefaultValuePostConversion, out depend);

                return _CLRDefaultValuePostConversion;
            }
        }

        public virtual string NativeTypeName
        {
            get { return (this as ITypeMember).Type.GetNativeTypeName(IsConst, (this as ITypeMember).PassedByType); }
        }

        DefType _type = null;
        public virtual DefType Type
        {
            get
            {
                if (_type == null)
                {
                    if (Container != "")
                    {
                        _type = CreateExplicitContainerType(Container, ContainerKey, (ContainerValue != "") ? ContainerValue : TypeName);
                        _type.ParentClass = Function.Class;
                    }
                    else
                        _type = Function.Class.FindType<DefType>(TypeName, false);
                }

                return _type;
            }
        }

        public virtual string Container
        {
            get { return (_elem.ChildNodes[0] as XmlElement).GetAttribute("container"); }
        }

        public virtual string ContainerKey
        {
            get { return (_elem.ChildNodes[0] as XmlElement).GetAttribute("containerKey"); }
        }

        public virtual string ContainerValue
        {
            get { return (_elem.ChildNodes[0] as XmlElement).GetAttribute("containerValue"); }
        }

        public virtual string Array
        {
            get { return (_elem.ChildNodes[0] as XmlElement).GetAttribute("array"); }
        }

        protected XmlElement _elem;

        public DefFunction Function;
        public PassedByType PassedByType;

        bool? _isConst;
        public bool IsConst
        {
            get
            {
                if (_isConst == null)
                    _isConst = (_elem.ChildNodes[0] as XmlElement).GetAttribute("const") == "true";

                return (bool)_isConst;
            }
        }

        string _typename = null;
        public string TypeName
        {
            get
            {
                if (_typename == null)
                {
                    _typename = _elem.ChildNodes[0].InnerText;
                    if (_typename.StartsWith(Globals.NativeNamespace + "::"))
                        _typename = _typename.Substring((Globals.NativeNamespace + "::").Length);
                }
                return _typename;
            }
        }

        private string _name;
        public string Name
        {
            get 
            {
                if (_name == null)
                {
                    if (_elem.ChildNodes.Count > 1)
                        _name = _elem.ChildNodes[1].InnerText;
                }

                return _name;
            }
            set { _name = value; }
        }

        public string DefaultValue
        {
            get
            {
                if (_elem.ChildNodes.Count < 3)
                    return null;

                return _elem.ChildNodes[2].InnerText.Replace("\n","").Trim();
            }
        }

        public DefParam(XmlElement elem)
        {
            this._elem = elem;
            this.PassedByType = (PassedByType)Enum.Parse(typeof(PassedByType), elem.GetAttribute("passedBy"), true);
        }

        public DefParam(ITypeMember m, string name)
        {
            this._name = name;
            this._type = m.Type;
            this._typename = m.TypeName;
            this.PassedByType = m.PassedByType;
            this._isConst = m.IsConst;
        }
    }

    public class DefProperty : ITypeMember
    {
        string ITypeMember.TypeName
        {
            get { return this.TypeName; }
        }
        PassedByType ITypeMember.PassedByType
        {
            get { return this.PassedByType; }
        }
        DefClass ITypeMember.Class
        {
            get { return this.Class; }
        }
        DefType ITypeMember.Type
        {
            get { return (CanRead) ? GetterFunction.Type : SetterFunction.Parameters[0].Type; }
        }
        bool ITypeMember.IsConst
        {
            get { return (CanRead) ? GetterFunction.IsConst : SetterFunction.Parameters[0].IsConst; }
        }
        bool ITypeMember.HasAttribute<T>()
        {
            return Function.HasAttribute<T>();
        }
        T ITypeMember.GetAttribute<T>()
        {
            return Function.GetAttribute<T>();
        }

        public virtual DefFunction Function
        {
            get { return (CanRead) ? GetterFunction : SetterFunction; }
        }

        string _clrTypeName;
        public virtual string CLRTypeName
        {
            get
            {
                if (_clrTypeName == null)
                    _clrTypeName = (this as ITypeMember).Type.GetCLRTypeName(this);

                return _clrTypeName;
            }
        }

        public virtual string NativeTypeName
        {
            get { return (this as ITypeMember).Type.GetNativeTypeName((this as ITypeMember).IsConst, (this as ITypeMember).PassedByType); }
        }

        public DefFunction SetterFunction;
        public DefFunction GetterFunction;

        public string Name;
        public bool CanRead;
        public bool CanWrite;
        public string TypeName;
        public PassedByType PassedByType;

        public DefProperty Clone()
        {
            return (DefProperty)this.MemberwiseClone();
        }

        public bool IsGetterVirtual
        {
            get { return GetterFunction.VirtualType != VirtualType.NonVirtual; }
        }

        public bool IsSetterVirtual
        {
            get { return SetterFunction.VirtualType != VirtualType.NonVirtual; }
        }

        public DefClass Class
        {
            get { return (CanRead) ? GetterFunction.Class : SetterFunction.Class; }
        }
    }

    class DefTypeMember : ITypeMember
    {
        string ITypeMember.TypeName
        {
            get { return _type.Name; }
        }
        PassedByType ITypeMember.PassedByType
        {
            get { return _passed; }
        }
        DefClass ITypeMember.Class
        {
            get { throw new Exception("Unexpected"); }
        }
        DefType ITypeMember.Type
        {
            get { return _type; }
        }
        bool ITypeMember.HasAttribute<T>()
        {
            return false;
        }
        T ITypeMember.GetAttribute<T>()
        {
            throw new Exception("Unexpected");
        }

        private bool _isConst;
        public virtual bool IsConst
        {
            get { return _isConst; }
        }

        public virtual string NativeTypeName
        {
            get { return (this as ITypeMember).Type.GetNativeTypeName(IsConst, (this as ITypeMember).PassedByType); }
        }

        string _clrTypeName;
        public virtual string CLRTypeName
        {
            get
            {
                if (_clrTypeName == null)
                    _clrTypeName = (this as ITypeMember).Type.GetCLRTypeName(this);

                return _clrTypeName;
            }
        }

        protected DefType _type;
        protected PassedByType _passed;

        public DefTypeMember(DefType type, PassedByType passed, bool isConst)
        {
            this._type = type;
            this._passed = passed;
            this._isConst = isConst;
        }
    }
}
