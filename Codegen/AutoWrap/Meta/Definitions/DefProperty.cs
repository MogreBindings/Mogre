namespace AutoWrap.Meta
{
    /// <summary>
    /// Represents a CLR property (inside of a CLR class).
    /// </summary>
    public class DefProperty : ITypeMember
    {
        /// <summary>
        /// Either the get or the set accessor method of this property.
        /// </summary>
        public virtual DefFunction Function
        {
            get { return (CanRead) ? GetterFunction : SetterFunction; }
        }

        public DefFunction SetterFunction;
        public DefFunction GetterFunction;

        /// <summary>
        /// The name of this property.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Denotes whether this property can be read.
        /// </summary>
        public bool CanRead
        {
            get { return GetterFunction != null; }
        }

        /// <summary>
        /// Denotes whether this property can be set.
        /// </summary>
        public bool CanWrite
        {
            get { return SetterFunction != null; }
        }

        public bool IsGetterVirtual
        {
            get { return GetterFunction.VirtualType != VirtualType.NonVirtual; }
        }

        public bool IsSetterVirtual
        {
            get { return SetterFunction.VirtualType != VirtualType.NonVirtual; }
        }

        #region ITypeMember Implementations

        public DefType MemberType
        {
            get { return (CanRead) ? GetterFunction.Type : SetterFunction.Parameters[0].Type; }
        }

        /// <summary>
        /// The name of this member's type - valid for both C++ and C++/CLI. May be different from 
        /// <c>Type.Name</c> when the type is an inner type (like <c>HardwareBuffer::Usage</c>) or 
        /// when the type is a const type. No namepsace name will be included (as in 
        /// <see cref="MemberTypeCLRName"/> and <see cref="MemberTypeNativeName"/>).
        /// </summary>
        public string MemberTypeName { get; set; }

        /// <summary>
        /// The fully qualified name of this member's CLR type (i.e. with CLR (dest) namespace).
        /// </summary>
        public virtual string MemberTypeCLRName
        {
            get { return MemberType.GetCLRTypeName(this); }
        }

        /// <summary>
        /// The fully qualified name of this member's native type (i.e. with native (source) namespace).
        /// </summary>
        public string MemberTypeNativeName
        {
            get { return (this as ITypeMember).MemberType.GetNativeTypeName((this as ITypeMember).IsConst, (this as ITypeMember).PassedByType); }
        }
        
        /// <summary>
        /// Denotes how this member's type is passed (i.e. as copy or reference).
        /// </summary>
        public PassedByType PassedByType { get; set; }
        
        public DefClass ContainingClass
        {
            get { return (CanRead) ? GetterFunction.Class : SetterFunction.Class; }
        }

        public bool IsConst
        {
            get { return (CanRead) ? GetterFunction.IsConst : SetterFunction.Parameters[0].IsConst; }
        }

        public bool HasAttribute<T>() where T : AutoWrapAttribute
        {
            return Function.HasAttribute<T>();
        }

        public T GetAttribute<T>() where T : AutoWrapAttribute
        {
            return Function.GetAttribute<T>();
        }

        #endregion

        public DefProperty(string name)
        {
            Name = name;
        }

        public DefProperty Clone()
        {
            return (DefProperty)MemberwiseClone();
        }
    }
}