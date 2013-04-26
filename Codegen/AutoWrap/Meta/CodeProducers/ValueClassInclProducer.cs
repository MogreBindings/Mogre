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
    class ValueClassInclProducer : ClassInclProducer
    {
        protected override string GetTopBaseClassName()
        {
            return null;
        }

        protected override void AddDefinition()
        {
            if (_classDefinition.HasAttribute<SequentialLayoutAttribute> ())
            {
                _codeBuilder.AppendIndent ("");
                _codeBuilder.Append ("[StructLayout(LayoutKind::Sequential)]\n");
            }

            _codeBuilder.AppendIndent("");
            if (!_classDefinition.IsNested)
                _codeBuilder.Append("public ");
            else
                _codeBuilder.Append(_classDefinition.ProtectionLevel.GetCLRProtectionName() + ": ");
            _codeBuilder.AppendFormat("value class {0}\n", _classDefinition.CLRName);
        }

        protected override void AddPreDeclarations()
        {
            if (!_classDefinition.IsNested)
                _wrapper.AddPragmaMakePublicForType(_classDefinition);
        }

        protected override void GenerateCodeInternalDeclarations()
        {
            base.GenerateCodeInternalDeclarations();

            if (IsReadOnly)
            {
                foreach (MemberFieldDefinition field in _classDefinition.PublicFields)
                {
                    _codeBuilder.AppendLine(field.MemberType.FullyQualifiedCLRName + " " + NameToPrivate(field) + ";");
                }
                _codeBuilder.AppendEmptyLine();
            }
        }

        protected override void GenerateCodePublicDeclarations()
        {
            base.GenerateCodePublicDeclarations();
            _codeBuilder.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( " + GetClassName() + " )");
        }

        protected override void AddPublicConstructors()
        {
        }

        protected override void GenerateCodePropertyField(MemberFieldDefinition field)
        {
            //TODO comments for fields
            //AddComments(field);

            if (IsReadOnly)
            {
                string ptype = GetCLRTypeName(field);
                _codeBuilder.AppendFormatIndent("property {0} {1}\n{{\n", ptype, CodeStyleDefinition.ToCamelCase(field.NativeName));
                _codeBuilder.IncreaseIndent();
                _codeBuilder.AppendLine(ptype + " get()\n{");
                _codeBuilder.AppendLine("\treturn " + NameToPrivate(field) + ";");
                _codeBuilder.AppendLine("}");
                _codeBuilder.DecreaseIndent();
                _codeBuilder.AppendLine("}");
            }
            else
            {
                _codeBuilder.AppendLine(field.MemberType.FullyQualifiedCLRName + " " + field.NativeName + ";");
            }
        }

        public ValueClassInclProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }
    }
}
