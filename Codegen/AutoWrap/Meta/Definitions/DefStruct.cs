using System;
using System.Xml;

namespace AutoWrap.Meta
{
    public class DefStruct : DefClass
    {
        public DefStruct(XmlElement elem)
            : base(elem)
        {
            if (elem.Name != "struct")
                throw new Exception("Not struct element");
        }
    }
}