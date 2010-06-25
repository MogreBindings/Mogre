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
    class CppNativeProxyClassProducer : NativeProxyClassProducer
    {
        private static bool IsCachedFunction(DefFunction f)
        {
            return (f.PassedByType == PassedByType.Reference
                && (f.Type.IsValueType || f.Type.IsPureManagedClass));
        }

        public static void AddNativeProxyMethodBody(DefFunction f, string managedTarget, IndentStringBuilder sb)
        {
            string managedCall;
            string fullPostConv = null;

            if (f.IsGetProperty)
            {
                sb.AppendLine(f.CLRTypeName + " mp_return = " + managedTarget + "->" + f.CLRName + ";");
                managedCall = "mp_return";
            }
            else if (f.IsSetProperty)
            {
                DefParam param = f.Parameters[0];
                managedCall = managedTarget + "->" + f.CLRName + " = " + param.Type.GetNativeCallConversion(param.Name, param);
            }
            else
            {
                string pre, post, conv;

                foreach (DefParam param in f.Parameters)
                {
                    param.Type.GetNativeParamConversion(param, out pre, out conv, out post);
                    if (!String.IsNullOrEmpty(pre))
                        sb.AppendLine(pre);

                    if (!String.IsNullOrEmpty(post))
                        fullPostConv += post + "\n";
                }

                bool explicitCast = f.HasAttribute<ExplicitCastingForParamsAttribute>();

                if (!f.IsVoid)
                {
                    sb.AppendIndent(f.CLRTypeName + " mp_return = " + managedTarget + "->" + f.CLRName + "(");
                    for (int i = 0; i < f.Parameters.Count; i++)
                    {
                        DefParam param = f.Parameters[i];
                        param.Type.GetNativeParamConversion(param, out pre, out conv, out post);
                        sb.Append(" ");
                        if (explicitCast) sb.Append("(" + param.CLRTypeName + ")");
                        sb.Append(conv);
                        if (i < f.Parameters.Count - 1) sb.Append(",");
                    }
                    sb.Append(" );\n");
                    managedCall = "mp_return";

                    if (!String.IsNullOrEmpty(fullPostConv))
                        sb.AppendLine(fullPostConv);
                }
                else
                {
                    managedCall = managedTarget + "->" + f.CLRName + "(";
                    for (int i = 0; i < f.Parameters.Count; i++)
                    {
                        DefParam param = f.Parameters[i];
                        param.Type.GetNativeParamConversion(param, out pre, out conv, out post);
                        managedCall += " ";
                        if (explicitCast) managedCall += "(" + param.CLRTypeName + ")";
                        managedCall += conv;
                        if (i < f.Parameters.Count - 1) managedCall += ",";
                    }
                    managedCall += " )";
                }
            }

            if (!f.IsVoid)
            {
                if (f.Type is IDefString)
                {
                    sb.AppendLine("SET_NATIVE_STRING( Mogre::Implementation::cachedReturnString, " + managedCall + " )");
                    sb.AppendLine("return Mogre::Implementation::cachedReturnString;");
                }
                else
                {
                    string returnExpr;
                    string newname, expr, postcall;
                    DefParam param = new DefParam(f, managedCall);
                    expr = f.Type.GetPreCallParamConversion(param, out newname);
                    postcall = f.Type.GetPostCallParamConversionCleanup(param);
                    if (!String.IsNullOrEmpty(expr))
                    {
                        sb.AppendLine(expr);
                        if (String.IsNullOrEmpty(postcall))
                            returnExpr = newname;
                        else
                        {
                            throw new Exception("Unexpected");
                        }
                    }
                    else
                    {
                        returnExpr = newname;
                    }

                    if (IsCachedFunction(f))
                    {
                        sb.AppendLine("STATIC_ASSERT( sizeof(" + f.Type.FullNativeName + ") <= CACHED_RETURN_SIZE )");
                        sb.AppendLine("memcpy( Mogre::Implementation::cachedReturn, &" + returnExpr + ", sizeof(" + f.Type.FullNativeName + ") );");
                        sb.AppendLine("return *reinterpret_cast<" + f.Type.FullNativeName + "*>(Mogre::Implementation::cachedReturn);");
                    }
                    else
                    {
                        sb.AppendLine("return " + returnExpr + ";");
                    }
                }
            }
            else
            {
                sb.AppendLine(managedCall + ";");

                if (!String.IsNullOrEmpty(fullPostConv))
                    sb.AppendLine(fullPostConv);
            }
        }

        public CppNativeProxyClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }

        protected override void AddOverridableFunction(DefFunction f)
        {
            _wrapper.CppCheckTypeForDependancy(f.Type);
            foreach (DefParam param in f.Parameters)
                _wrapper.CppCheckTypeForDependancy(param.Type);

            _sb.AppendIndent("");
            _sb.Append(f.NativeTypeName + " " + ProxyName + "::" + f.Name + "(");
            AddNativeMethodParams(f);
            _sb.Append(" )");
            if (f.IsConstFunctionCall)
                _sb.Append(" const");
            _sb.Append("\n");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();

            if (!f.IsAbstract)
            {
                _sb.AppendLine("if (_overriden[ " + _methodIndices[f] + " ])");
                _sb.AppendLine("{");
                _sb.IncreaseIndent();
            }

            if (f.HasAttribute<CustomNativeProxyDeclarationAttribute>())
            {
                string txt = f.GetAttribute<CustomNativeProxyDeclarationAttribute>().DeclarationText;
                txt = ReplaceCustomVariables(txt, f).Replace("@MANAGED@", "_managed");
                _sb.AppendLine(txt);
            }
            else
            {
                AddNativeProxyMethodBody(f, "_managed", _sb);
            }

            if (!f.IsAbstract)
            {
                _sb.DecreaseIndent();
                _sb.AppendLine("}");
                _sb.AppendLine("else");
                _sb.AppendIndent("\t");
                if (!f.IsVoid) _sb.Append("return ");
                _sb.Append(f.Class.Name + "::" + f.Name + "(");
                for (int i = 0; i < f.Parameters.Count; i++)
                {
                    DefParam param = f.Parameters[i];
                    _sb.Append(" " + param.Name);
                    if (i < f.Parameters.Count - 1) _sb.Append(",");
                }
                _sb.Append(" );\n");
            }

            _sb.DecreaseIndent();
            _sb.AppendLine("}");
        }

        //protected override void AddProtectedFunction(DefFunction f)
        //{
        //    _sb.AppendIndent("");
        //    _sb.Append(f.NativeTypeName + " " + ProxyName + "::base_" + f.Name + "(");
        //    AddNativeMethodParams(f);
        //    _sb.Append(" )");
        //    if (f.IsConstFunctionCall)
        //        _sb.Append(" const");
        //    _sb.Append("\n");
        //    _sb.AppendLine("{");
        //    _sb.IncreaseIndent();

        //    _sb.AppendIndent("");
        //    if (!f.IsVoid)
        //        _sb.Append("return ");
        //    _sb.Append(_t.FullNativeName + "::" + f.Name + "(");

        //    for (int i = 0; i < f.Parameters.Count; i++)
        //    {
        //        DefParam param = f.Parameters[i];
        //        _sb.Append(" " + param.Name);
        //        if (i < f.Parameters.Count - 1) _sb.Append(",");
        //    }
        //    _sb.Append(" );\n");

        //    _sb.DecreaseIndent();
        //    _sb.AppendLine("}");
        //}
    }

    class CppOverridableClassProducer : CppNonOverridableClassProducer
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
                fname = GetClassName().Replace(_t.CLRName, _t.Name);
                res = res.Replace(GetClassName() + "::", fname + "::");

                _sb = tempsb;
                _sb.AppendLine(res);
            }
            else
                base.Add();
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

        public CppOverridableClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
            _wrapper.PostClassProducers.Add(new CppNativeProxyClassProducer(_wrapper, _t, _sb));
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
        //    if (f.ProtectionType == ProtectionType.Public)
        //        return "static_cast<" + ProxyName + "*>(_native)->" + _t.FullNativeName + "::" + f.Name;
        //    else
        //        return "static_cast<" + ProxyName + "*>(_native)->" + "base_" + f.Name;
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

        protected override void AddDefaultImplementationClass()
        {
        }

        protected override void AddPublicConstructor(DefFunction f)
        {
            _sb.AppendIndent(GetClassName() + "::" + _t.Name);
            if (f == null)
                _sb.Append("()");
            else
                AddMethodParameters(f);
            _sb.Append(" : " + GetBaseClassName() + "( (CLRObject*)0 )");
            _sb.Append("\n");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();

            _sb.AppendLine("_createdByCLR = true;");
            _sb.AppendLine("Type^ thisType = this->GetType();");

            if (!IsAbstractClass && !_t.IsInterface)
                _sb.AppendLine("_isOverriden = (thisType != " + _t.CLRName + "::typeid);");
            else
                _sb.AppendLine("_isOverriden = true;  //it's abstract or interface so it must be overriden");

            int count = 0;
            string preCall = null, postCall = null;

            if (f != null)
            {
                count = f.Parameters.Count;
                preCall = GetMethodPreNativeCall(f, count);
                postCall = GetMethodPostNativeCall(f, count);

                if (!String.IsNullOrEmpty(preCall))
                    _sb.AppendLine(preCall);
            }

            if (!IsAbstractClass && !_t.IsInterface)
            {
                _sb.AppendLine("if (_isOverriden)");
                _sb.AppendLine("{");
                _sb.IncreaseIndent();
            }

            string proxyName = NativeProxyClassProducer.GetProxyName(_t);
            _sb.AppendIndent(proxyName + "* proxy = new " + proxyName + "(this");

            if (count > 0)
            {
                _sb.Append(",");
                for (int i = 0; i < count; i++)
                {
                    DefParam p = f.Parameters[i];
                    string newname;
                    p.Type.GetPreCallParamConversion(p, out newname);
                    _sb.Append(" " + newname);
                    if (i < count - 1) _sb.Append(",");
                }
            }

            _sb.Append(");\n");

            _sb.AppendLine("proxy->_overriden = Implementation::SubclassingManager::Instance->GetOverridenMethodsArrayPointer(thisType, " + _t.Name + "::typeid, " + _methodIndicesCount + ");");
            _sb.AppendLine("_native = proxy;");

            if (!IsAbstractClass && !_t.IsInterface)
            {
                _sb.DecreaseIndent();
                _sb.AppendLine("}");
                _sb.AppendLine("else");
                _sb.AppendIndent("\t_native = new " + _t.FullNativeName + "(");

                if (count > 0)
                {
                    _sb.Append(",");
                    for (int i = 0; i < count; i++)
                    {
                        DefParam p = f.Parameters[i];
                        string newname;
                        p.Type.GetPreCallParamConversion(p, out newname);
                        _sb.Append(" " + newname);
                        if (i < count - 1) _sb.Append(",");
                    }
                }

                _sb.Append(");\n");
            }

            if (!String.IsNullOrEmpty(postCall))
            {
                _sb.AppendLine();
                _sb.AppendLine(postCall);
                _sb.AppendLine();
            }

            _sb.AppendLine();
            AddConstructorBody();

            _sb.DecreaseIndent();
            _sb.AppendLine("}");
        }
    }

    class CppSubclassingClassProducer : CppOverridableClassProducer
    {
        protected DefClass[] _additionalInterfaces;

        public CppSubclassingClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb, DefClass[] additionalInterfaces)
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

        protected override bool AllowMethodOverloads
        {
            get
            {
                return false;
            }
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

        protected override void AddPublicDeclarations()
        {
            if (_t.Constructors.Length > 0)
            {
                foreach (DefFunction func in _t.Constructors)
                    AddPublicConstructor(func);
            }
            else
                AddPublicConstructor(null);

            _sb.AppendLine();
            foreach (DefProperty prop in _overridableProperties)
            {
                AddProperty(prop);
                _sb.AppendLine();
            }

            foreach (DefFunction func in _overridableFunctions)
            {
                if (!func.IsProperty && func.ProtectionType == ProtectionType.Public)
                {
                    AddMethod(func);
                    _sb.AppendLine();
                }
            }
        }

        protected override void AddProtectedDeclarations()
        {
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
