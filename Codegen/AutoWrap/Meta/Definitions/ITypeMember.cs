namespace AutoWrap.Meta
{
    /// <summary>
    /// Describes a member of a class (or struct) or something in a class (or struct) context. 
    /// This interface is implemented by: <see cref="DefTypeMember"/>, <see cref="DefProperty"/>, 
    /// <see cref="DefParam"/>, and <see cref="DefMember"/>.
    /// </summary>
    public interface ITypeMember
    {
        /// <summary>
        /// The member's type. For fields and properties this is the data type.
        /// For methods this is the return type.
        /// </summary>
        DefType MemberType { get; }

        /// <summary>
        /// The name of this member's type - valid for both C++ and C++/CLI. May be different from 
        /// <c>Type.Name</c> when the type is an inner type (like <c>HardwareBuffer::Usage</c>) or 
        /// when the type is a const type. No namepsace name will be included (as in 
        /// <see cref="MemberTypeCLRName"/> and <see cref="MemberTypeNativeName"/>).
        /// </summary>
        string MemberTypeName { get; }

        /// <summary>
        /// The fully qualified name of this member's CLR type (i.e. with CLR (dest) namespace).
        /// </summary>
        string MemberTypeCLRName { get; }

        /// <summary>
        /// The fully qualified name of this member's native type (i.e. with native (source) namespace).
        /// </summary>
        string MemberTypeNativeName { get; }

        /// <summary>
        /// Denotes how this member's type is passed (i.e. as copy or reference).
        /// </summary>
        PassedByType PassedByType { get; }

        /// <summary>
        /// The class this member is contained in.
        /// </summary>
        DefClass ContainingClass { get; }

        /// <summary>
        /// Indicates whether this member is C++ <c>const</c>.
        /// </summary>
        bool IsConst { get; }

        bool HasAttribute<T>() where T : AutoWrapAttribute;
        T GetAttribute<T>() where T : AutoWrapAttribute;
    }

    public static class ITypeMemberExtensions
    {
        /// <summary>
        /// Indicates whether this type is handled. "Handled" means that the type can be
        /// used as parameter or return type in the generated code. Methods, properties,
        /// and fields using an unhandled type wont be included in the generated code.
        /// </summary>
        public static bool IsTypeHandled(this ITypeMember m)
        {
            if (m.MemberType.IsIgnored)
                return false;

            if (m.MemberType is DefClass && ((DefClass)m.MemberType).IsSingleton)
                return false;

            return (m.MemberTypeName != "UserDefinedObject");
        }
    }
}