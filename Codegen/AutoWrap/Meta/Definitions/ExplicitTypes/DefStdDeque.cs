using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefStdDeque : DefStdVector
    {
        public override string STLContainer
        {
            get { return "Deque"; }
        }

        public override string FullSTLContainerTypeName
        {
            get { return "STLDeque<" + TypeMembers[0].MemberTypeCLRName + ", " + TypeMembers[0].MemberTypeNativeName + ">"; }
        }

        public new static TypedefDefinition CreateExplicitType(TypedefDefinition typedef)
        {
            return new DefStdDeque(typedef.MetaDef, typedef.Element);
        }

        public DefStdDeque(MetaDefinition metaDef, XmlElement elem)
            : base(metaDef, elem)
        {
        }
    }
}