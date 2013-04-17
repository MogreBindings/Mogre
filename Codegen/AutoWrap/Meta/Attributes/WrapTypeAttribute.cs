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

namespace AutoWrap.Meta
{
    public enum WrapTypes
    {
        NonOverridable,
        Overridable,
        NativeDirector,
        Interface,
        Singleton,
        SharedPtr,
        ReadOnlyStruct,
        ValueType,
        NativePtrValueType,
        CLRHandle,
        PlainWrapper
    }

    public class WrapTypeAttribute : AutoWrapAttribute
    {
        public WrapTypes WrapType;

        public WrapTypeAttribute(WrapTypes type)
        {
            WrapType = type;
        }

        public static WrapTypeAttribute FromElement(XmlElement elem)
        {
            WrapTypes wt = (WrapTypes) Enum.Parse(typeof (WrapTypes), elem.InnerText, true);
            return new WrapTypeAttribute(wt);
        }

        public override void ProcessHolder(AttributeSet holder)
        {
            switch (WrapType)
            {
                case WrapTypes.NativePtrValueType:
                case WrapTypes.ValueType:
                    if (!holder.HasAttribute<ValueTypeAttribute>())
                        holder.AddAttribute(new ValueTypeAttribute());
                    break;
                case WrapTypes.Overridable:
                    ClassDefinition type = (ClassDefinition) holder;
                    AddAttributeToInheritanceChain(type, new BaseForSubclassingAttribute());
                    break;
            }
        }
    }
}