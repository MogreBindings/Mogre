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
        public IncNativeProxyClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }

        protected virtual void AddDefinition()
        {
            _sb.AppendIndent("class " + ProxyName + " : public " + _t.FullNativeName);
            if (_t.IsInterface)
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
            if (_t.IsNested)
            {
                className = _t.ParentClass.FullCLRName + "::" + _t.Name;
            }
            else
            {
                className = _wrapper.ManagedNamespace + "::" + _t.Name;
            }

            _sb.AppendLine("friend ref class " + className + ";");

            //Because of a possible compiler bug, in order for properties to access
            //protected members of a native class, they must be explicitely declared
            //with 'friend' specifier
            foreach (DefProperty prop in _overridableProperties)
            {
                if (prop.GetterFunction.ProtectionType == ProtectionType.Protected
                    || (prop.CanWrite && prop.SetterFunction.ProtectionType == ProtectionType.Protected))
                    _sb.AppendLine("friend ref class " + className + "::" + prop.Name + ";");
            }

            if (_t.IsInterface)
            {
                foreach (DefField field in _t.Fields)
                {
                    if (!field.IsIgnored
                        && field.ProtectionType == ProtectionType.Protected
                        && !field.IsStatic)
                        _sb.AppendLine("friend ref class " + className + "::" + field.Name + ";");
                }
            }

            _sb.AppendLine();
            _sb.AppendLine("bool* _overriden;");

            _sb.AppendLine();
            _sb.AppendLine("gcroot<" + className + "^> _managed;");

            //_sb.AppendLine();
            //foreach (DefField field in _protectedFields)
            //{
            //    _sb.AppendLine(field.NativeTypeName + "& ref_" + field.Name + ";");
            //}

            _sb.AppendLine();
            _sb.AppendLine("virtual void _Init_CLRObject() override { *static_cast<CLRObject*>(this) = _managed; }");
        }

        protected override void AddConstructor(DefFunction f)
        {
            string className;
            if (_t.IsNested)
            {
                className = _t.ParentClass.FullCLRName + "::" + _t.Name;
            }
            else
            {
                className = _wrapper.ManagedNamespace + "::" + _t.Name;
            }
            _sb.AppendIndent(ProxyName + "( " + className + "^ managedObj");
            if (f != null)
            {
                foreach (DefParam param in f.Parameters)
                    _sb.Append(", " + param.NativeTypeName + " " + param.Name);
            }

            _sb.Append(" ) :\n");

            if (f != null)
            {
                _sb.AppendIndent("\t" + _t.FullNativeName + "(");
                for (int i = 0; i < f.Parameters.Count; i++)
                {
                    DefParam param = f.Parameters[i];
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

        protected override void AddOverridableFunction(DefFunction f)
        {
            _sb.AppendIndent("");
            if (f.IsVirtual)
                _sb.Append("virtual ");
            _sb.Append(f.NativeTypeName + " " + f.Name + "(");
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
            if (_t.IsInterface)
            {
                IndentStringBuilder tempsb = _sb;
                _sb = new IndentStringBuilder();
                base.Add();
                string fname = _t.FullCLRName.Replace(_t.CLRName, _t.Name);
                string res = _sb.ToString().Replace(_t.FullCLRName + "::", fname + "::");
                _sb = tempsb;
                _sb.AppendLine(res);
            }
            else
                base.Add();
        }

        protected override void AddPreDeclarations()
        {
            if (!_t.IsNested)
            {
                _wrapper.AddPreDeclaration("ref class " + _t.Name + ";");
                _wrapper.AddPragmaMakePublicForType(_t);

            }
        }

        protected override void AddDefinition()
        {
            if (_t.IsInterface)
            {
                //put _t.Name instead of _t.CLRName
                _sb.AppendIndent("");
                if (!_t.IsNested)
                    _sb.Append("public ");
                else
                    _sb.Append(GetProtectionString(_t.ProtectionType) + ": ");
                string baseclass = GetBaseAndInterfaces();
                if (baseclass != "")
                    _sb.AppendFormat("ref class {0}{1} : {2}\n", _t.Name, (IsAbstractClass) ? " abstract" : "", baseclass);
                else
                    _sb.AppendFormat("ref class {0}{1}\n", _t.Name, (IsAbstractClass) ? " abstract" : "");
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

        public IncOverridableClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
            _wrapper.PostClassProducers.Add(new IncNativeProxyClassProducer(_wrapper, _t, _sb));
        }

        private string _proxyName;
        protected virtual string ProxyName
        {
            get
            {
                if (_proxyName == null)
                    _proxyName = NativeProxyClassProducer.GetProxyName(_t);

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

        protected override string GetNativeInvokationTarget(DefFunction f)
        {
            return "static_cast<" + ProxyName + "*>(_native)->" + f.Class.Name + "::" + f.Name;
        }

        protected override string GetNativeInvokationTarget(DefField field)
        {
            return "static_cast<" + ProxyName + "*>(_native)->" + _t.FullNativeName + "::" + field.Name;
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
        protected DefClass[] _additionalInterfaces;

        public IncSubclassingClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb, DefClass[] additionalInterfaces)
            : base(wrapper, t, sb)
        {
            this._additionalInterfaces = additionalInterfaces;
        }

        protected override void Init()
        {
            _interfaces = new List<DefClass>();

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

        protected override bool DeclareAsOverride(DefFunction f)
        {
            if (f.ProtectionType == ProtectionType.Public)
                return true;
            else
                return false;
        }

        protected override void AddPreDeclarations()
        {
            if (!_t.IsNested)
                _wrapper.AddPragmaMakePublicForType(_t);
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
            return _t.FullCLRName;
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

            _sb.AppendLine();
            foreach (DefProperty prop in _overridableProperties)
            {
                if (!prop.Function.IsAbstract)
                {
                    AddProperty(prop);
                    _sb.AppendLine();
                }
            }

            foreach (DefFunction func in _overridableFunctions)
            {
                if (!func.IsProperty && func.ProtectionType == ProtectionType.Public
                    && !func.IsAbstract)
                {
                    AddMethod(func);
                    _sb.AppendLine();
                }
            }
        }

        protected override void AddProtectedDeclarations()
        {
            _sb.DecreaseIndent();
            _sb.AppendLine("protected public:");
            _sb.IncreaseIndent();

            foreach (DefFunction func in _overridableFunctions)
            {
                if (!func.IsProperty && func.ProtectionType == ProtectionType.Protected)
                {
                    AddMethod(func);
                    _sb.AppendLine();
                }
            }
        }
    }
}

