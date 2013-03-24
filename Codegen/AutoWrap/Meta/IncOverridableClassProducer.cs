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
    class IncNativeProxyClassProducer : NativeProxyClassProducer
    {
        public IncNativeProxyClassProducer(Wrapper wrapper, ClassDefinition t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }

        protected virtual void AddDefinition()
        {
            _sb.AppendIndent("class " + ProxyName + " : public " + _definition.FullNativeName);
            if (_definition.IsInterface)
                _sb.Append(", public CLRObject");
            _sb.Append("\n");
        }

        protected override void AddPreBody()
        {
            base.AddPreBody();

            AddDefinition();

            _sb.AppendLine("{");
            _sb.AppendLine("public:");
            _sb.IncreaseIndent();
        }

        protected override void AddPostBody()
        {
            base.AddPostBody();

            _sb.DecreaseIndent();
            _sb.AppendLine("};\n");
        }

        protected override void AddFields()
        {
            string className;
            if (_definition.IsNested)
            {
                className = _definition.SurroundingClass.FullCLRName + "::" + _definition.Name;
            }
            else
            {
                className = _wrapper.ManagedNamespace + "::" + _definition.Name;
            }

            _sb.AppendLine("friend ref class " + className + ";");

            //Because of a possible compiler bug, in order for properties to access
            //protected members of a native class, they must be explicitely declared
            //with 'friend' specifier
            foreach (PropertyDefinition prop in _overridableProperties)
            {
                if (prop.GetterFunction.ProtectionType == ProtectionLevel.Protected
                    || (prop.CanWrite && prop.SetterFunction.ProtectionType == ProtectionLevel.Protected))
                    _sb.AppendLine("friend ref class " + className + "::" + prop.Name + ";");
            }

            if (_definition.IsInterface)
            {
                foreach (MemberFieldDefinition field in _definition.Fields)
                {
                    if (!field.IsIgnored
                        && field.ProtectionType == ProtectionLevel.Protected
                        && !field.IsStatic)
                        _sb.AppendLine("friend ref class " + className + "::" + field.Name + ";");
                }
            }

            _sb.AppendEmptyLine();
            _sb.AppendLine("bool* _overriden;");

            _sb.AppendEmptyLine();
            _sb.AppendLine("gcroot<" + className + "^> _managed;");

            //_sb.AppendLine();
            //foreach (DefField field in _protectedFields)
            //{
            //    _sb.AppendLine(field.NativeTypeName + "& ref_" + field.Name + ";");
            //}

            _sb.AppendEmptyLine();
            _sb.AppendLine("virtual void _Init_CLRObject() override { *static_cast<CLRObject*>(this) = _managed; }");
        }

        protected override void AddConstructor(MemberMethodDefinition f)
        {
            string className;
            if (_definition.IsNested)
            {
                className = _definition.SurroundingClass.FullCLRName + "::" + _definition.Name;
            }
            else
            {
                className = _wrapper.ManagedNamespace + "::" + _definition.Name;
            }
            _sb.AppendIndent(ProxyName + "( " + className + "^ managedObj");
            if (f != null)
            {
                foreach (ParamDefinition param in f.Parameters)
                    _sb.Append(", " + param.MemberTypeNativeName + " " + param.Name);
            }

            _sb.Append(" ) :\n");

            if (f != null)
            {
                _sb.AppendIndent("\t" + _definition.FullNativeName + "(");
                for (int i = 0; i < f.Parameters.Count; i++)
                {
                    ParamDefinition param = f.Parameters[i];
                    _sb.Append(" " + param.Name);
                    if (i < f.Parameters.Count - 1)
                        _sb.Append(",");
                }
                _sb.Append(" ),\n");
            }

            _sb.AppendIndent("\t_managed(managedObj)");

            //foreach (DefField field in _protectedFields)
            //{
            //    _sb.Append(",\n");
            //    _sb.AppendIndent("\tref_" + field.Name + "(" + field.Name + ")");
            //}
            _sb.Append("\n");
            _sb.AppendLine("{");
            _sb.AppendLine("}");
        }

        protected override void AddOverridableFunction(MemberMethodDefinition f)
        {
            _sb.AppendIndent("");
            if (f.IsVirtual)
                _sb.Append("virtual ");
            _sb.Append(f.MemberTypeNativeName + " " + f.Name + "(");
            AddNativeMethodParams(f);
            _sb.Append(" ) ");
            if (f.IsConstFunctionCall)
                _sb.Append("const ");
            _sb.Append("override;\n");
        }

        //protected override void AddProtectedFunction(DefFunction f)
        //{
        //    _sb.AppendIndent("");
        //    _sb.Append(f.NativeTypeName + " base_" + f.Name + "(");
        //    AddNativeMethodParams(f);
        //    _sb.Append(" ) ");
        //    if (f.IsConstFunctionCall)
        //        _sb.Append("const ");
        //    _sb.Append(";\n");
        //}
    }

    class IncOverridableClassProducer : IncNonOverridableClassProducer
    {
        public override void Add()
        {
            if (_definition.IsInterface)
            {
                IndentStringBuilder tempsb = _sb;
                _sb = new IndentStringBuilder();
                base.Add();
                string fname = _definition.FullCLRName.Replace(_definition.CLRName, _definition.Name);
                string res = _sb.ToString().Replace(_definition.FullCLRName + "::", fname + "::");
                _sb = tempsb;
                _sb.AppendLine(res);
            }
            else
                base.Add();
        }

        protected override void AddPreDeclarations()
        {
            if (!_definition.IsNested)
            {
                _wrapper.AddPreDeclaration("ref class " + _definition.Name + ";");
                _wrapper.AddPragmaMakePublicForType(_definition);

            }
        }

        protected override void AddDefinition()
        {
            if (_definition.IsInterface)
            {
                //put _t.Name instead of _t.CLRName
                _sb.AppendIndent("");
                if (!_definition.IsNested)
                    _sb.Append("public ");
                else
                    _sb.Append(_definition.ProtectionLevel.GetCLRProtectionName() + ": ");
                string baseclass = GetBaseAndInterfaces();
                if (baseclass != "")
                    _sb.AppendFormat("ref class {0}{1} : {2}\n", _definition.Name, (IsAbstractClass) ? " abstract" : "", baseclass);
                else
                    _sb.AppendFormat("ref class {0}{1}\n", _definition.Name, (IsAbstractClass) ? " abstract" : "");
            }
            else
                base.AddDefinition();
        }

        protected override bool AllowProtectedMembers
        {
            get
            {
                return true;
            }
        }

        protected override bool AllowSubclassing
        {
            get
            {
                return true;
            }
        }

        protected override bool AllowMethodIndexAttributes
        {
            get
            {
                return true;
            }
        }

        public IncOverridableClassProducer(Wrapper wrapper, ClassDefinition t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
            _wrapper.PostClassProducers.Add(new IncNativeProxyClassProducer(_wrapper, _definition, _sb));
        }

        private string _proxyName;
        protected virtual string ProxyName
        {
            get
            {
                if (_proxyName == null)
                    _proxyName = NativeProxyClassProducer.GetProxyName(_definition);

                return _proxyName;
            }
        }

        public override string ClassFullNativeName
        {
            get
            {
                return ProxyName;
            }
        }

        protected override string GetNativeInvokationTarget(MemberMethodDefinition f)
        {
            return "static_cast<" + ProxyName + "*>(_native)->" + f.Class.Name + "::" + f.Name;
        }

        protected override string GetNativeInvokationTarget(MemberFieldDefinition field)
        {
            return "static_cast<" + ProxyName + "*>(_native)->" + _definition.FullNativeName + "::" + field.Name;
        }

        //protected override string GetNativeInvokationTarget(DefFunction f)
        //{
        //    string name = (f.ProtectionType == ProtectionType.Public) ? f.Name : "base_" + f.Name;
        //    return "static_cast<" + ProxyName + "*>(_native)->" + name;
        //}
        //protected override string GetNativeInvokationTarget(DefField field)
        //{
        //    string name = (field.ProtectionType == ProtectionType.Public) ? field.Name : "ref_" + field.Name;
        //    return "static_cast<" + ProxyName + "*>(_native)->" + name;
        //}
        //protected override string GetNativeInvokationTarget(bool isConst)
        //{
        //    string ret = "static_cast<";
        //    if (isConst)
        //        ret += "const ";
        //    return ret + ProxyName + "*>(_native)";
        //}

        protected override void AddManagedNativeConversionsDefinition()
        {
        }

        protected override void AddInternalConstructors()
        {
            // Allow the internal constructor for interfaces too, so that they can be wrapped by a SharedPtr class
            //if (!_t.IsInterface)
                base.AddInternalConstructors();
        }

        protected override void AddDefaultImplementationClass()
        {
        }
    }

    class IncSubclassingClassProducer : IncOverridableClassProducer
    {
        protected ClassDefinition[] _additionalInterfaces;

        public IncSubclassingClassProducer(Wrapper wrapper, ClassDefinition t, IndentStringBuilder sb, ClassDefinition[] additionalInterfaces)
            : base(wrapper, t, sb)
        {
            this._additionalInterfaces = additionalInterfaces;
        }

        protected override void Init()
        {
            _interfaces = new List<ClassDefinition>();

            if (_additionalInterfaces != null)
                _interfaces.AddRange(_additionalInterfaces);

            base.Init();
        }

        protected override bool RequiresCleanUp
        {
            get
            {
                return false;
            }
        }

        protected override bool AllowMethodOverloads
        {
            get
            {
                return false;
            }
        }

        protected override bool DeclareAsOverride(MemberMethodDefinition f)
        {
            if (f.ProtectionType == ProtectionLevel.Public)
                return true;
            else
                return false;
        }

        protected override void AddPreDeclarations()
        {
            if (!_definition.IsNested)
                _wrapper.AddPragmaMakePublicForType(_definition);
        }

        protected override void AddAllNestedTypes()
        {
        }

        protected override void AddPreNestedTypes()
        {
        }

        protected override void AddPostNestedTypes()
        {
        }

        protected override string GetBaseAndInterfaces()
        {
            return _definition.FullCLRName;
        }

        protected override void AddPrivateDeclarations()
        {
            //_sb.DecreaseIndent();
            //_sb.AppendLine("private:");
            //_sb.IncreaseIndent();
        }

        protected override void AddPublicDeclarations()
        {
            _sb.DecreaseIndent();
            _sb.AppendLine("public:");
            _sb.IncreaseIndent();

            AddPublicConstructors();

            _sb.AppendEmptyLine();
            foreach (PropertyDefinition prop in _overridableProperties)
            {
                if (!prop.IsAbstract)
                {
                    AddProperty(prop);
                    _sb.AppendEmptyLine();
                }
            }

            foreach (MemberMethodDefinition func in _overridableFunctions)
            {
                if (!func.IsProperty && func.ProtectionType == ProtectionLevel.Public
                    && !func.IsAbstract)
                {
                    AddMethod(func);
                    _sb.AppendEmptyLine();
                }
            }
        }

        protected override void AddProtectedDeclarations()
        {
            _sb.DecreaseIndent();
            _sb.AppendLine("protected public:");
            _sb.IncreaseIndent();

            foreach (MemberMethodDefinition func in _overridableFunctions)
            {
                if (!func.IsProperty && func.ProtectionType == ProtectionLevel.Protected)
                {
                    AddMethod(func);
                    _sb.AppendEmptyLine();
                }
            }
        }
    }
}

