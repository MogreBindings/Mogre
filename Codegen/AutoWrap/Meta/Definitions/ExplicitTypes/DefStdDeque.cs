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

        public DefStdDeque(NamespaceDefinition nsDef, ClassDefinition surroundingClass, XmlElement elem)
            : base(nsDef, surroundingClass, elem)
        {
        }
    }
}