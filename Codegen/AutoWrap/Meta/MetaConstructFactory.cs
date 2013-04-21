using System;
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
        private MetaDefinition _metaDef;

        internal MetaDefinition MetaDef
        {
            get { return _metaDef; }
            set
            {
                if (_metaDef != null)
                    throw new InvalidOperationException("Already initialized.");

                _metaDef = value;
            }
        }

        public virtual NamespaceDefinition CreateNamespace(XmlElement elem, string managedRootNamespaceName)
        {
            return new NamespaceDefinition(_metaDef, elem, managedRootNamespaceName);
        }
  
        public virtual ClassDefinition CreateClass(NamespaceDefinition nsDef, XmlElement elem)
        {
            return new ClassDefinition(nsDef, elem);
        }
          
        public virtual StructDefinition CreateStruct(NamespaceDefinition nsDef, XmlElement elem)
        {
            return new StructDefinition(nsDef, elem);
        }

        public virtual TypedefDefinition CreateTypedef(NamespaceDefinition nsDef, XmlElement elem)
        {
            return new TypedefDefinition(nsDef, elem);
        }

        public virtual EnumDefinition CreateEnum(NamespaceDefinition nsDef, XmlElement elem)
        {
            return new EnumDefinition(nsDef, elem);
        }

        /// <summary>
        /// Creates an instance of this class from the specified xml element. This method will
        /// create an instance from an apropriate subclass (e.g. <see cref="ClassDefinition"/> for a class).
        /// </summary>
        /// <returns>Returns the new instance or "null" in case of a global variable.</returns>
        public AbstractTypeDefinition CreateType(NamespaceDefinition nsDef, XmlElement elem)
        {
            switch (elem.Name)
            {
                case "class":
                    return CreateClass(nsDef, elem);
                case "struct":
                    return CreateStruct(nsDef, elem);
                case "typedef":
                    return CreateTypedef(nsDef, elem);
                case "enumeration":
                    return CreateEnum(nsDef, elem);
                case "variable":
                    //It's global variables, ignore them
                    return null;
                default:
                    throw new InvalidOperationException("Type unknown: '" + elem.Name + "'");
            }
        }
    }
}