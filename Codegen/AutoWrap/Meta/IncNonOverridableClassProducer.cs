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
    class IncNonOverridableClassProducer : IncClassProducer
    {
        protected override string GetTopBaseClassName()
        {
            return "Wrapper";
        }

        protected override void AddPublicDeclarations()
        {
            base.AddPublicDeclarations();
            AddManagedNativeConversionsDefinition();
        }

        protected virtual void AddManagedNativeConversionsDefinition()
        {
            _sb.AppendFormatIndent("DEFINE_MANAGED_NATIVE_CONVERSIONS( {0} )\n", GetClassName());
        }

        protected override void AddInternalConstructors()
        {
            base.AddInternalConstructors();
            _sb.AppendFormatIndent("{0}( CLRObject* obj ) : " + GetBaseClassName() + "(obj)\n", _t.Name);
            _sb.AppendLine("{");
            _sb.IncreaseIndent();
            base.AddConstructorBody();
            _sb.DecreaseIndent();
            _sb.AppendLine("}\n");
        }

        protected override void AddPostBody()
        {
            base.AddPostBody();
            AddDefaultImplementationClass();
        }

        protected virtual void AddDefaultImplementationClass()
        {
            if (IsAbstractClass)
            {
                _sb.AppendLine("ref class " + _t.CLRName + "_Default : public " + _t.CLRName);
                _sb.AppendLine("{");
                _sb.AppendLine("public protected:");
                _sb.IncreaseIndent();
                _sb.AppendFormatIndent("{0}_Default( CLRObject* obj ) : {0}(obj)\n", _t.CLRName);
                _sb.AppendLine("{");
                _sb.AppendLine("}\n");
                _sb.DecreaseIndent();
                _sb.AppendLine("public:");
                _sb.IncreaseIndent();

                foreach (DefFunction f in _abstractFunctions)
                {
                    _sb.AppendIndent("virtual ");
                    _sb.Append(GetCLRTypeName(f) + " " + f.CLRName);
                    AddMethodParameters(f, f.Parameters.Count);
                    _sb.Append(" override;\n");
                }

                foreach (DefProperty p in _abstractProperties)
                {
                    string ptype = GetCLRTypeName(p);
                    _sb.AppendFormatIndent("property {0} {1}\n{{\n", ptype, p.Name);
                    if (p.CanRead)
                    {
                        _sb.AppendLine(GetProtectionString(p.GetterFunction.ProtectionType) + ":");
                        _sb.AppendLine("\tvirtual " + ptype + " get() override;");
                    }
                    if (p.CanWrite)
                    {
                        _sb.AppendLine(GetProtectionString(p.SetterFunction.ProtectionType) + ":");
                        _sb.AppendLine("\tvirtual void set(" + ptype + " " + p.SetterFunction.Parameters[0].Name + ") override;");
                    }
                    _sb.AppendLine("}");
                }

                _sb.DecreaseIndent();
                _sb.AppendLine("};\n");
            }
        }

        public IncNonOverridableClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }
    }
}
