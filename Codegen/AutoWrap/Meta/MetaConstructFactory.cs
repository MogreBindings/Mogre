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
  
        public virtual ClassDefinition CreateClass(XmlElement elem)
        {
            return new ClassDefinition(_metaDef, elem);
        }
          
        public virtual StructDefinition CreateStruct(XmlElement elem)
        {
            return new StructDefinition(_metaDef, elem);
        }

        public virtual TypedefDefinition CreateTypedef(XmlElement elem)
        {
            return new TypedefDefinition(_metaDef, elem);
        }

        public virtual EnumDefinition CreateEnum(XmlElement elem)
        {
            return new EnumDefinition(_metaDef, elem);
        }

        /// <summary>
        /// Creates an instance of this class from the specified xml element. This method will
        /// create an instance from an apropriate subclass (e.g. <see cref="ClassDefinition"/> for a class).
        /// </summary>
        /// <returns>Returns the new instance or "null" in case of a global variable.</returns>
        public AbstractTypeDefinition CreateType(XmlElement elem) 
        {
            switch (elem.Name)
            {
                case "class":
                    return CreateClass(elem);
                case "struct":
                    return CreateStruct(elem);
                case "typedef":
                    return CreateTypedef(elem);
                case "enumeration":
                    return CreateEnum(elem);
                case "variable":
                    //It's global variables, ignore them
                    return null;
                default:
                    throw new InvalidOperationException("Type unknown: '" + elem.Name + "'");
            }
        }
    }
}