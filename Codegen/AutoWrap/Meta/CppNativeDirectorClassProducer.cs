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
    class CppNativeDirectorClassProducer : CppClassProducer
    {
        public override bool IsNativeClass
        {
            get { return true; }
        }

        public CppNativeDirectorClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }

        protected override void AddPublicConstructor(DefFunction f)
        {
        }

        protected override string GetClassName()
        {
            string full = _t.FullCLRName;
            int index = full.IndexOf("::");
            string name = full.Substring(index + 2);

            index = name.LastIndexOf("::");
            if (index == -1)
                return GetNativeDirectorName(_t);

            if (!_t.IsNested)
            {
                return name.Substring(0, index + 2) + GetNativeDirectorName(_t);
            }
            else
            {
                name = name.Substring(0, index);
                index = name.LastIndexOf("::");
                if (index == -1)
                    return GetNativeDirectorName(_t);
                else
                    return name.Substring(0, index + 2) + GetNativeDirectorName(_t);
            }
        }

        protected override void AddMethod(DefFunction f)
        {
            string def = f.Definition.Replace(f.Class.FullNativeName, GetClassName()) + "(";
            if (def.StartsWith("virtual "))
                def = def.Substring("virtual ".Length);
            _sb.AppendIndent(def);
            for (int i = 0; i < f.Parameters.Count; i++)
            {
                DefParam param = f.Parameters[i];
                _sb.Append(" ");
                AddNativeMethodParam(param);
                if (i < f.Parameters.Count - 1) _sb.Append(",");
            }
            _sb.Append(" )\n");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();

            _sb.AppendLine("if (doCallFor" + f.CLRName + ")");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();

            CppNativeProxyClassProducer.AddNativeProxyMethodBody(f, "_receiver", _sb);

            _sb.DecreaseIndent();
            _sb.AppendLine("}");
            if (!f.IsVoid)
            {
                _sb.AppendLine("else");
                string ret = null;
                if (f.HasAttribute<DefaultReturnValueAttribute>())
                    ret = f.GetAttribute<DefaultReturnValueAttribute>().Name;
                else
                    throw new Exception("Default return value not set.");
                _sb.AppendLine("\treturn " + ret + ";");
            }

            _sb.DecreaseIndent();
            _sb.AppendLine("}");
        }

        protected virtual void AddNativeMethodParam(DefParam param)
        {
            _sb.Append(param.NativeTypeName + " " + param.Name);
        }
    }
}
