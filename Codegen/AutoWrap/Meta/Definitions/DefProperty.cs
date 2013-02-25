namespace AutoWrap.Meta
{
    public class DefProperty : ITypeMember
    {
        string ITypeMember.TypeName
        {
            get { return TypeName; }
        }

        PassedByType ITypeMember.PassedByType
        {
            get { return PassedByType; }
        }

        DefClass ITypeMember.Class
        {
            get { return Class; }
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

        private string _clrTypeName;
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
            return (DefProperty) MemberwiseClone();
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
}