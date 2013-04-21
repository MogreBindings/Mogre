using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefStdSet : DefStdList
    {
        public override string STLContainer
        {
            get { return "Set"; }
        }

        public override string FullSTLContainerTypeName
        {
            get { return "STLSet<" + TypeMembers[0].MemberTypeCLRName + ", " + TypeMembers[0].MemberTypeNativeName + ">"; }
        }

        public new static TypedefDefinition CreateExplicitType(TypedefDefinition typedef)
        {
            return new DefStdSet(typedef.MetaDef, typedef.Element);
        }

        public DefStdSet(MetaDefinition metaDef, XmlElement elem)
            : base(metaDef, elem)
        {
        }
    }
}