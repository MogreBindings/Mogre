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
    class CppNonOverridableClassProducer : ClassCppCodeProducer
    {
        protected override string GetTopBaseClassName()
        {
            return "Wrapper";
        }

        protected override void AddPostBody()
        {
            base.AddPostBody();
            AddDefaultImplementationClass();
        }

        protected override void AddConstructorBody()
        {
            base.AddConstructorBody();
            _code.AppendEmptyLine();
            _code.AppendLine("_native->_MapToCLRObject(this, System::Runtime::InteropServices::GCHandleType::Normal);");
        }

        protected virtual void AddDefaultImplementationClass()
        {
            if (IsAbstractClass)
            {
                string className = GetClassName() + "_Default";
                foreach (MemberMethodDefinition f in _abstractFunctions)
                {
                    _code.AppendIndent(GetCLRTypeName(f) + " " + className + "::" + f.CLRName);
                    AddMethodParameters(f, f.Parameters.Count);
                    _code.Append("\n");
                    _code.AppendLine("{");
                    _code.IncreaseIndent();
                    AddMethodBody(f, f.Parameters.Count);
                    _code.DecreaseIndent();
                    _code.AppendLine("}\n");
                }

                foreach (MemberPropertyDefinition p in _abstractProperties)
                {
                    string ptype = GetCLRTypeName(p);
                    string pname = className + "::" + p.Name;
                    if (p.CanRead)
                    {
                        string managedType = GetMethodNativeCall(p.GetterFunction, 0);

                        _code.AppendLine(ptype + " " + pname + "::get()");
                        _code.AppendLine("{");
                        if (_cachedMembers.Contains(p.GetterFunction))
                        {
                            string priv = NameToPrivate(p.Name);
                            _code.AppendLine("\treturn ( CLR_NULL == " + priv + " ) ? (" + priv + " = " + managedType + ") : " + priv + ";");
                        }
                        else
                        {
                            _code.AppendLine("\treturn " + managedType + ";");
                        }
                        _code.AppendLine("}\n");
                    }

                    if (p.CanWrite)
                    {
                        _code.AppendLine("void " + pname + "::set( " + ptype + " " + p.SetterFunction.Parameters[0].Name + " )");
                        _code.AppendLine("{");
                        _code.IncreaseIndent();

                        AddMethodBody(p.SetterFunction, 1);

                        _code.DecreaseIndent();
                        _code.AppendLine("}\n");
                    }
                }

                _code.AppendEmptyLine();
            }
        }

        public CppNonOverridableClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }
    }
}
