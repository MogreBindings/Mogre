using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefHashedVector : DefStdVector
    {
        public override string STLContainer
        {
            get { return "HashedVector"; }
        }

        public override string FullSTLContainerTypeName
        {
            get { return "STLHASHEDVECTOR<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ">"; }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefHashedVector(typedef.Element);
        }

        public DefHashedVector(XmlElement elem)
            : base(elem)
        {
        }
    }
}