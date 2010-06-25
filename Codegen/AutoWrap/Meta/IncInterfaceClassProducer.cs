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
    class IncInterfaceClassProducer : IncClassProducer
    {
        protected override string GetTopBaseClassName()
        {
            return null;
        }

        protected override bool AllowMethodOverloads
        {
            get
            {
                return false;
            }
        }

        protected override void AddPreDeclarations()
        {
            if (!_t.IsNested)
            {
                _wrapper.AddPreDeclaration("interface class " + _t.CLRName + ";");
                _wrapper.AddPragmaMakePublicForType(_t);
            }
        }

        public IncInterfaceClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }

        protected override void AddPublicDeclarations()
        {
            _sb.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_INTERFACE( " + _t.CLRName + ", " + _t.FullNativeName + " )\n");
            _sb.AppendLine("virtual " + _t.FullNativeName + "* _GetNativePtr();\n");
            base.AddPublicDeclarations();
        }

        protected override void AddPublicConstructors()
        {
        }

        protected override void AddPrivateDeclarations()
        {
        }

        protected override void AddInternalDeclarations()
        {
        }

        protected override void AddDefinition()
        {
            _sb.AppendIndent("");
            if (!_t.IsNested)
                _sb.Append("public ");
            else
                _sb.Append(GetProtectionString(_t.ProtectionType) + ": ");
            _sb.Append("interface class " + _t.CLRName + "\n");
        }

        protected override void AddProtectedDeclarations()
        {
        }

        protected override void AddMethod(DefFunction f)
        {
            if (f.IsVirtual)
                base.AddMethod(f);
        }

        protected override void AddProperty(DefProperty p)
        {
            if (p.Function.IsVirtual)
                base.AddProperty(p);
        }
    }
}
