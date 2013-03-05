namespace AutoWrap.Meta
{
    public interface ITypeMember
    {
        /// <summary>
        /// The name of this member's type. For fields and properties this is the data type.
        /// For methods this is the return type.
        /// </summary>
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

    public static class ITypeMemberExtensions
    {
        /// <summary>
        /// Indicates whether this type is handled. "Handled" means that the type can be
        /// used as parameter or return type in the generated code. Methods, properties,
        /// and fields using an unhandled type wont be included in the generated code.
        /// </summary>
        public static bool IsTypeHandled(this ITypeMember m)
        {
            if (m.Type.IsIgnored)
                return false;

            if (m.Type is DefClass && ((DefClass)m.Type).IsSingleton)
                return false;

            return (m.TypeName != "UserDefinedObject");
        }
    }
}