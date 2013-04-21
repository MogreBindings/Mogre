using AutoWrap.Meta;
using System.Xml;

namespace AutoWrap.Mogre
{
    internal class MogreConstructFactory : MetaConstructFactory
    {
        public override NamespaceDefinition CreateNamespace(XmlElement elem, string managedRootNamespaceName)
        {
            return new MogreNamespaceDefinition(MetaDef, elem, managedRootNamespaceName);
        }
    }
}