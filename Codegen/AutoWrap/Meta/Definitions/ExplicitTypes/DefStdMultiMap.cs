using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefStdMultiMap : DefStdMap
    {
        public override string STLContainer
        {
            get { return "MultiMap"; }
        }

        public override string FullSTLContainerTypeName
        {
            get { return "STLMultiMap<" + TypeMembers[0].MemberTypeCLRName + ", " + TypeMembers[1].MemberTypeCLRName + ", " + TypeMembers[0].MemberTypeNativeName + ", " + TypeMembers[1].MemberTypeNativeName + ">"; }
        }

        //public override string GetCLRTypeName(ITypeMember m)
        //{
        //    switch (m.PassedByType)
        //    {
        //        case PassedByType.Reference:
        //            return "Collections::Generic::SortedList<" + TypeMembers[0].CLRTypeName + ", Collections::Generic::List<" + TypeMembers[1].CLRTypeName + ">^>^";
        //        case PassedByType.PointerPointer:
        //        case PassedByType.Value:
        //        case PassedByType.Pointer:
        //        default:
        //            throw new Exception("Unexpected");
        //    }
        //}

        //public override string GetNativeCallConversion(string expr, ITypeMember m)
        //{
        //    switch (m.PassedByType)
        //    {
        //        case PassedByType.Reference:
        //            return "GetSortedListFromMultiMap<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ", " + FullNativeName + ">( " + expr + ")";
        //        case PassedByType.PointerPointer:
        //        case PassedByType.Value:
        //        case PassedByType.Pointer:
        //        default:
        //            throw new Exception("Unexpected");
        //    }
        //}

        public new static TypedefDefinition CreateExplicitType(TypedefDefinition typedef)
        {
            return new DefStdMultiMap(typedef.Element);
        }

        public DefStdMultiMap(XmlElement elem)
            : base(elem)
        {
        }
    }
}