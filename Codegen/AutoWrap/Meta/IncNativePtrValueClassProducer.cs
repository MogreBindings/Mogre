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
            {
                _wrapper.AddPreDeclaration("value class " + _definition.CLRName + ";");
                _wrapper.AddPragmaMakePublicForType(_definition);
            }
        }

        protected override void AddPrivateDeclarations()
        {
            base.AddPrivateDeclarations();
            _code.AppendLine(_definition.FullNativeName + "* _native;");
        }

        protected override void AddPublicDeclarations()
        {
            base.AddPublicDeclarations();

            _code.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_NATIVEPTRVALUECLASS( " + GetClassName() + ", " + _definition.FullNativeName + " )");
            _code.AppendEmptyLine();

            _code.AppendEmptyLine();
            _code.AppendLine("property IntPtr NativePtr");
            _code.AppendLine("{");
            _code.AppendLine("\tIntPtr get() { return (IntPtr)_native; }");
            _code.AppendLine("}");

            if (!IsReadOnly && IsConstructable)
            {
                _code.AppendEmptyLine();
                AddCreators();

                _code.AppendEmptyLine();
                _code.AppendLine("void DestroyNativePtr()");
                _code.AppendLine("{");
                _code.AppendLine("\tif (_native)  { delete _native; _native = 0; }");
                _code.AppendLine("}");
            }

            _code.AppendEmptyLine();
            _code.AppendLine("property bool IsNull");
            _code.AppendLine("{");
            _code.AppendLine("\tbool get() { return (_native == 0); }");
            _code.AppendLine("}");
        }

        protected override void AddPublicConstructors()
        {
        }

        protected virtual void AddCreators()
        {
            if (_definition.IsNativeAbstractClass)
                return;

            if (_definition.Constructors.Length > 0)
            {
                foreach (MemberMethodDefinition func in _definition.Constructors)
                    if (func.ProtectionLevel == ProtectionLevel.Public)
                        AddCreator(func);
            }
            else
                AddCreator(null);
        }

        protected virtual void AddCreator(MemberMethodDefinition f)
        {
            if (f == null)
                _code.AppendLine("static " + _definition.CLRName + " Create();");
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

                    _code.AppendIndent("static " + _definition.CLRName + " Create");
                    AddMethodParameters(f, f.Parameters.Count - dc);
                    _code.Append(";\n");
                }

            }
        }

        public IncNativePtrValueClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }
    }
}
