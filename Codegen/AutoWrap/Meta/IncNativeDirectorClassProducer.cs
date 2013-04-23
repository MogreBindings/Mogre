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
    class IncNativeDirectorClassProducer : ClassInclCodeProducer
    {
        public override bool IsNativeClass
        {
            get { return true; }
        }

        protected override string GetTopBaseClassName()
        {
            return null;
        }

        protected override bool AllowMethodOverloads
        {
            get { return false; }
        }

        public IncNativeDirectorClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }

        protected override void AddInternalDeclarations()
        {
        }

        protected virtual string ReceiverInterfaceName
        {
            get
            {
                return GetNativeDirectorReceiverInterfaceName(_definition);
            }
        }

        protected virtual string DirectorName
        {
            get
            {
                return GetNativeDirectorName(_definition);
            }
        }

        protected override void AddPreBody()
        {
            _code.AppendLine("interface class " + ReceiverInterfaceName + "\n{");
            _code.IncreaseIndent();
            foreach (MemberMethodDefinition f in _definition.PublicMethods)
            {
                if (f.IsDeclarableFunction && f.IsVirtual)
                {
                    base.AddMethod(f);
                    _code.AppendEmptyLine();
                }
            }
            _code.DecreaseIndent();
            _code.AppendLine("};\n");

            if (!_definition.IsNested)
            {
                AddMethodHandlersClass(_definition, _code);
            }

            base.AddPreBody();
        }

        public static void AddMethodHandlersClass(ClassDefinition type, SourceCodeStringBuilder sb)
        {
            if (!type.HasWrapType(WrapTypes.NativeDirector))
                throw new Exception("Unexpected");

            if (type.IsNested)
                sb.AppendIndent("public: ");
            else
                sb.AppendIndent("public ");

            sb.Append("ref class " + type.Name + " abstract sealed\n");
            sb.AppendLine("{");
            sb.AppendLine("public:");
            sb.IncreaseIndent();

            foreach (MemberMethodDefinition f in type.PublicMethods)
            {
                if (f.IsDeclarableFunction && f.IsVirtual)
                {
                    //if (f.Parameters.Count > 0)
                    //{
                    //    AddEventArgsClass(f, sb);
                    //}

                    sb.AppendIndent("delegate static " + f.MemberTypeCLRName + " " + f.CLRName + "Handler(");
                    for (int i = 0; i < f.Parameters.Count; i++)
                    {
                        ParamDefinition param = f.Parameters[i];
                        sb.Append(" " + param.Type.GetCLRParamTypeName(param) + " " + param.Name);
                        if (i < f.Parameters.Count - 1) sb.Append(",");
                    }
                    sb.Append(" );\n");
                }
            }

            sb.DecreaseIndent();
            sb.AppendLine("};");
            sb.AppendEmptyLine();
        }

        //protected static void AddEventArgsClass(DefFunction func, IndentStringBuilder sb)
        //{
        //    string className = func.CLRName + "EventArgs";
        //    sb.AppendLine("ref class " + className + " : EventArgs");
        //    sb.AppendLine("{");
        //    sb.AppendLine("public:");
        //    sb.IncreaseIndent();

        //    foreach (DefParam param in func.Parameters)
        //        sb.AppendLine(param.Type.GetCLRParamTypeName(param) + " " + param.Name + ";");

        //    sb.AppendLine();
        //    sb.AppendIndent(className + "(");
        //    for (int i = 0; i < func.Parameters.Count; i++)
        //    {
        //        DefParam param = func.Parameters[i];
        //        sb.Append(" " + param.Type.GetCLRParamTypeName(param) + " " + param.Name);
        //        if (i < func.Parameters.Count - 1) sb.Append(",");
        //    }
        //    sb.Append(" )\n");

        //    sb.AppendLine("{");
        //    sb.IncreaseIndent();
        //    foreach (DefParam param in func.Parameters)
        //        sb.AppendLine("this->" + param.Name + ";");

        //}

        protected override void AddPrivateDeclarations()
        {
            base.AddPrivateDeclarations();
            _code.AppendLine("gcroot<" + ReceiverInterfaceName + "^> _receiver;");
        }

        protected override void AddPublicConstructors()
        {
            _code.AppendLine(DirectorName + "( " + ReceiverInterfaceName + "^ recv )");
            _code.AppendIndent("\t: _receiver(recv)");
            foreach (MemberMethodDefinition f in _definition.PublicMethods)
            {
                if (f.IsDeclarableFunction && f.IsVirtual)
                {
                    _code.Append(", doCallFor" + f.CLRName + "(false)");
                }
            }
            _code.Append("\n");
            _code.AppendLine("{");
            _code.AppendLine("}");
        }

        protected override void AddPublicFields()
        {
            base.AddPublicFields();
            foreach (MemberMethodDefinition f in _definition.PublicMethods)
            {
                if (f.IsDeclarableFunction && f.IsVirtual)
                {
                    _code.AppendLine("bool doCallFor" + f.CLRName + ";");
                }
            }
        }

        protected override void AddMethod(MemberMethodDefinition f)
        {
            _code.AppendIndent(f.Definition.Replace(f.ContainingClass.FullyQualifiedNativeName + "::", "") + "(");
            for (int i = 0; i < f.Parameters.Count; i++)
            {
                ParamDefinition param = f.Parameters[i];
                _code.Append(" ");
                AddNativeMethodParam(param);
                if (i < f.Parameters.Count - 1) _code.Append(",");
            }
            _code.Append(" ) override;\n");
        }

        protected virtual void AddNativeMethodParam(ParamDefinition param)
        {
            _code.Append(param.MemberTypeNativeName + " " + param.Name);
        }

        protected override void AddDefinition()
        {
            _code.AppendLine("class " + DirectorName + " : public " + _definition.FullyQualifiedNativeName);
        }

        protected override void AddProtectedDeclarations()
        {
        }
    }
}
