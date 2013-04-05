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
    class IncReadOnlyStructClassProducer : ClassInclCodeProducer
    {
        protected override string GetTopBaseClassName()
        {
            return null;
        }

        protected override void AddDefinition()
        {
            _code.AppendIndent("");
            if (!_definition.IsNested)
                _code.Append("public ");
            else
                _code.Append(_definition.ProtectionLevel.GetCLRProtectionName() + ": ");
            _code.AppendFormat("ref class {0}\n", _definition.CLRName);
        }

        protected override void AddPublicConstructors()
        {
        }

        protected override void AddInternalConstructors()
        {
            base.AddInternalConstructors();
            _code.AppendLine(_definition.CLRName + "()");
            _code.AppendLine("{");
            _code.IncreaseIndent();
            base.AddConstructorBody();
            _code.DecreaseIndent();
            _code.AppendLine("}\n");
        }

        protected override void AddInternalDeclarations()
        {
            base.AddInternalDeclarations();
            foreach (MemberFieldDefinition field in _definition.PublicFields)
            {
                if (!field.IsIgnored)
                    _code.AppendLine(field.MemberTypeCLRName + " " + NameToPrivate(field) + ";");
            }
            _code.AppendEmptyLine();

            _code.AppendLine("static operator " + _definition.CLRName + "^ (const " + _definition.FullNativeName + "& obj)");
            _code.AppendLine("{");
            _code.IncreaseIndent();
            _code.AppendLine(_definition.CLRName + "^ clr = gcnew " + _definition.CLRName + ";");
            foreach (MemberFieldDefinition field in _definition.PublicFields)
            {
                if (!field.IsIgnored)
                {
                    string conv = field.Type.ProduceNativeCallConversionCode("obj." + field.Name, field);
                    _code.AppendLine("clr->" + NameToPrivate(field) + " = " + conv + ";");
                }
            }
            _code.AppendEmptyLine();
            _code.AppendLine("return clr;");
            _code.DecreaseIndent();
            _code.AppendLine("}");

            _code.AppendEmptyLine();
            _code.AppendLine("static operator " + _definition.CLRName + "^ (const " + _definition.FullNativeName + "* pObj)");
            _code.AppendLine("{");
            _code.AppendLine("\treturn *pObj;");
            _code.AppendLine("}");
        }

        protected override void AddPropertyField(MemberFieldDefinition field)
        {
            //TODO comments for fields
            //AddComments(field);
            string ptype = GetCLRTypeName(field);
            _code.AppendFormatIndent("property {0} {1}\n{{\n", ptype, ToCamelCase(field.Name));
            _code.IncreaseIndent();
            _code.AppendLine(ptype + " get()\n{");
            _code.AppendLine("\treturn " + NameToPrivate(field) + ";");
            _code.AppendLine("}");
            _code.DecreaseIndent();
            _code.AppendLine("}");
        }

        public IncReadOnlyStructClassProducer(Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }
    }
}
