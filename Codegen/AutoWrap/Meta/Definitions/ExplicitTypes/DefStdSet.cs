﻿using System.Xml;

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

        public DefStdSet(NamespaceDefinition nsDef, ClassDefinition surroundingClass, XmlElement elem)
            : base(nsDef, surroundingClass, elem)
        {
        }
    }
}