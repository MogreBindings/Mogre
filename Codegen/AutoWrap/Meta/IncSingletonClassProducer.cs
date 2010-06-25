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
    class IncSingletonClassProducer : IncClassProducer
    {
        protected override string GetTopBaseClassName()
        {
            return null;
        }

        protected override bool DoCleanupInFinalizer
        {
            get
            {
                return true;
            }
        }

        protected override bool AllowProperty(DefProperty p)
        {
            if (base.AllowProperty(p))
            {
                if (p.Name == "Singleton" || p.Name == "SingletonPtr")
                    return false;
                else
                    return true;
            }
            else
                return false;
        }

        protected override void AddDisposerBody()
        {
            _sb.AppendLine("_native = " + _t.FullNativeName + "::getSingletonPtr();");

            base.AddDisposerBody();

            _sb.AppendLine("if (_createdByCLR && _native) { delete _native; _native = 0; }");
            _sb.AppendLine("_singleton = nullptr;");
        }

        protected override void AddPrivateDeclarations()
        {
            base.AddPrivateDeclarations();
            _sb.AppendLine("static " + _t.CLRName + "^ _singleton;");

            if (_t.BaseClass == null)
            {
                _sb.AppendLine(_t.FullNativeName + "* _native;");
                _sb.AppendLine("bool _createdByCLR;");
            }
        }

        protected override void AddInternalConstructors()
        {
            base.AddInternalConstructors();

            if (_t.BaseClass == null)
                _sb.AppendLine(_t.CLRName + "( " + _t.FullNativeName + "* obj ) : _native(obj)");
            else
                _sb.AppendLine(_t.CLRName + "( " + _t.FullNativeName + "* obj ) : " + _t.BaseClass.CLRName + "(obj)");

            _sb.AppendLine("{");
            _sb.IncreaseIndent();
            base.AddConstructorBody();
            _sb.DecreaseIndent();
            _sb.AppendLine("}\n");
        }

        protected override void AddPublicFields()
        {
            _sb.AppendLine("static property " + _t.CLRName + "^ Singleton");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();
            _sb.AppendLine(_t.CLRName + "^ get()");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();
                
            _sb.AppendLine(_t.FullNativeName + "* ptr = " + _t.FullNativeName + "::getSingletonPtr();");
            _sb.AppendLine("if (_singleton == CLR_NULL || _singleton->_native != ptr)");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();
            _sb.AppendLine("if (_singleton != CLR_NULL)");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();
            _sb.AppendLine("_singleton->_native = 0;");
            _sb.AppendLine("_singleton = nullptr;");
            _sb.DecreaseIndent();
            _sb.AppendLine("}");

            _sb.AppendLine("if ( ptr ) _singleton = gcnew " + _t.CLRName + "( ptr );");
            _sb.DecreaseIndent();
            _sb.AppendLine("}");
            _sb.AppendLine("return _singleton;");
            _sb.DecreaseIndent();
            _sb.AppendLine("}");
            _sb.DecreaseIndent();
            _sb.AppendLine("}");

            base.AddPublicFields();
        }

        public IncSingletonClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }
    }
}
