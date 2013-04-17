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
using System.Reflection;
using System.Collections.Generic;
using System.Xml;

namespace AutoWrap.Meta
{
    /// <summary>
    /// Holds the information from the <c>meta.xml</c> file, i.e. information about the 
    /// original C++ source code.
    /// </summary>
    public class MetaDefinition
    {
        private readonly XmlDocument _doc = new XmlDocument();
        private readonly string _managedNamespace;
        private readonly List<KeyValuePair<AttributeSet, AutoWrapAttribute>> _holders = new List<KeyValuePair<AttributeSet, AutoWrapAttribute>>();

        public List<NamespaceDefinition> NameSpaces = new List<NamespaceDefinition>();

        public MetaDefinition(string file, string managedNamespace)
        {
            _doc.Load(file);
            this._managedNamespace = managedNamespace;

            XmlElement root = (XmlElement)_doc.GetElementsByTagName("meta")[0];

            foreach (XmlNode elem in root.ChildNodes)
            {
                if (elem is XmlElement)
                    AddNamespace(elem as XmlElement);
            }
        }

        public void AddAttributes(string file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file);

            XmlElement root = (XmlElement)doc.GetElementsByTagName("meta")[0];

            foreach (XmlNode elem in root.ChildNodes)
            {
                if (elem is XmlElement)
                    AddAttributesInNamespace(GetNameSpace((elem as XmlElement).GetAttribute("name")), elem as XmlElement);
            }

            foreach (KeyValuePair<AttributeSet, AutoWrapAttribute> pair in _holders)
            {
                pair.Value.ProcessHolder(pair.Key);
            }
        }

        private void AddAttributesInNamespace(NamespaceDefinition nameSpace, XmlElement elem)
        {
            foreach (XmlNode child in elem.ChildNodes)
            {
                if (child is XmlElement)
                    AddAttributesInType(nameSpace.GetDefType((child as XmlElement).GetAttribute("name")), child as XmlElement);
            }
        }

        private void AddAttributesInType(AbstractTypeDefinition type, XmlElement elem)
        {
            foreach (XmlAttribute attr in elem.Attributes)
            {
                if (attr.Name != "name")
                    AddAttributeInHolder(type, CreateAttribute(attr));
            }

            foreach (XmlNode child in elem.ChildNodes)
            {
                if (!(child is XmlElement))
                    continue;

                if (child.Name[0] == '_')
                {
                    AddAttributeInHolder(type, CreateAttribute(child as XmlElement));
                    continue;
                }

                switch (child.Name)
                {
                    case "class":
                    case "struct":
                    case "enumeration":
                    case "typedef":
                        AddAttributesInType((type as ClassDefinition).GetNestedType((child as XmlElement).GetAttribute("name")), child as XmlElement);
                        break;
                    case "function":
                    case "variable":
                        foreach (AbstractMemberDefinition m in (type as ClassDefinition).GetMembers((child as XmlElement).GetAttribute("name")))
                            AddAttributesInMember(m, child as XmlElement);
                        break;
                    default:
                        throw new Exception("Unexpected");
                }
            }
        }

        private void AddAttributesInMember(AbstractMemberDefinition member, XmlElement elem)
        {
            foreach (XmlAttribute attr in elem.Attributes)
            {
                if (attr.Name != "name")
                    AddAttributeInHolder(member, CreateAttribute(attr));
            }

            foreach (XmlNode child in elem.ChildNodes)
            {
                if (!(child is XmlElement))
                    continue;

                if (child.Name[0] == '_')
                {
                    AddAttributeInHolder(member, CreateAttribute(child as XmlElement));
                    continue;
                }

                switch (child.Name)
                {
                    case "param":
                        if (!(member is MemberMethodDefinition))
                            throw new Exception("Unexpected");

                        string name = (child as XmlElement).GetAttribute("name");
                        ParamDefinition param = null;
                        foreach (ParamDefinition p in (member as MemberMethodDefinition).Parameters)
                        {
                            if (p.Name == name)
                            {
                                param = p;
                                break;
                            }
                        }
                        if (param == null)
                            return;
                            //throw new Exception("Wrong param name");

                        foreach (XmlAttribute attr in child.Attributes)
                        {
                            if (attr.Name != "name")
                                AddAttributeInHolder(param, CreateAttribute(attr));
                        }
                        break;

                    default:
                        throw new Exception("Unexpected");
                }
            }
        }

        private AutoWrapAttribute CreateAttribute(XmlElement elem)
        {
            string typename = elem.Name.Substring(1);
            string nameSpace = typeof(WrapTypeAttribute).Namespace;

            Type type = Assembly.GetExecutingAssembly().GetType(nameSpace + "." + typename + "Attribute", true, true);
            return (AutoWrapAttribute) type.GetMethod("FromElement").Invoke(null, new object[] {elem});
        }

        private AutoWrapAttribute CreateAttribute(XmlAttribute attr)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement elem = doc.CreateElement("_" + attr.Name);
            elem.InnerText = attr.Value;
            return CreateAttribute(elem);
        }

        private void AddAttributeInHolder(AttributeSet holder, AutoWrapAttribute attr)
        {
            holder.AddAttribute(attr);
            _holders.Add(new KeyValuePair<AttributeSet, AutoWrapAttribute>(holder, attr));
        }

        public NamespaceDefinition GetNameSpace(string name)
        {
            NamespaceDefinition spc = null;
            foreach (NamespaceDefinition ns in NameSpaces)
            {
                if (ns.NativeName == name)
                {
                    spc = ns;
                    break;
                }
            }

            if (spc == null)
                throw new Exception("couldn't find namespace");

            return spc;
        }

        private void AddNamespace(XmlElement elem)
        {
            if (elem.Name != "namespace")
                throw new Exception("Wrong element; expected 'namespace'.");

            NamespaceDefinition spc = new NamespaceDefinition(elem, _managedNamespace);

            if (spc.NativeName.Contains("::"))
            {
                string pname = spc.NativeName.Substring(0, spc.NativeName.LastIndexOf("::"));
                foreach (NamespaceDefinition fns in NameSpaces)
                {
                    if (fns.NativeName == pname)
                    {
                        spc.ParentNameSpace = fns;
                        fns.ChildNameSpaces.Add(spc);
                        break;
                    }
                }
            }

            NameSpaces.Add(spc);
        }
    }
}