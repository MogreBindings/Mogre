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
}