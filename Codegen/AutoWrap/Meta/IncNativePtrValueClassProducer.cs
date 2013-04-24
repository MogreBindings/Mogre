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
    class IncNativePtrValueClassProducer : ClassInclCodeProducer
    {
        protected override string GetTopBaseClassName()
        {
            return null;
        }

        protected override void AddDefinition()
        {
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
            {
                _wrapper.AddPreDeclaration("value class " + _classDefinition.CLRName + ";");
                _wrapper.AddPragmaMakePublicForType(_classDefinition);
            }
        }

        protected override void AddPrivateDeclarations()
        {
            base.AddPrivateDeclarations();
            _codeBuilder.AppendLine(_classDefinition.FullyQualifiedNativeName + "* _native;");
        }

        protected override void AddPublicDeclarations()
        {
            base.AddPublicDeclarations();

            _codeBuilder.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_NATIVEPTRVALUECLASS( " + GetClassName() + ", " + _classDefinition.FullyQualifiedNativeName + " )");
            _codeBuilder.AppendEmptyLine();

            _codeBuilder.AppendEmptyLine();
            _codeBuilder.AppendLine("property IntPtr NativePtr");
            _codeBuilder.AppendLine("{");
            _codeBuilder.AppendLine("\tIntPtr get() { return (IntPtr)_native; }");
            _codeBuilder.AppendLine("}");

            if (!IsReadOnly)
            {
                _codeBuilder.AppendEmptyLine();
                AddCreators();

                _codeBuilder.AppendEmptyLine();
                _codeBuilder.AppendLine("void DestroyNativePtr()");
                _codeBuilder.AppendLine("{");
                _codeBuilder.AppendLine("\tif (_native)  { delete _native; _native = 0; }");
                _codeBuilder.AppendLine("}");
            }

            _codeBuilder.AppendEmptyLine();
            _codeBuilder.AppendLine("property bool IsNull");
            _codeBuilder.AppendLine("{");
            _codeBuilder.AppendLine("\tbool get() { return (_native == 0); }");
            _codeBuilder.AppendLine("}");
        }

        protected override void AddPublicConstructors()
        {
        }

        protected virtual void AddCreators()
        {
            if (_classDefinition.IsNativeAbstractClass)
                return;

            if (_classDefinition.Constructors.Length > 0)
            {
                foreach (MemberMethodDefinition func in _classDefinition.Constructors)
                    if (func.ProtectionLevel == ProtectionLevel.Public)
                        AddCreator(func);
            }
            else
                AddCreator(null);
        }

        protected virtual void AddCreator(MemberMethodDefinition f)
        {
            if (f == null)
                _codeBuilder.AppendLine("static " + _classDefinition.CLRName + " Create();");
            else
            {
                int defcount = 0;

                if (!f.HasAttribute<NoDefaultParamOverloadsAttribute>())
                {
                    foreach (ParamDefinition param in f.Parameters)
                        if (param.DefaultValue != null)
                            defcount++;
                }

                // The overloads (because of default values)
                for (int dc = 0; dc <= defcount; dc++)
                {
                    if (dc < defcount && f.HasAttribute<HideParamsWithDefaultValuesAttribute>())
                        continue;

                    _codeBuilder.AppendIndent("static " + _classDefinition.CLRName + " Create");
                    AddMethodParameters(f, f.Parameters.Count - dc);
                    _codeBuilder.Append(";\n");
                }

            }
        }

        public IncNativePtrValueClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }
    }
}
