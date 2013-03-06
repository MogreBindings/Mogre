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
using System.Text;
using System.Xml;

namespace AutoWrap.Meta
{
    public abstract class DefMember : AttributeHolder, ITypeMember
    {
        string ITypeMember.MemberTypeName
        {
            get { return this.TypeName; }
        }
        PassedByType ITypeMember.PassedByType
        {
            get { return this.PassedByType; }
        }
        DefClass ITypeMember.ContainingClass
        {
            get { return this.Class; }
        }
        DefType ITypeMember.MemberType
        {
            get { return this.Type; }
        }
        bool ITypeMember.HasAttribute<T>()
        {
            return HasAttribute<T>();
        }
        T ITypeMember.GetAttribute<T>()
        {
            return this.GetAttribute<T>();
        }

        public abstract bool IsProperty { get; }

        public virtual bool IsConst
        {
            get { return false; }
        }

        public virtual bool IsIgnored
        {
            get
            {
                //TODO: Find a more general way to handle templates and get rid of this hacky way
                if (Definition.StartsWith("Controller<"))
                    return true;

                return (Type.IsIgnored || this.HasAttribute<IgnoreAttribute>());
            }
        }

        private string _clrTypeName;

        public virtual string MemberTypeCLRName
        {
            get
            {
                if (_clrTypeName == null)
                    _clrTypeName = Type.GetCLRTypeName(this);

                return _clrTypeName;
            }
        }

        public virtual string MemberTypeNativeName
        {
            get { return (this as ITypeMember).MemberType.GetNativeTypeName(IsConst, (this as ITypeMember).PassedByType); }
        }

        private DefType _type = null;

        public virtual DefType Type
        {
            get
            {
                if (_type == null)
                {
                    if (Container != "")
                    {
                        _type = CreateExplicitContainerType(Container, ContainerKey, (ContainerValue != "") ? ContainerValue : TypeName);
                        _type.SurroundingClass = Class;
                    }
                    else
                        _type = Class.FindType<DefType>(TypeName, false);
                }

                return _type;
            }
        }

        protected virtual void InterpretChildElement(XmlElement child)
        {
            throw new Exception("Unknown child of member: '" + child.Name + "'");
        }

        protected XmlElement _elem;

        public DefClass Class;

        protected string _name;
        public virtual string Name
        {
            get { return _name; }
        }

        protected string _container;
        public virtual string Container
        {
            get { return _container; }
        }

        protected string _containerKey;
        public virtual string ContainerKey
        {
            get { return _containerKey; }
        }

        protected string _containerValue;
        public virtual string ContainerValue
        {
            get { return _containerValue; }
        }

        public string TypeName = null;
        public string Definition;
        public ProtectionLevel ProtectionType;
        public PassedByType PassedByType;

        public virtual bool IsVoid
        {
            get { return (TypeName == "void" || TypeName == "const void")
                && PassedByType == PassedByType.Value; }
        }

        public virtual string CLRName
        {
            get
            {
                if (HasAttribute<RenameAttribute>())
                    return GetAttribute<RenameAttribute>().Name;
                else
                    return Name;
            }
        }

        public bool IsStatic
        {
            get { return _elem.GetAttribute("static") == "yes"; }
        }

        public XmlElement Element
        {
            get { return _elem; }
        }

        public DefMember(XmlElement elem)
        {
            this._elem = elem;
            this.ProtectionType = DefType.GetProtectionEnum(elem.GetAttribute("protection"));
            this.PassedByType = (PassedByType)Enum.Parse(typeof(PassedByType), elem.GetAttribute("passedBy"), true);

            foreach (XmlElement child in elem.ChildNodes)
            {
                switch (child.Name)
                {
                    case "name":
                        _name = child.InnerText;
                        break;
                    case "type":
                        this.TypeName = child.InnerText;
                        this._container = child.GetAttribute("container");
                        this._containerKey = child.GetAttribute("containerKey");
                        this._containerValue = child.GetAttribute("containerValue");
                        break;
                    case "definition":
                        this.Definition = child.InnerText;
                        break;
                    default:
                        InterpretChildElement(child);
                        break;
                }
            }
        }
    }
}
