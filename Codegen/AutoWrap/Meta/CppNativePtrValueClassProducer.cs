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
    class CppNativePtrValueClassProducer : CppClassProducer
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
                _sb.AppendLine();
                AddCreators();
            }
        }

        protected override void AddPublicConstructor(DefFunction f)
        {
        }

        protected virtual void AddCreators()
        {
            if (!_t.IsNativeAbstractClass)
            {
                if (_t.Constructors.Length > 0)
                {
                    foreach (DefFunction func in _t.Constructors)
                        if (func.ProtectionType == ProtectionType.Public)
                            AddCreator(func);
                }
                else
                    AddCreator(null);

                _sb.AppendLine();
            }
        }

        protected virtual void AddCreator(DefFunction f)
        {
            if (f == null)
                AddCreatorOverload(f, 0);
            else
            {
                int defcount = 0;

                if (!f.HasAttribute<NoDefaultParamOverloadsAttribute>())
                {
                    foreach (DefParam param in f.Parameters)
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

        protected virtual void AddCreatorOverload(DefFunction f, int count)
        {
            _sb.AppendIndent(_t.FullCLRName + " " + GetClassName() + "::Create");
            if (f == null)
                _sb.Append("()");
            else
                AddMethodParameters(f, count);

            _sb.Append("\n");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();

            string preCall = null, postCall = null;

            if (f != null)
            {
                preCall = GetMethodPreNativeCall(f, count);
                postCall = GetMethodPostNativeCall(f, count);

                if (!String.IsNullOrEmpty(preCall))
                    _sb.AppendLine(preCall);
            }

            _sb.AppendLine(_t.CLRName + " ptr;");
            _sb.AppendIndent("ptr._native = new " + _t.FullNativeName + "(");

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    DefParam p = f.Parameters[i];
                    string newname;
                    p.Type.GetPreCallParamConversion(p, out newname);
                    _sb.Append(" " + newname);
                    if (i < count - 1) _sb.Append(",");
                }
            }

            _sb.Append(");\n");

            if (!String.IsNullOrEmpty(postCall))
            {
                _sb.AppendLine();
                _sb.AppendLine(postCall);
                _sb.AppendLine();
            }

            _sb.AppendLine("return ptr;");

            _sb.DecreaseIndent();
            _sb.AppendLine("}");
        }

        public CppNativePtrValueClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }
    }
}
