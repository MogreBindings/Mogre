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

namespace AutoWrap.Meta
{
    class IncValueClassProducer : ClassInclCodeProducer
    {
        protected override string GetTopBaseClassName()
        {
            return null;
        }

        protected override void AddDefinition()
        {
            if (_definition.HasAttribute<SequentialLayoutAttribute> ()) {
                _code.AppendIndent ("");
                _code.Append ("[StructLayout(LayoutKind::Sequential)]\n");
            }

            _code.AppendIndent("");
            if (!_definition.IsNested)
                _code.Append("public ");
            else
                _code.Append(_definition.ProtectionLevel.GetCLRProtectionName() + ": ");
            _code.AppendFormat("value class {0}\n", _definition.CLRName);
        }

        protected override void AddPreDeclarations()
        {
            if (!_definition.IsNested)
                _wrapper.AddPragmaMakePublicForType(_definition);
        }

        protected override void AddInternalDeclarations()
        {
            base.AddInternalDeclarations();

            if (IsReadOnly)
            {
                foreach (MemberFieldDefinition field in _definition.PublicFields)
                {
                    _code.AppendLine(field.MemberType.FullCLRName + " " + NameToPrivate(field) + ";");
                }
                _code.AppendEmptyLine();
            }
        }

        protected override void AddPublicDeclarations()
        {
            base.AddPublicDeclarations();
            _code.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( " + GetClassName() + " )");
        }

        protected override void AddPublicConstructors()
        {
        }

        protected override void AddPropertyField(MemberFieldDefinition field)
        {
            //TODO comments for fields
            //AddComments(field);

            if (IsReadOnly)
            {
                string ptype = GetCLRTypeName(field);
                _code.AppendFormatIndent("property {0} {1}\n{{\n", ptype, CodeStyleDefinition.ToCamelCase(field.NativeName));
                _code.IncreaseIndent();
                _code.AppendLine(ptype + " get()\n{");
                _code.AppendLine("\treturn " + NameToPrivate(field) + ";");
                _code.AppendLine("}");
                _code.DecreaseIndent();
                _code.AppendLine("}");
            }
            else
            {
                _code.AppendLine(field.MemberType.FullCLRName + " " + field.NativeName + ";");
            }
        }

        public IncValueClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }
    }
}
