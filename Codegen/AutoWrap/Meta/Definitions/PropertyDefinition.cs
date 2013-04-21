namespace AutoWrap.Meta
{
    /// <summary>
    /// Represents a CLR property (inside of a CLR class).
    /// </summary>
    public class PropertyDefinition : ITypeMember
    {
        /// <summary>
        /// The name of this property.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Returns one of the accessor functions to be used to identify certain attribues
        /// of this property (like protection level).
        /// </summary>
        private MemberMethodDefinition _accessorFunction
        {
            get { return (CanRead) ? GetterFunction : SetterFunction; }
        }

        public MemberMethodDefinition SetterFunction;
        public MemberMethodDefinition GetterFunction;

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

        public ProtectionLevel ProtectionLevel
        {
            get { return _accessorFunction.ProtectionLevel; }
        }
    
        public bool IsVirtual
        {
            get { return _accessorFunction.IsVirtual; }
        }
    
        public bool IsAbstract
        {
            get { return _accessorFunction.IsAbstract; }
        }
    
        public bool IsStatic
        {
            get { return _accessorFunction.IsStatic; }
        }
    

        #region ITypeMember Implementations

        public AbstractTypeDefinition MemberType
        {
            get { return (CanRead) ? GetterFunction.MemberType : SetterFunction.Parameters[0].Type; }
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
        
        public ClassDefinition ContainingClass
        {
            get { return _accessorFunction.Class; }
        }

        public bool IsConst
        {
            get { return (CanRead) ? GetterFunction.IsConst : SetterFunction.Parameters[0].IsConst; }
        }

        public bool HasAttribute<T>() where T : AutoWrapAttribute
        {
            return _accessorFunction.HasAttribute<T>();
        }

        public T GetAttribute<T>() where T : AutoWrapAttribute
        {
            return _accessorFunction.GetAttribute<T>();
        }

        #endregion

        public PropertyDefinition(string name)
        {
            Name = name;
        }

        public PropertyDefinition Clone()
        {
            return (PropertyDefinition)MemberwiseClone();
        }
    
        /// <summary>
        /// Checks whether this property is contained in the specified class or any of its base classes.
        /// </summary>
        /// <param name="clazz">the class to check</param>
        /// <param name="allowInheritedSignature">if this is <c>false</c> only the specified class will be
        /// checked for the property. Otherwise all base classes will be checked as well.</param>
        public bool IsContainedIn(ClassDefinition clazz, bool allowInBaseClass)
        {
            return clazz.ContainsFunctionSignature(_accessorFunction.Signature, allowInBaseClass);
        }
    }
}