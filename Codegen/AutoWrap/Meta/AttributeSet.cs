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

namespace AutoWrap.Meta
{
    /// <summary>
    /// A set of attributes (identified by their type; see sub classes of <see cref="AutoWrapAttribute"/>) associated with
    /// a source code element. Note that only source code elements coming from the original C++ code have associated attributes.
    /// Source code elements generated from the C++ sources (like CLR properties or types) won't be derived from this class.
    /// </summary>
    public abstract class AttributeSet : AbstractCodeProducer
    {
        private Dictionary<Type, AutoWrapAttribute> _attributes = new Dictionary<Type, AutoWrapAttribute>();

        /// <summary>
        /// The attributes in this attribute set.
        /// </summary>
        public IEnumerable<AutoWrapAttribute> Attributes
        {
            get
            {
                foreach (AutoWrapAttribute attrib in _attributes.Values)
                    yield return attrib;
            }
        }

        protected AttributeSet(MetaDefinition metaDef)
            : base(metaDef)
        {
        }

        /// <summary>
        /// Adds an attribute to this set. If this attribute has already been added then it's
        /// overwritten.
        /// </summary>
        public virtual void AddAttribute(AutoWrapAttribute attrib)
        {
            _attributes[attrib.GetType()] = attrib;
        }

        /// <summary>
        /// Adds multiple attributes to this set.
        /// </summary>
        public void AddAttributes(IEnumerable<AutoWrapAttribute> attribs)
        {
            foreach (AutoWrapAttribute attrib in attribs)
                AddAttribute(attrib);
        }

        /// <summary>
        /// Checks whether this set contains the specified attribute.
        /// </summary>
        /// <typeparam name="T">the attribute to look for (specified by its type)</typeparam>
        public virtual bool HasAttribute<T>() where T : AutoWrapAttribute
        {
            return _attributes.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Returns the attribute with the specified type T.
        /// </summary>
        /// <typeparam name="T">The attribute's type (i.e. the kind of attribute); if this
        /// attribute isn't part of this set a <c>KeyNotFoundException</c> will be thrown.</typeparam>
        public virtual T GetAttribute<T>() where T : AutoWrapAttribute
        {
            return (T)_attributes[typeof(T)];
        }

        // FIXME: Check whether this is really necessary.
        public void LinkAttributes(AttributeSet set)
        {
            // NOTE: This is needed at one location and it's not enough to copy the elements.
            //   We need to use the same reference.
            _attributes = set._attributes;
        }
    }
}
