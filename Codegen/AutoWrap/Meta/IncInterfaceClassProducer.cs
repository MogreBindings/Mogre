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
    class IncInterfaceClassProducer : ClassInclCodeProducer
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
            if (!_definition.IsNested)
            {
                _wrapper.AddPreDeclaration("interface class " + _definition.CLRName + ";");
                _wrapper.AddPragmaMakePublicForType(_definition);
            }
        }

        public IncInterfaceClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }

        protected override void AddPublicDeclarations()
        {
            _code.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_INTERFACE( " + _definition.CLRName + ", " + _definition.FullyQualifiedNativeName + " )\n");
            _code.AppendLine("virtual " + _definition.FullyQualifiedNativeName + "* _GetNativePtr();\n");
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
            _code.AppendIndent("");
            if (!_definition.IsNested)
                _code.Append("public ");
            else
                _code.Append(_definition.ProtectionLevel.GetCLRProtectionName() + ": ");
            _code.Append("interface class " + _definition.CLRName + "\n");
        }

        protected override void AddProtectedDeclarations()
        {
        }

        protected override void AddMethod(MemberMethodDefinition f)
        {
            if (f.IsVirtual)
                base.AddMethod(f);
        }

        protected override void AddProperty(MemberPropertyDefinition p)
        {
            if (p.IsVirtual)
                base.AddProperty(p);
        }
    }
}
