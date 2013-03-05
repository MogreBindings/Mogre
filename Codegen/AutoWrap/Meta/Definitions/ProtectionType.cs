using System;

namespace AutoWrap.Meta
{
    /// <summary>
    /// Visibility/protection level.
    /// </summary>
    public enum ProtectionType
    {
        Public,
        Private,
        Protected
    }

    public static class ProtectionTypeExtensions
    {
        /// <summary>
        /// Returns the C++/CLI protection/visibility level name for the C++ level name.
        /// </summary>
        public static string GetCLRProtectionName(this ProtectionType prot)
        {
            switch (prot)
            {
                case ProtectionType.Public:
                    return "public";
                case ProtectionType.Protected:
                    return "protected public";
                case ProtectionType.Private:
                    return "private";
                default:
                    throw new Exception("Unexpected");
            }
        }
    }
}