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
    //class CppNativeProtectedTypesProxy : NativeProtectedTypesProxy
    //{
    //    public CppNativeProtectedTypesProxy(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
    //        : base(wrapper, t, sb)
    //    {
    //    }

    //    protected string ProxyName
    //    {
    //        get
    //        {
    //            return NativeProtectedTypesProxy.GetProtectedTypesProxyName(_t);
    //        }
    //    }

    //    protected override void AddNestedType(DefType nested)
    //    {
    //    }

    //    protected override void AddStaticField(DefField field)
    //    {
    //        _sb.AppendIndent("");
    //        _sb.Append(field.NativeTypeName + "& " + ProxyName + "::ref_" + field.Name + " = " + field.Class.FullNativeName + "::" + field.Name + ";\n");
    //    }

    //    protected override void AddMethod(DefFunction f)
    //    {
    //        // method is supposed to be static

    //        _sb.AppendIndent("");
    //        _sb.Append(f.NativeTypeName + " " + ProxyName + "::base_" + f.Name + "(");
    //        AddNativeMethodParams(f);
    //        _sb.Append(" )");
    //        if (f.IsConstFunctionCall)
    //            _sb.Append(" const");
    //        _sb.Append("\n");
    //        _sb.AppendLine("{");
    //        _sb.IncreaseIndent();

    //        _sb.AppendIndent("");
    //        if (!f.IsVoid)
    //            _sb.Append("return ");
    //        _sb.Append(_t.FullNativeName + "::" + f.Name + "(");

    //        for (int i = 0; i < f.Parameters.Count; i++)
    //        {
    //            DefParam param = f.Parameters[i];
    //            _sb.Append(" " + param.Name);
    //            if (i < f.Parameters.Count - 1) _sb.Append(",");
    //        }
    //        _sb.Append(" );\n");

    //        _sb.DecreaseIndent();
    //        _sb.AppendLine("}");
    //    }
    //}
}
