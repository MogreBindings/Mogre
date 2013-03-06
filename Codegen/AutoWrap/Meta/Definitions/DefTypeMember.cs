using System;

namespace AutoWrap.Meta
{
    internal class DefTypeMember : ITypeMember
    {
        string ITypeMember.MemberTypeName
        {
            get { return _type.Name; }
        }

        PassedByType ITypeMember.PassedByType
        {
            get { return _passed; }
        }

        DefClass ITypeMember.ContainingClass
        {
            get { throw new Exception("Unexpected"); }
        }

        DefType ITypeMember.MemberType 
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

        public virtual string MemberTypeNativeName
        {
            get { return (this as ITypeMember).MemberType.GetNativeTypeName(IsConst, (this as ITypeMember).PassedByType); }
        }

        private string _clrTypeName;
        public virtual string MemberTypeCLRName 
        {
            get
            {
                if (_clrTypeName == null)
                    _clrTypeName = (this as ITypeMember).MemberType.GetCLRTypeName(this);

                return _clrTypeName;
            }
        }

        protected DefType _type;
        protected PassedByType _passed;

        public DefTypeMember(DefType type, PassedByType passed, bool isConst)
        {
            _type = type;
            _passed = passed;
            _isConst = isConst;
        }
    }
}