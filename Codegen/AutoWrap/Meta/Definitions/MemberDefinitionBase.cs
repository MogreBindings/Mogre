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
    /// Describes a native class or struct member, i.e. a field (see <see cref="MemberFieldDefinition"/>) or a 
    /// method (<see cref="MemberMethodDefinition"/>). Note that only native (C++) members will be derived
    /// from this class; i.e. CLR properties aren't derived from this class.
    /// </summary>
    // NOTE: Don't call this class "AbstractMemberDefintion" to avoid confusion about whether the 
    //   described member is an "abstract" member (i.e. an abstract method).
    public abstract class MemberDefinitionBase : AttributeSet, ITypeMember
    {
        /// <summary>
        /// The managed (C++/CLI) name of this member.
        /// </summary>
        public virtual string CLRName
        {
            get
            {
                if (HasAttribute<RenameAttribute>())
                {
                    return GetAttribute<RenameAttribute>().Name;
                }
                
                return Name;
            }
        }

        public virtual bool IsIgnored
        {
            get
            {
                //TODO: Find a more general way to handle templates and get rid of this hacky way
                if (Definition.StartsWith("Controller<"))
                    return true;

                return (this.MemberType.IsIgnored || this.HasAttribute<IgnoreAttribute>());
            }
        }

        public readonly bool IsStatic;

        /// <summary>
        /// Indicates whether this member is C++ <c>const</c>.
        /// </summary>
        /// <remarks>Required by <see cref="ITypeMember"/>.</remarks>
        public abstract bool IsConst { get; }

        AbstractTypeDefinition _type = null;
        public virtual AbstractTypeDefinition MemberType
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
                        _type = Class.FindType<AbstractTypeDefinition>(TypeName, false);
                }

                return _type;
            }
        }

        /// <summary>
        /// The fully qualified name of this member's CLR type (i.e. with CLR (dest) namespace).
        /// </summary>
        /// <remarks>Required by <see cref="ITypeMember"/>.</remarks>
        public virtual string MemberTypeCLRName
        {
            get { return this.MemberType.GetCLRTypeName(this);  }
        }

        /// <summary>
        /// The fully qualified name of this member's native type (i.e. with native (source) namespace).
        /// </summary>
        /// <remarks>Required by <see cref="ITypeMember"/>.</remarks>
        public virtual string MemberTypeNativeName
        {
            get { return (this as ITypeMember).MemberType.GetNativeTypeName(IsConst, (this as ITypeMember).PassedByType); }
        }

        public ClassDefinition Class;

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
    
        /// <summary>
        /// The native (C++) protection level of this member (e.g. "public", "protected", ...).
        /// </summary>
        public ProtectionLevel ProtectionLevel;
    
        /// <summary>
        /// Describes how this member is accessed (e.g. pointer or copy). The actual interpretation
        /// depends on whether this member is a method or a field.
        /// </summary>
        public PassedByType PassedByType;

        string ITypeMember.MemberTypeName
        {
            get { return this.TypeName; }
        }

        PassedByType ITypeMember.PassedByType
        {
            get { return this.PassedByType; }
        }

        ClassDefinition ITypeMember.ContainingClass
        {
            get { return this.Class; }
        }

        public MemberDefinitionBase(MetaDefinition metaDef, XmlElement elem)
            : base(metaDef)
        {
            this.IsStatic = elem.GetAttribute("static") == "yes";
            this.ProtectionLevel = AbstractTypeDefinition.GetProtectionEnum(elem.GetAttribute("protection"));
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
                        // Let the subclass decide what to do with this.
                        InterpretChildElement(child);
                        break;
                }
            }
        }


        /// <summary>
        /// This method allows subclasses to interpret XML child elements other than
        /// "name", "type", and "definition" (which are already interpreted in the constructor).
        /// </summary>
        /// <param name="child">the child element to be interpreted</param>
        protected virtual void InterpretChildElement(XmlElement child)
        {
            throw new Exception("Unsupported child element: '" + child.Name + "'");
        }
    }
}
