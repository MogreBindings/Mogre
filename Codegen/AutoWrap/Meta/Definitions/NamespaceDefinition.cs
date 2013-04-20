#region GPL license
/*
 * This source file is part of the AutoWrap code generator of the
 * MOGRE project (http://mogre.sourceforge.net).
 * 
 * Copyright (C) 2006-2007 Argiris Kirtzidis
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
#endregion

using System;
using System.Collections.Generic;
using System.Xml;

namespace AutoWrap.Meta
{
    /// <summary>
    /// Contains the definition of a C++ namespace (i.e. its name) along with the name of its
    /// managed counter part.
    /// </summary>
    public class NamespaceDefinition
    {
        private readonly NamespaceDefinition _parentNamespace;
        private readonly List<NamespaceDefinition> _childNamespaces = new List<NamespaceDefinition>();

        /// <summary>
        /// The native (C++) name of this namespace.
        /// </summary>
        /// <seealso cref="CLRName"/>
        public readonly string NativeName;

        /// <summary>
        /// The managed (C++/CLI) name of this namespace. May differ from <see cref="NativeName"/>.
        /// </summary>
        public readonly string CLRName;
        
        private readonly List<AbstractTypeDefinition> _containedTypes = new List<AbstractTypeDefinition>();
        /// <summary>
        /// Contains the type definitions (mostly class definitions) contained in this namespace.
        /// </summary>
        public IEnumerable<AbstractTypeDefinition> ContainedTypes
        {
            get
            {
                foreach (AbstractTypeDefinition def in _containedTypes)
                {
                    yield return def;
                }
            }
        }

        public NamespaceDefinition(XmlElement elem, string managedRootNamespaceName, MetaDefinition metaDef)
        {
            // The child namespace names are stored in the attributes "second" and "third". Thus
            // we can only support up to three namespace levels (like "Level1::Level2::Level3").
            string second = elem.GetAttribute("second");
            string third = elem.GetAttribute("third");
            
            NativeName = elem.GetAttribute("name");
            CLRName = managedRootNamespaceName;
            
            if (second != "")
            {
                NativeName += "::" + second;
                CLRName += "::" + second;
            }
            
            if (third != "")
            {
                NativeName += "::" + third;
                CLRName += "::" + third;
            }
            
            // If this is a child namespace, set parent and add itself to the parent.
            if (NativeName.Contains("::"))
            {
                // Is a child namespace.
                string parentNamespaceName = NativeName.Substring(0, NativeName.LastIndexOf("::"));
                _parentNamespace = metaDef.GetNameSpace(parentNamespaceName);
            
                _parentNamespace._childNamespaces.Add(this);
            }
            else
                _parentNamespace = null;
            
            //
            // Add types contained in this namespace.
            //
            foreach (XmlElement child in elem.ChildNodes)
            {
                AbstractTypeDefinition type = AbstractTypeDefinition.CreateType(child);
                if (type != null)
                {
                    type.NameSpace = this;
                    _containedTypes.Add(type);
                }
            }
        }

        public T FindType<T>(string name, bool raiseException = true)
        {
            if (name.EndsWith(" std::string"))
                name = "std::string";

            if (name == "std::string")
                return (T)(object)new DefString();

            if (name == "DisplayString")
                return (T)(object)new DefUtfString();

            if (name.StartsWith(Globals.NativeNamespace + "::"))
                name = name.Substring(name.IndexOf("::") + 2);

            T type = FindTypeInList<T>(name, _containedTypes, false);
            if (type == null)
            {
                if (_parentNamespace == null)
                {
                    if (raiseException)
                        throw new Exception("Could not find type");

                    return (T) (object) new DefInternal(name);
                }

                return _parentNamespace.FindType<T>(name, raiseException);
            }

            if (type is AbstractTypeDefinition)
            {
                // Short circuit out to handle OGRE 1.6 memory allocator types
                if (((AbstractTypeDefinition) (object) type).IsIgnored)
                    return type;
            }

            return (T)(object)((AbstractTypeDefinition)(object)type).CreateExplicitType();
        }

        protected virtual T FindTypeInList<T>(string name, List<AbstractTypeDefinition> types, bool raiseException)
        {
            List<AbstractTypeDefinition> list = new List<AbstractTypeDefinition>();

            string topname = name;
            string nextnames = null;

            if (name.Contains("::"))
            {
                topname = name.Substring(0, name.IndexOf("::"));
                nextnames = name.Substring(name.IndexOf("::") + 2);
            }

            foreach (AbstractTypeDefinition t in types)
            {
                if (t is T && t.Name == topname)
                {
                    list.Add(t);
                }
            }

            if (list.Count == 0)
            {
                if (raiseException)
                    throw new Exception("Could not find type");

                return default(T);
            }

            if (list.Count > 1)
                throw new Exception("Found more than one type");

            T type = (T) (object) list[0];

            if (nextnames == null)
                return type;

            return FindTypeInList<T>(nextnames, ((ClassDefinition)(object)type).NestedTypes, raiseException);
        }

        public AbstractTypeDefinition GetDefType(string name)
        {
            AbstractTypeDefinition type = null;
            foreach (AbstractTypeDefinition t in _containedTypes)
            {
                if (t.Name == name)
                {
                    type = t;
                    break;
                }
            }

            if (type == null)
                throw new Exception(String.Format("DefType not found for '{0}'", name));

            return type;
        }
    }
}