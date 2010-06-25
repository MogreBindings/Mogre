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
    class CppNonOverridableClassProducer : CppClassProducer
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
            _sb.AppendLine();
            _sb.AppendLine("_native->_MapToCLRObject(this, System::Runtime::InteropServices::GCHandleType::Normal);");
        }

        protected virtual void AddDefaultImplementationClass()
        {
            if (IsAbstractClass)
            {
                string className = GetClassName() + "_Default";
                foreach (DefFunction f in _abstractFunctions)
                {
                    _sb.AppendIndent(GetCLRTypeName(f) + " " + className + "::" + f.CLRName);
                    AddMethodParameters(f, f.Parameters.Count);
                    _sb.Append("\n");
                    _sb.AppendLine("{");
                    _sb.IncreaseIndent();
                    AddMethodBody(f, f.Parameters.Count);
                    _sb.DecreaseIndent();
                    _sb.AppendLine("}\n");
                }

                foreach (DefProperty p in _abstractProperties)
                {
                    string ptype = GetCLRTypeName(p);
                    string pname = className + "::" + p.Name;
                    if (p.CanRead)
                    {
                        string managedType = GetMethodNativeCall(p.GetterFunction, 0);

                        _sb.AppendLine(ptype + " " + pname + "::get()");
                        _sb.AppendLine("{");
                        if (_cachedMembers.Contains(p.GetterFunction))
                        {
                            string priv = NameToPrivate(p.Name);
                            _sb.AppendLine("\treturn ( CLR_NULL == " + priv + " ) ? (" + priv + " = " + managedType + ") : " + priv + ";");
                        }
                        else
                        {
                            _sb.AppendLine("\treturn " + managedType + ";");
                        }
                        _sb.AppendLine("}\n");
                    }

                    if (p.CanWrite)
                    {
                        _sb.AppendLine("void " + pname + "::set( " + ptype + " " + p.SetterFunction.Parameters[0].Name + " )");
                        _sb.AppendLine("{");
                        _sb.IncreaseIndent();

                        AddMethodBody(p.SetterFunction, 1);

                        _sb.DecreaseIndent();
                        _sb.AppendLine("}\n");
                    }
                }

                _sb.AppendLine();
            }
        }

        public CppNonOverridableClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }
    }
}
