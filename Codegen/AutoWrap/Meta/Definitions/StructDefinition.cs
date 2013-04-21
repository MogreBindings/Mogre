using System;
using System.Xml;

namespace AutoWrap.Meta
{
    public class StructDefinition
        : ClassDefinition
    {
        public StructDefinition(NamespaceDefinition nsDef, XmlElement elem)
            : base(nsDef, elem)
        {
            if (elem.Name != "struct")
                throw new Exception("Not struct element");
        }
    }
}