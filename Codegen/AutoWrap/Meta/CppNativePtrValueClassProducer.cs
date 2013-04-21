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
using System.Reflection;

namespace AutoWrap.Meta
{
    class CppNativePtrValueClassProducer : ClassCppCodeProducer
    {
        protected override string GetNativeInvokationTarget(bool isConst)
        {
            return "_native";
        }

        protected override string GetNativeInvokationTargetObject()
        {
            return "*_native";
        }

        protected override void AddPublicDeclarations()
        {
            base.AddPublicDeclarations();

            if (!IsReadOnly && IsConstructable)
            {
                _code.AppendEmptyLine();
                AddCreators();
            }
        }

        protected override void AddPublicConstructor(MemberMethodDefinition f)
        {
        }

        protected virtual void AddCreators()
        {
            if (!_definition.IsNativeAbstractClass)
            {
                if (_definition.Constructors.Length > 0)
                {
                    foreach (MemberMethodDefinition func in _definition.Constructors)
                        if (func.ProtectionLevel == ProtectionLevel.Public)
                            AddCreator(func);
                }
                else
                    AddCreator(null);

                _code.AppendEmptyLine();
            }
        }

        protected virtual void AddCreator(MemberMethodDefinition f)
        {
            if (f == null)
                AddCreatorOverload(f, 0);
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

                    AddCreatorOverload(f, f.Parameters.Count - dc);
                }

            }
        }

        protected virtual void AddCreatorOverload(MemberMethodDefinition f, int count)
        {
            _code.AppendIndent(_definition.FullCLRName + " " + GetClassName() + "::Create");
            if (f == null)
                _code.Append("()");
            else
                AddMethodParameters(f, count);

            _code.Append("\n");
            _code.AppendLine("{");
            _code.IncreaseIndent();

            string preCall = null, postCall = null;

            if (f != null)
            {
                preCall = GetMethodPreNativeCall(f, count);
                postCall = GetMethodPostNativeCall(f, count);

                if (!String.IsNullOrEmpty(preCall))
                    _code.AppendLine(preCall);
            }

            _code.AppendLine(_definition.CLRName + " ptr;");
            _code.AppendIndent("ptr._native = new " + _definition.FullNativeName + "(");

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    ParamDefinition p = f.Parameters[i];
                    string newname;
                    p.Type.ProducePreCallParamConversionCode(p, out newname);
                    _code.Append(" " + newname);
                    if (i < count - 1) _code.Append(",");
                }
            }

            _code.Append(");\n");

            if (!String.IsNullOrEmpty(postCall))
            {
                _code.AppendEmptyLine();
                _code.AppendLine(postCall);
                _code.AppendEmptyLine();
            }

            _code.AppendLine("return ptr;");

            _code.DecreaseIndent();
            _code.AppendLine("}");
        }

        public CppNativePtrValueClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }
    }
}
