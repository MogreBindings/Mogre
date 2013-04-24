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
    class IncPlainWrapperClassProducer : ClassInclCodeProducer
    {
        protected override string GetTopBaseClassName()
        {
            return null;
        }

        protected override bool RequiresCleanUp
        {
            get { return _classDefinition.BaseClass == null; }
        }

        protected override bool DoCleanupInFinalizer
        {
            get { return !_classDefinition.HasAttribute<NoFinalizerAttribute>(); }
        }

        protected override void AddInternalDeclarations()
        {
            base.AddInternalDeclarations();

            if (_classDefinition.BaseClass == null)
            {
                _codeBuilder.AppendLine(_classDefinition.FullyQualifiedNativeName + "* _native;");
                _codeBuilder.AppendLine("bool _createdByCLR;");
                _codeBuilder.AppendEmptyLine();
            }
        }

        protected override void AddPublicDeclarations()
        {
            base.AddPublicDeclarations();
            AddManagedNativeConversionsDefinition();
        }

        protected virtual void AddManagedNativeConversionsDefinition()
        {
            if (_classDefinition.Name == _classDefinition.CLRName)
                _codeBuilder.AppendFormatIndent("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_PLAINWRAPPER( {0} )\n", GetClassName());
            else
            {
                string clrName = _classDefinition.FullyQualifiedCLRName.Substring(_classDefinition.FullyQualifiedCLRName.IndexOf("::") + 2);
                string nativeName = _classDefinition.FullyQualifiedNativeName.Substring(_classDefinition.FullyQualifiedNativeName.IndexOf("::") + 2);
                _codeBuilder.AppendFormatIndent("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_PLAINWRAPPER_EXPLICIT( {0}, {1} )\n", clrName, nativeName);
            }
        }

        protected override void AddInternalConstructors()
        {
            base.AddInternalConstructors();

            if (_classDefinition.BaseClass == null)
            {
                _codeBuilder.AppendFormatIndent("{0}( " + _classDefinition.FullyQualifiedNativeName + "* obj ) : _native(obj), _createdByCLR(false)\n", _classDefinition.CLRName);
            }
            else
            {
                ClassDefinition topclass = GetTopClass(_classDefinition);
                _codeBuilder.AppendFormatIndent("{0}( " + topclass.FullyQualifiedNativeName + "* obj ) : " + topclass.CLRName + "(obj)\n", _classDefinition.CLRName);
            }
            _codeBuilder.AppendLine("{");
            _codeBuilder.IncreaseIndent();

            //NOTE: SuppressFinalize should not be called when the class is 'wrapped' by a SharedPtr class, (i.e DataStreamPtr -> DataStream)
            //so that the SharedPtr class gets a chance to clean up. Look for a way to have SuppressFinalize without this kind of problems.
            //_sb.AppendLine("System::GC::SuppressFinalize(this);");

            base.AddConstructorBody();
            _codeBuilder.DecreaseIndent();
            _codeBuilder.AppendLine("}\n");
        }

        protected override void AddDisposerBody()
        {
            base.AddDisposerBody();
            _codeBuilder.AppendLine("if (_createdByCLR &&_native)");
            _codeBuilder.AppendLine("{");
            _codeBuilder.AppendLine("\tdelete _native;");
            _codeBuilder.AppendLine("\t_native = 0;");
            _codeBuilder.AppendLine("}");
        }

        public IncPlainWrapperClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }
    }
}
