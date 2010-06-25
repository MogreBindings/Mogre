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
    public class Producer
    {
        public static bool IsInternalTypeDef(DefType type)
        {
            if (!(type is DefTypeDef))
                return false;

            if (type.IsSharedPtr)
                return false;

            DefTypeDef explicitType = (type.IsNested) ? type.ParentClass.FindType<DefTypeDef>(type.Name) : type.NameSpace.FindType<DefTypeDef>(type.Name);
            if (explicitType.IsSTLContainer)
                return false;

            if (explicitType.BaseType is DefInternal)
                return true;

            return false;
        }

        public static string GetProtectionString(ProtectionType prot)
        {
            switch (prot)
            {
                case ProtectionType.Public:
                    return "public";
                case ProtectionType.Protected:
                    return "protected public";
                case ProtectionType.Private:
                    return "private";
                default:
                    throw new Exception("Unexpected");
            }
        }

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

        protected virtual bool IsUnhandledType(ITypeMember m)
        {
            if (m.Type.IsIgnored)
                return true;

            if (m.Type is DefClass && (m.Type as DefClass).IsSingleton)
                return true;

            switch (m.TypeName)
            {
                case "UserDefinedObject":
                    return true;
                default:
                    return false;
            }
        }

        protected virtual bool AllowProperty(DefProperty p)
        {
            if (p.Function.HasAttribute<IgnoreAttribute>() || IsUnhandledType(p))
                return false;
            else if (p.Class.IsSingleton && (p.Name == "Singleton" || p.Name == "SingletonPtr"))
                return false;
            else
                return true;
        }

        protected virtual bool AllowFunction(DefFunction f)
        {
            if (f.HasAttribute<IgnoreAttribute>() || IsUnhandledType(f))
                return false;
            else
            {
                foreach (DefParam param in f.Parameters)
                {
                    if (IsUnhandledType(param))
                        return false;
                }

                return true;
            }
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

        private static bool IsTypeNameIteratorWrapper(string typename)
        {
            string[] iters = new string[] { "MapIterator", "ConstMapIterator",
                            "VectorIterator", "ConstVectorIterator" };

            foreach (string it in iters)
                if (typename.StartsWith(it))
                    return true;

            return false;
        }

        public static bool IsIteratorWrapper(DefTypeDef type)
        {
            if (IsTypeNameIteratorWrapper(type.BaseTypeName))
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

        protected virtual string ToCamelCase(string name)
        {
            return Char.ToUpper(name[0]) + name.Substring(1);
        }
    }
}
