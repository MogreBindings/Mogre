using System.Xml;
using AutoWrap.Meta;

namespace AutoWrap.Mogre
{
    internal class MogreNamespaceDefinition : NamespaceDefinition
    {
        public MogreNamespaceDefinition(MetaDefinition metaDef, XmlElement elem, string managedRootNamespaceName)
            : base(metaDef, elem, managedRootNamespaceName)
        {
        }

        public override T FindType<T>(string name, bool raiseException = true)
        {
            if (name == "DisplayString")
                return (T) (object) new DefUtfString();

            return base.FindType<T>(name, raiseException);
        }
    }
}