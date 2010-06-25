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
    class IncReadOnlyStructClassProducer : IncClassProducer
    {
        protected override string GetTopBaseClassName()
        {
            return null;
        }

        protected override void AddDefinition()
        {
            _sb.AppendIndent("");
            if (!_t.IsNested)
                _sb.Append("public ");
            else
                _sb.Append(GetProtectionString(_t.ProtectionType) + ": ");
            _sb.AppendFormat("ref class {0}\n", _t.CLRName);
        }

        protected override void AddPublicConstructors()
        {
        }

        protected override void AddInternalConstructors()
        {
            base.AddInternalConstructors();
            _sb.AppendLine(_t.CLRName + "()");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();
            base.AddConstructorBody();
            _sb.DecreaseIndent();
            _sb.AppendLine("}\n");
        }

        protected override void AddInternalDeclarations()
        {
            base.AddInternalDeclarations();
            foreach (DefField field in _t.PublicFields)
            {
                if (!field.IsIgnored)
                    _sb.AppendLine(field.CLRTypeName + " " + NameToPrivate(field) + ";");
            }
            _sb.AppendLine();

            _sb.AppendLine("static operator " + _t.CLRName + "^ (const " + _t.FullNativeName + "& obj)");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();
            _sb.AppendLine(_t.CLRName + "^ clr = gcnew " + _t.CLRName + ";");
            foreach (DefField field in _t.PublicFields)
            {
                if (!field.IsIgnored)
                {
                    string conv = field.Type.GetNativeCallConversion("obj." + field.Name, field);
                    _sb.AppendLine("clr->" + NameToPrivate(field) + " = " + conv + ";");
                }
            }
            _sb.AppendLine();
            _sb.AppendLine("return clr;");
            _sb.DecreaseIndent();
            _sb.AppendLine("}");

            _sb.AppendLine();
            _sb.AppendLine("static operator " + _t.CLRName + "^ (const " + _t.FullNativeName + "* pObj)");
            _sb.AppendLine("{");
            _sb.AppendLine("\treturn *pObj;");
            _sb.AppendLine("}");
        }

        protected override void AddPropertyField(DefField field)
        {
            //TODO comments for fields
            //AddComments(field);
            string ptype = GetCLRTypeName(field);
            _sb.AppendFormatIndent("property {0} {1}\n{{\n", ptype, ToCamelCase(field.Name));
            _sb.IncreaseIndent();
            _sb.AppendLine(ptype + " get()\n{");
            _sb.AppendLine("\treturn " + NameToPrivate(field) + ";");
            _sb.AppendLine("}");
            _sb.DecreaseIndent();
            _sb.AppendLine("}");
        }

        public IncReadOnlyStructClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }
    }
}
