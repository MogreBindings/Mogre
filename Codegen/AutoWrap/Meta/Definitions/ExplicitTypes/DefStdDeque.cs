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
            get { return "STLDeque<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ">"; }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefStdDeque(typedef.Element);
        }

        public DefStdDeque(XmlElement elem)
            : base(elem)
        {
        }
    }
}