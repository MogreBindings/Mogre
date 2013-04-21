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
    class IncNonOverridableClassProducer : ClassInclCodeProducer
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
            _code.AppendFormatIndent("DEFINE_MANAGED_NATIVE_CONVERSIONS( {0} )\n", GetClassName());
        }

        protected override void AddInternalConstructors()
        {
            base.AddInternalConstructors();
            _code.AppendFormatIndent("{0}( CLRObject* obj ) : " + GetBaseClassName() + "(obj)\n", _definition.Name);
            _code.AppendLine("{");
            _code.IncreaseIndent();
            base.AddConstructorBody();
            _code.DecreaseIndent();
            _code.AppendLine("}\n");
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
                _code.AppendLine("ref class " + _definition.CLRName + "_Default : public " + _definition.CLRName);
                _code.AppendLine("{");
                _code.AppendLine("public protected:");
                _code.IncreaseIndent();
                _code.AppendFormatIndent("{0}_Default( CLRObject* obj ) : {0}(obj)\n", _definition.CLRName);
                _code.AppendLine("{");
                _code.AppendLine("}\n");
                _code.DecreaseIndent();
                _code.AppendLine("public:");
                _code.IncreaseIndent();

                foreach (MemberMethodDefinition f in _abstractFunctions)
                {
                    _code.AppendIndent("virtual ");
                    _code.Append(GetCLRTypeName(f) + " " + f.CLRName);
                    AddMethodParameters(f, f.Parameters.Count);
                    _code.Append(" override;\n");
                }

                foreach (MemberPropertyDefinition p in _abstractProperties)
                {
                    string ptype = GetCLRTypeName(p);
                    _code.AppendFormatIndent("property {0} {1}\n{{\n", ptype, p.Name);
                    if (p.CanRead)
                    {
                        _code.AppendLine(p.GetterFunction.ProtectionLevel.GetCLRProtectionName() + ":");
                        _code.AppendLine("\tvirtual " + ptype + " get() override;");
                    }
                    if (p.CanWrite)
                    {
                        _code.AppendLine(p.SetterFunction.ProtectionLevel.GetCLRProtectionName() + ":");
                        _code.AppendLine("\tvirtual void set(" + ptype + " " + p.SetterFunction.Parameters[0].Name + ") override;");
                    }
                    _code.AppendLine("}");
                }

                _code.DecreaseIndent();
                _code.AppendLine("};\n");
            }
        }

        public IncNonOverridableClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }
    }
}
