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
    class IncCLRHandleClassProducer : IncPlainWrapperClassProducer
    {
        protected override string GetTopBaseClassName()
        {
            return "INativePointer";
        }

        protected override bool DoCleanupInFinalizer
        {
            get { return _t.HasAttribute<DoCleanupInFinalizerAttribute>(); }
        }

        protected override void AddPrivateDeclarations()
        {
            base.AddPrivateDeclarations();
            _sb.AppendLine();
            _sb.AppendLine("virtual void ClearNativePtr() = INativePointer::ClearNativePtr");
            _sb.AppendLine("{");
            _sb.AppendLine("\t_native = 0;");
            _sb.AppendLine("}");
        }

        protected override void AddManagedNativeConversionsDefinition()
        {
            _sb.AppendFormatIndent("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_CLRHANDLE( {0} )\n", GetClassName());
        }

        public IncCLRHandleClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }
    }
}
