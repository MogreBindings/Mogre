using System;
using System.Xml;

namespace AutoWrap.Meta
{
    public class MemberFieldDefinition : AbstractMemberDefinition
    {
        public override bool IsProperty
        {
            get { return false; }
        }

        public override bool IsConst
        {
            get { return Definition.StartsWith("const "); }
        }

        private string _fullNativeName;

        public virtual string FullNativeName
        {
            get { return _fullNativeName; }
        }

        private string _arraySize;

        public virtual string ArraySize
        {
            get { return _arraySize; }
        }

        public virtual bool IsNativeArray
        {
            get { return (_arraySize != null); }
        }

        public MemberFieldDefinition(XmlElement elem)
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
                string size = _fullNativeName.Substring(index + 1, last - index - 1);
                if (size != "")
                {
                    _arraySize = size;
                    _fullNativeName = _fullNativeName.Substring(0, index);
                }
            }
        }
    }
}