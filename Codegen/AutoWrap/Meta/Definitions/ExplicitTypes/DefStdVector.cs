using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefStdVector : DefStdList
    {
        public override string STLContainer
        {
            get { return "Vector"; }
        }

        public override string FullSTLContainerTypeName
        {
            get { return "STLVector<" + TypeMembers[0].MemberTypeCLRName + ", " + TypeMembers[0].MemberTypeNativeName + ">"; }
        }

        public override string NativeCallConversionFunction
        {
            get { return "GetArrayFromVector"; }
        }

        public new static TypedefDefinition CreateExplicitType(TypedefDefinition typedef)
        {
            return new DefStdVector(typedef.NameSpace, typedef.Element);
        }

        public DefStdVector(NamespaceDefinition nsDef, XmlElement elem)
            : base(nsDef, elem)
        {
        }
    }
}