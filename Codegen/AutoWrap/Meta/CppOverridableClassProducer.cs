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
        private static bool IsCachedFunction(MemberMethodDefinition f)
        {
            return (f.PassedByType == PassedByType.Reference
                && (f.MemberType.IsValueType || f.MemberType.IsPureManagedClass));
        }

        public static void AddNativeProxyMethodBody(MemberMethodDefinition f, string managedTarget, SourceCodeStringBuilder sb)
        {
            string managedCall;
            string fullPostConv = null;

            if (f.IsGetProperty)
            {
                sb.AppendLine(f.MemberTypeCLRName + " mp_return = " + managedTarget + "->" + f.CLRName + ";");
                managedCall = "mp_return";
            }
            else if (f.IsSetProperty)
            {
                ParamDefinition param = f.Parameters[0];
                managedCall = managedTarget + "->" + f.CLRName + " = " + param.Type.ProduceNativeCallConversionCode(param.Name, param);
            }
            else
            {
                string pre, post, conv;

                foreach (ParamDefinition param in f.Parameters)
                {
                    param.Type.ProduceNativeParamConversionCode(param, out pre, out conv, out post);
                    if (!String.IsNullOrEmpty(pre))
                        sb.AppendLine(pre);

                    if (!String.IsNullOrEmpty(post))
                        fullPostConv += post + "\n";
                }

                bool explicitCast = f.HasAttribute<ExplicitCastingForParamsAttribute>();

                if (!f.HasReturnValue)
                {
                    sb.AppendIndent(f.MemberTypeCLRName + " mp_return = " + managedTarget + "->" + f.CLRName + "(");
                    for (int i = 0; i < f.Parameters.Count; i++)
                    {
                        ParamDefinition param = f.Parameters[i];
                        param.Type.ProduceNativeParamConversionCode(param, out pre, out conv, out post);
                        sb.Append(" ");
                        if (explicitCast) sb.Append("(" + param.MemberTypeCLRName + ")");
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
                        ParamDefinition param = f.Parameters[i];
                        param.Type.ProduceNativeParamConversionCode(param, out pre, out conv, out post);
                        managedCall += " ";
                        if (explicitCast) managedCall += "(" + param.MemberTypeCLRName + ")";
                        managedCall += conv;
                        if (i < f.Parameters.Count - 1) managedCall += ",";
                    }
                    managedCall += " )";
                }
            }

            if (!f.HasReturnValue)
            {
                if (f.MemberType is IDefString)
                {
                    sb.AppendLine("SET_NATIVE_STRING( Mogre::Implementation::cachedReturnString, " + managedCall + " )");
                    sb.AppendLine("return Mogre::Implementation::cachedReturnString;");
                }
                else
                {
                    string returnExpr;
                    string newname, expr, postcall;
                    ParamDefinition param = new ParamDefinition(f.MetaDef, f, managedCall);
                    expr = f.MemberType.ProducePreCallParamConversionCode(param, out newname);
                    postcall = f.MemberType.ProducePostCallParamConversionCleanupCode(param);
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
                        sb.AppendLine("STATIC_ASSERT( sizeof(" + f.MemberType.FullNativeName + ") <= CACHED_RETURN_SIZE )");
                        sb.AppendLine("memcpy( Mogre::Implementation::cachedReturn, &" + returnExpr + ", sizeof(" + f.MemberType.FullNativeName + ") );");
                        sb.AppendLine("return *reinterpret_cast<" + f.MemberType.FullNativeName + "*>(Mogre::Implementation::cachedReturn);");
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

        public CppNativeProxyClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
        }

        protected override void AddOverridableFunction(MemberMethodDefinition f)
        {
            _wrapper.CppCheckTypeForDependancy(f.MemberType);
            foreach (ParamDefinition param in f.Parameters)
                _wrapper.CppCheckTypeForDependancy(param.Type);

            _code.AppendIndent("");
            _code.Append(f.MemberTypeNativeName + " " + ProxyName + "::" + f.Name + "(");
            AddNativeMethodParams(f);
            _code.Append(" )");
            if (f.IsConstFunctionCall)
                _code.Append(" const");
            _code.Append("\n");
            _code.AppendLine("{");
            _code.IncreaseIndent();

            if (!f.IsAbstract)
            {
                _code.AppendLine("if (_overriden[ " + _methodIndices[f] + " ])");
                _code.AppendLine("{");
                _code.IncreaseIndent();
            }

            if (f.HasAttribute<CustomNativeProxyDeclarationAttribute>())
            {
                string txt = f.GetAttribute<CustomNativeProxyDeclarationAttribute>().DeclarationText;
                txt = ReplaceCustomVariables(txt, f).Replace("@MANAGED@", "_managed");
                _code.AppendLine(txt);
            }
            else
            {
                AddNativeProxyMethodBody(f, "_managed", _code);
            }

            if (!f.IsAbstract)
            {
                _code.DecreaseIndent();
                _code.AppendLine("}");
                _code.AppendLine("else");
                _code.AppendIndent("\t");
                if (!f.HasReturnValue) _code.Append("return ");
                _code.Append(f.ContainingClass.Name + "::" + f.Name + "(");
                for (int i = 0; i < f.Parameters.Count; i++)
                {
                    ParamDefinition param = f.Parameters[i];
                    _code.Append(" " + param.Name);
                    if (i < f.Parameters.Count - 1) _code.Append(",");
                }
                _code.Append(" );\n");
            }

            _code.DecreaseIndent();
            _code.AppendLine("}");
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
            if (_definition.IsInterface)
            {
                SourceCodeStringBuilder tempsb = _code;
                _code = new SourceCodeStringBuilder();
                base.Add();
                string fname = _definition.FullCLRName.Replace(_definition.CLRName, _definition.Name);
                string res = _code.ToString().Replace(_definition.FullCLRName + "::", fname + "::");
                fname = GetClassName().Replace(_definition.CLRName, _definition.Name);
                res = res.Replace(GetClassName() + "::", fname + "::");

                _code = tempsb;
                _code.AppendLine(res);
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

        public CppOverridableClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
            _wrapper.PostClassProducers.Add(new CppNativeProxyClassProducer(metaDef, _wrapper, _definition, _code));
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
            return "static_cast<" + ProxyName + "*>(_native)->" + f.ContainingClass.Name + "::" + f.Name;
        }

        protected override string GetNativeInvokationTarget(MemberFieldDefinition field)
        {
            return "static_cast<" + ProxyName + "*>(_native)->" + _definition.FullNativeName + "::" + field.Name;
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

        protected override void AddPublicConstructor(MemberMethodDefinition f)
        {
            _code.AppendIndent(GetClassName() + "::" + _definition.Name);
            if (f == null)
                _code.Append("()");
            else
                AddMethodParameters(f);
            _code.Append(" : " + GetBaseClassName() + "( (CLRObject*)0 )");
            _code.Append("\n");
            _code.AppendLine("{");
            _code.IncreaseIndent();

            _code.AppendLine("_createdByCLR = true;");
            _code.AppendLine("Type^ thisType = this->GetType();");

            if (!IsAbstractClass && !_definition.IsInterface)
                _code.AppendLine("_isOverriden = (thisType != " + _definition.CLRName + "::typeid);");
            else
                _code.AppendLine("_isOverriden = true;  //it's abstract or interface so it must be overriden");

            int count = 0;
            string preCall = null, postCall = null;

            if (f != null)
            {
                count = f.Parameters.Count;
                preCall = GetMethodPreNativeCall(f, count);
                postCall = GetMethodPostNativeCall(f, count);

                if (!String.IsNullOrEmpty(preCall))
                    _code.AppendLine(preCall);
            }

            if (!IsAbstractClass && !_definition.IsInterface)
            {
                _code.AppendLine("if (_isOverriden)");
                _code.AppendLine("{");
                _code.IncreaseIndent();
            }

            string proxyName = NativeProxyClassProducer.GetProxyName(_definition);
            _code.AppendIndent(proxyName + "* proxy = new " + proxyName + "(this");

            if (count > 0)
            {
                _code.Append(",");
                for (int i = 0; i < count; i++)
                {
                    ParamDefinition p = f.Parameters[i];
                    string newname;
                    p.Type.ProducePreCallParamConversionCode(p, out newname);
                    _code.Append(" " + newname);
                    if (i < count - 1) _code.Append(",");
                }
            }

            _code.Append(");\n");

            _code.AppendLine("proxy->_overriden = Implementation::SubclassingManager::Instance->GetOverridenMethodsArrayPointer(thisType, " + _definition.Name + "::typeid, " + _methodIndicesCount + ");");
            _code.AppendLine("_native = proxy;");

            if (!IsAbstractClass && !_definition.IsInterface)
            {
                _code.DecreaseIndent();
                _code.AppendLine("}");
                _code.AppendLine("else");
                _code.AppendIndent("\t_native = new " + _definition.FullNativeName + "(");

                if (count > 0)
                {
                    _code.Append(",");
                    for (int i = 0; i < count; i++)
                    {
                        ParamDefinition p = f.Parameters[i];
                        string newname;
                        p.Type.ProducePreCallParamConversionCode(p, out newname);
                        _code.Append(" " + newname);
                        if (i < count - 1) _code.Append(",");
                    }
                }

                _code.Append(");\n");
            }

            if (!String.IsNullOrEmpty(postCall))
            {
                _code.AppendEmptyLine();
                _code.AppendLine(postCall);
                _code.AppendEmptyLine();
            }

            _code.AppendEmptyLine();
            AddConstructorBody();

            _code.DecreaseIndent();
            _code.AppendLine("}");
        }
    }

    class CppSubclassingClassProducer : CppOverridableClassProducer
    {
        protected ClassDefinition[] _additionalInterfaces;

        public CppSubclassingClassProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb, ClassDefinition[] additionalInterfaces)
            : base(metaDef, wrapper, t, sb)
        {
            this._additionalInterfaces = additionalInterfaces;
        }

        protected override void Init()
        {
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
            if (_definition.Constructors.Length > 0)
            {
                foreach (MemberMethodDefinition func in _definition.Constructors)
                    AddPublicConstructor(func);
            }
            else
                AddPublicConstructor(null);

            _code.AppendEmptyLine();
            foreach (PropertyDefinition prop in _overridableProperties)
            {
                AddProperty(prop);
                _code.AppendEmptyLine();
            }

            foreach (MemberMethodDefinition func in _overridableFunctions)
            {
                if (!func.IsProperty && func.ProtectionLevel == ProtectionLevel.Public)
                {
                    AddMethod(func);
                    _code.AppendEmptyLine();
                }
            }
        }

        protected override void AddProtectedDeclarations()
        {
            foreach (MemberMethodDefinition func in _overridableFunctions)
            {
                if (!func.IsProperty && func.ProtectionLevel == ProtectionLevel.Protected)
                {
                    AddMethod(func);
                    _code.AppendEmptyLine();
                }
            }
        }
    }
}
