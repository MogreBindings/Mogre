using System;
using System.Xml;

namespace AutoWrap.Meta
{
    public class DefParam : AttributeHolder, ITypeMember
    {
        string ITypeMember.MemberTypeName
        {
            get { return TypeName; }
        }

        PassedByType ITypeMember.PassedByType
        {
            get { return PassedByType; }
        }

        DefClass ITypeMember.ContainingClass
        {
            get { return Function.Class; }
        }

        DefType ITypeMember.MemberType
        {
            get { return Type; }
        }

        bool ITypeMember.HasAttribute<T>()
        {
            return HasAttribute<T>();
        }

        T ITypeMember.GetAttribute<T>()
        {
            return GetAttribute<T>();
        }

        private string _clrTypeName;
        public virtual string MemberTypeCLRName
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

        public virtual string MemberTypeNativeName
        {
            get { return (this as ITypeMember).MemberType.GetNativeTypeName(IsConst, (this as ITypeMember).PassedByType); }
        }

        private DefType _type;
        public virtual DefType Type
        {
            get
            {
                if (_type == null)
                {
                    if (Container != "")
                    {
                        _type = CreateExplicitContainerType(Container, ContainerKey, (ContainerValue != "") ? ContainerValue : TypeName);
                        _type.SurroundingClass = Function.Class;
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

        private bool? _isConst;
        public bool IsConst
        {
            get
            {
                if (_isConst == null)
                    _isConst = (_elem.ChildNodes[0] as XmlElement).GetAttribute("const") == "true";

                return (bool) _isConst;
            }
        }

        private string _typename;
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

                return _elem.ChildNodes[2].InnerText.Replace("\n", "").Trim();
            }
        }

        public DefParam(XmlElement elem)
        {
            _elem = elem;
            PassedByType = (PassedByType) Enum.Parse(typeof (PassedByType), elem.GetAttribute("passedBy"), true);
        }

        public DefParam(ITypeMember m, string name)
        {
            _name = name;
            _type = m.MemberType;
            _typename = m.MemberTypeName; 
            PassedByType = m.PassedByType;
            _isConst = m.IsConst;
        }
    }
}