using System.Xml;

namespace AutoWrap.Meta
{
    /// <summary>
    /// This factory produces all the constructs (e.g. classes, namespaces, ...) used by the wrapping process.
    /// Custom implementations can be implemented by subclassing this class. Otherwise it's completely legitimate
    /// to use this class on its own.
    /// </summary>
    public class MetaConstructFactory
    {
        public virtual NamespaceDefinition CreateNamespace(MetaDefinition metaDef, XmlElement elem,
                                                           string managedRootNamespaceName)
        {
            return new NamespaceDefinition(metaDef, elem, managedRootNamespaceName);
        }
    }
}