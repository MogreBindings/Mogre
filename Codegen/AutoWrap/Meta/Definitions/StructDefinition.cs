using System;
using System.Xml;

namespace AutoWrap.Meta
{
    public class StructDefinition
        : ClassDefinition
    {
        public StructDefinition(MetaDefinition metaDef, XmlElement elem)
            : base(metaDef, elem)
        {
            if (elem.Name != "struct")
                throw new Exception("Not struct element");
        }
    }
}