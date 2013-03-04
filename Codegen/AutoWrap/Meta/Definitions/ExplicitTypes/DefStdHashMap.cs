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
            get { return "STLHashMap<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ", " + TypeMembers[1].NativeTypeName + ">"; }
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