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
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace AutoWrap.Meta
{
    /// <summary>
    /// Base class for all classes that generate some kind of source code.
    /// </summary>
    public class CodeProducer
    {
        protected virtual string GetNativeDirectorReceiverInterfaceName(DefClass type)
        {
            if (!type.HasWrapType(WrapTypes.NativeDirector))
                throw new Exception("Unexpected");

            string name = (type.IsNested) ? type.ParentClass.Name + "_" + type.Name : type.Name;
            return "I" + name + "_Receiver";
        }

        protected virtual string GetNativeDirectorName(DefClass type)
        {
            if (!type.HasWrapType(WrapTypes.NativeDirector))
                throw new Exception("Unexpected");

            string name = (type.IsNested) ? type.ParentClass.Name + "_" + type.Name : type.Name;
            return name + "_Director";
        }

        /// <summary>
        /// Checks whether the specified property can be added to the generated source code.
        /// </summary>
        protected virtual bool IsPropertyAllowed(DefProperty p)
        {
            // If the property is ignored or the property is unhandled
            if (p.Function.HasAttribute<IgnoreAttribute>() || !p.IsTypeHandled())
                return false;
            
            if (p.Class.IsSingleton && (p.Name == "Singleton" || p.Name == "SingletonPtr"))
                return false;
            
            return true;
        }

        /// <summary>
        /// Checks whether the specified function can be added to the generated source code.
        /// </summary>
        protected virtual bool IsFunctionAllowed(DefFunction f)
        {
            // If the function is ignored or the return value type is unhandled
            if (f.HasAttribute<IgnoreAttribute>() || !f.IsTypeHandled())
                return false;
        
            // Check whether all parameter types are handled
            foreach (DefParam param in f.Parameters)
            {
                if (!param.IsTypeHandled())
                    return false;
            }

            return true;
        }

        protected virtual DefType CreateExplicitContainerType(string container, string key, string val)
        {
            string stdcont = "std::" + container;
            XmlDocument doc = new XmlDocument();
            XmlElement elem = doc.CreateElement("typedef");
            elem.SetAttribute("basetype", stdcont);
            elem.SetAttribute("name", stdcont);
            XmlElement te = doc.CreateElement("type");
            te.InnerText = val;
            elem.AppendChild(te);
            if (key != "")
            {
                te = doc.CreateElement("type");
                te.InnerText = key;
                elem.InsertAfter(te, null);
            }

            return DefTypeDef.CreateExplicitType((DefTypeDef)DefType.CreateType(elem));
        }

        protected virtual string NameToPrivate(string name)
        {
            return "_" + Char.ToLower(name[0]) + name.Substring(1);
        }
        protected virtual string NameToPrivate(DefMember m)
        {
            string name = m.Name;
            if (m is DefFunction
                && (m as DefFunction).IsGetProperty
                && name.StartsWith("get"))
                name = name.Substring(3);

            return NameToPrivate(name);
        }

        protected virtual void AddTypeDependancy(DefType type)
        {
        }

        public static bool IsIteratorWrapper(DefTypeDef type)
        {
            string[] iters = new string[] { "MapIterator", "ConstMapIterator",
                            "VectorIterator", "ConstVectorIterator" };

            foreach (string it in iters)
                if (type.BaseTypeName.StartsWith(it))
                    return true;

            return false;
        }

        protected virtual bool? CheckFunctionForGetProperty(DefFunction f)
        {
            string name = f.HasAttribute<RenameAttribute>() ? f.GetAttribute<RenameAttribute>().Name : f.Name;

            if (f.HasAttribute<CustomIncDeclarationAttribute>() || f.HasAttribute<CustomCppDeclarationAttribute>())
                return false;

            if (f.TypeName == "bool" &&
                ((name.StartsWith("is") && Char.IsUpper(name[2])) || (name.StartsWith("has") && Char.IsUpper(name[3])))
                && f.Parameters.Count == 0)
                return true;

            return CheckTypeMemberForGetProperty(f);
        }

        protected virtual bool? CheckTypeMemberForGetProperty(ITypeMember m)
        {
            if (!m.Type.IsValueType && (m.Type.IsSharedPtr || m.Type is DefTemplateOneType || m.Type is DefTemplateTwoTypes))
                return false;

            if (m.Type.HasAttribute<ReturnOnlyByMethodAttribute>())
                return false;

            return null;
        }

        /// <summary>
        /// Converts the name into upper camel case, meaning the first character will be
        /// made upper-case. Note that the remainder of the name must already be in camel
        /// case.
        /// </summary>
        public static string ToCamelCase(string name)
        {
            return Char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}
