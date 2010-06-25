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
    //class IncNativeProtectedTypesProxy : NativeProtectedTypesProxy
    //{
    //    public IncNativeProtectedTypesProxy(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
    //        : base(wrapper, t, sb)
    //    {
    //    }

    //    protected override void AddPreBody()
    //    {
    //        AddDefinition();
    //        _sb.AppendLine("{");
    //        _sb.AppendLine("public:");
    //        _sb.IncreaseIndent();
    //    }

    //    protected override void AddPostBody()
    //    {
    //        _sb.DecreaseIndent();
    //        _sb.AppendLine("};\n");
    //    }

    //    protected virtual void AddDefinition()
    //    {
    //        string className = GetProtectedTypesProxyName(_t);
    //        className = className.Substring(className.IndexOf("::") + 2);
    //        _sb.AppendLine("class " + className + " : public " + _t.FullNativeName);
    //    }

    //    protected override void AddNestedType(DefType nested)
    //    {
    //        if (nested.IsSTLContainer)
    //        {
    //            _sb.AppendLine("typedef " + nested.FullNativeName + " " + nested.CLRName + ";");
    //        }
    //        else if (nested is DefEnum)
    //        {
    //            _wrapper.IncAddEnum(nested as DefEnum, _sb, true);
    //        }
    //        else
    //            throw new Exception("Unexpected");
    //    }

    //    protected override void AddStaticField(DefField field)
    //    {
    //        _sb.AppendIndent("static ");
    //        _sb.Append(field.NativeTypeName + "& ref_" + field.Name + ";\n");
    //    }

    //    protected override void AddMethod(DefFunction f)
    //    {
    //        // method is supposed to be static

    //        _sb.AppendIndent(f.Definition.Replace(f.Class.FullNativeName + "::" + f.Name, "base_" + f.Name) + "(");
    //        AddNativeMethodParams(f);
    //        _sb.Append(" ) ");
    //        if (f.IsConstFunctionCall)
    //            _sb.Append("const ");
    //        _sb.Append(";\n");
    //    }
    //}
}
