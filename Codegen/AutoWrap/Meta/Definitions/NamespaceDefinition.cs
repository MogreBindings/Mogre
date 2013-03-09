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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace AutoWrap.Meta
{
    public class NamespaceDefinition
    {
        XmlElement _elem;
        string _managedNamespace;

        public NamespaceDefinition ParentNameSpace = null;
        public List<NamespaceDefinition> ChildNameSpaces = new List<NamespaceDefinition>();

        public XmlElement Element
        {
            get { return _elem; }
        }

        public string NativeName
        {
            get { return NamespaceDefinition.GetFullName(_elem); }
        }

        public string CLRName;

        public List<AbstractTypeDefinition> Types = new List<AbstractTypeDefinition>();

        public T FindType<T>(string name)
        {
            return FindType<T>(name, true);
        }
        public T FindType<T>(string name, bool raiseException)
        {
            if (name.EndsWith(" std::string"))
                name = "std::string";

            if (name == "std::string")
                return (T)(object)new DefString();

            if (name == "DisplayString")
                return (T)(object)new DefUtfString ();

            if (name.StartsWith(Globals.NativeNamespace + "::"))
                name = name.Substring(name.IndexOf("::") + 2);

            T type = FindTypeInList<T>(name, Types, false);
            if (type == null)
            {
                if (ParentNameSpace == null)
                {
                    if (raiseException)
                        throw new Exception("Could not find type");
                    else
                        return (T)(object)new DefInternal(name);
                }
                else
                    return ParentNameSpace.FindType<T>(name, raiseException);
            }

            if(type is AbstractTypeDefinition) {
                // Short circuit out to handle OGRE 1.6 memory allocator types
                if(((AbstractTypeDefinition)(object)type).IsIgnored) {
                    return (T)(object)type;
                }
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
                else
                    return default(T);
            }
            else if (list.Count > 1)
                throw new Exception("Found more than one type");

            T type = (T)(object)list[0];

            if (nextnames == null)
                return type;
            else
                return FindTypeInList<T>(nextnames, ((ClassDefinition)(object)type).NestedTypes, raiseException);
        }

        public NamespaceDefinition(XmlElement elem, string managedNamespace)
        {
            this._elem = elem;
            this._managedNamespace = managedNamespace;

            string second = elem.GetAttribute("second");
            string third = elem.GetAttribute("third");

            this.CLRName = managedNamespace;

            if (second != "")
                this.CLRName += "::" + second;

            if (third != "")
                this.CLRName += "::" + third;

            foreach (XmlElement child in elem.ChildNodes)
            {
                AbstractTypeDefinition type = AbstractTypeDefinition.CreateType(child);
                if (type != null)
                {
                    type.NameSpace = this;
                    this.Types.Add(type);
                }
            }
        }

        public AbstractTypeDefinition GetDefType(string name)
        {
            AbstractTypeDefinition type = null;
            foreach (AbstractTypeDefinition t in Types)
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

        public static string GetFullName(XmlElement elem)
        {
            if (elem.Name != "namespace")
                throw new Exception("Wrong element; expected 'namespace'.");

            string first, second, third;
            first = elem.GetAttribute("name");
            second = elem.GetAttribute("second");
            third = elem.GetAttribute("third");

            string name = first;
            if (second != "")
                name += "::" + second;

            if (third != "")
                name += "::" + third;

            return name;
        }
    }
}
