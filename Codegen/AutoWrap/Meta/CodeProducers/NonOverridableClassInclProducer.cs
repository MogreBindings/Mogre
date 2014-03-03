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
    class NonOverridableClassInclProducer : ClassInclProducer
    {
        protected override string GetTopBaseClassName()
        {
            return "Wrapper";
        }

        protected override void GenerateCodePublicDeclarations()
        {
            base.GenerateCodePublicDeclarations();
            AddManagedNativeConversionsDefinition();
        }

        protected virtual void AddManagedNativeConversionsDefinition()
        {
            _codeBuilder.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS( {0} )", GetClassName());
        }

        protected override void AddInternalConstructors()
        {
            base.AddInternalConstructors();
            _codeBuilder.AppendLine("{0}( CLRObject* obj ) : " + GetBaseClassName() + "(obj)", _classDefinition.Name);
            _codeBuilder.BeginBlock();
            base.GenerateCodeConstructorBody();
            _codeBuilder.EndBlock();
            _codeBuilder.AppendEmptyLine();
        }

        protected override void GenerateCodePostBody()
        {
            base.GenerateCodePostBody();
            AddDefaultImplementationClass();
        }

        protected virtual void AddDefaultImplementationClass()
        {
            if (IsAbstractClass)
            {
                _codeBuilder.AppendLine("ref class " + _classDefinition.CLRName + "_Default : public " + _classDefinition.CLRName);
                _codeBuilder.AppendLine("{");
                _codeBuilder.AppendLine("public protected:");
                _codeBuilder.IncreaseIndent();
                _codeBuilder.AppendLine("{0}_Default( CLRObject* obj ) : {0}(obj)", _classDefinition.CLRName);
                _codeBuilder.AppendLine("{");
                _codeBuilder.AppendLine("}\n");
                _codeBuilder.DecreaseIndent();
                _codeBuilder.AppendLine("public:");
                _codeBuilder.IncreaseIndent();

                foreach (MemberMethodDefinition f in _abstractFunctions)
                {
                    _codeBuilder.Append("virtual ");
                    _codeBuilder.Append(GetCLRTypeName(f) + " " + f.CLRName);
                    AddMethodParameters(f, f.Parameters.Count);
                    _codeBuilder.AppendLine(" override;");
                }

                foreach (MemberPropertyDefinition p in _abstractProperties)
                {
                    string ptype = GetCLRTypeName(p);
                    _codeBuilder.AppendLine("property {0} {1}", ptype, p.Name);
                    _codeBuilder.BeginBlock();
                    if (p.CanRead)
                    {
                        _codeBuilder.DecreaseIndent();
                        _codeBuilder.AppendLine(p.GetterFunction.ProtectionLevel.GetCLRProtectionName() + ":");
                        _codeBuilder.IncreaseIndent();
                        _codeBuilder.AppendLine("virtual " + ptype + " get() override;");
                    }
                    if (p.CanWrite)
                    {
                        _codeBuilder.DecreaseIndent();
                        _codeBuilder.AppendLine(p.SetterFunction.ProtectionLevel.GetCLRProtectionName() + ":");
                        _codeBuilder.IncreaseIndent();
                        _codeBuilder.AppendLine("virtual void set(" + ptype + " " + p.SetterFunction.Parameters[0].Name + ") override;");
                    }
                    _codeBuilder.EndBlock();
                }

                _codeBuilder.DecreaseIndent();
                _codeBuilder.AppendLine("};\n");
            }
        }

        public NonOverridableClassInclProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }
    }
}
