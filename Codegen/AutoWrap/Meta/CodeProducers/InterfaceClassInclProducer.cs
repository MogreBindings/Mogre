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
    class InterfaceClassInclProducer : ClassInclProducer
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
            if (!_classDefinition.IsNested)
            {
                _wrapper.AddPreDeclaration("interface class " + _classDefinition.CLRName + ";");
                _wrapper.AddPragmaMakePublicForType(_classDefinition);
            }
        }

        public InterfaceClassInclProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }

        protected override void GenerateCodePublicDeclarations()
        {
            _codeBuilder.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_INTERFACE( " + _classDefinition.CLRName + ", " + _classDefinition.FullyQualifiedNativeName + " )\n");
            _codeBuilder.AppendLine("virtual " + _classDefinition.FullyQualifiedNativeName + "* _GetNativePtr();\n");
            base.GenerateCodePublicDeclarations();
        }

        protected override void AddPublicConstructors()
        {
        }

        protected override void GenerateCodePrivateDeclarations()
        {
        }

        protected override void GenerateCodeInternalDeclarations()
        {
        }

        protected override void AddDefinition()
        {
            if (!_classDefinition.IsNested)
                _codeBuilder.Append("public ");
            else
                _codeBuilder.Append(_classDefinition.ProtectionLevel.GetCLRProtectionName() + ": ");
            _codeBuilder.AppendLine("interface class " + _classDefinition.CLRName);
        }

        protected override void GenerateCodeProtectedDeclarations()
        {
        }

        protected override void GenerateCodeMethod(MemberMethodDefinition f)
        {
            if (f.IsVirtual)
                base.GenerateCodeMethod(f);
        }

        protected override void GenerateCodeProperty(MemberPropertyDefinition p)
        {
            if (p.IsVirtual)
                base.GenerateCodeProperty(p);
        }
    }
}
