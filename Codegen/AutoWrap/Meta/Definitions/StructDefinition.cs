using System;
using System.Xml;

namespace AutoWrap.Meta
{
    public class StructDefinition
        : ClassDefinition
    {
        public StructDefinition(XmlElement elem) : base(elem)
        {
            if (elem.Name != "struct")
                throw new Exception("Not struct element");
        }
    }
}