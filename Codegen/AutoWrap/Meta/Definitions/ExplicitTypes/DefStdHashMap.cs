using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefStdHashMap : DefStdMap
    {
        public override string STLContainer
        {
            get { return "HashMap"; }
        }

        public override string FullSTLContainerTypeName
        {
            get { return "STLHashMap<" + TypeMembers[0].MemberTypeCLRName + ", " + TypeMembers[1].MemberTypeCLRName + ", " + TypeMembers[0].MemberTypeNativeName + ", " + TypeMembers[1].MemberTypeNativeName + ">"; }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefStdHashMap(typedef.Element);
        }

        public DefStdHashMap(XmlElement elem)
            : base(elem)
        {
        }
    }
}