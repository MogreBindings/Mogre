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
    abstract class CppClassProducer : ClassProducer
    {
        public CppClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
            //if (AllowSubclassing)
            //{
            //    _wrapper.PreClassProducers.Add(new CppNativeProtectedTypesProxy(_wrapper, _t, _sb));
            //}
        }

        protected override void AddTypeDependancy(DefType type)
        {
            _wrapper.CppCheckTypeForDependancy(type);
        }

        protected virtual string GetCLRTypeName(ITypeMember m)
        {
            AddTypeDependancy(m.Type);
            if (m.Type.IsUnnamedSTLContainer)
                return GetClassName() + "::" + m.CLRTypeName;
            else
                return m.CLRTypeName;
        }

        protected virtual string GetCLRParamTypeName(DefParam param)
        {
            AddTypeDependancy(param.Type);
            return param.Type.GetCLRParamTypeName(param);
        }

        protected override void AddPostBody()
        {
            base.AddPostBody();
            _sb.AppendLine();

            if (_t.HasAttribute<CLRObjectAttribute>(true)) {
                _sb.AppendLine("__declspec(dllexport) " + _wrapper.GetInitCLRObjectFuncSignature(_t));
                _sb.AppendLine("{");
                _sb.AppendLine("\t*pClrObj = gcnew " + _t.FullCLRName + "(pClrObj);");
                _sb.AppendLine("}");
            }

            _sb.AppendLine();
        }

        protected override void AddInternalDeclarations()
        {
            base.AddInternalDeclarations();

            foreach (DefClass cls in _interfaces)
            {
                _sb.AppendLine(cls.FullNativeName + "* " + GetClassName() + "::_" + cls.CLRName + "_GetNativePtr()");
                _sb.AppendLine("{");
                _sb.AppendLine("\treturn static_cast<" + cls.FullNativeName + "*>( " + GetNativeInvokationTarget() + " );");
                _sb.AppendLine("}");
                _sb.AppendLine();
            }
        }

		protected override void AddPublicDeclarations()
		{
			if ((!_t.IsNativeAbstractClass || _t.IsInterface)
					&& IsConstructable)
			{
				if (_t.Constructors.Length > 0)
				{
					foreach (DefFunction function in _t.Constructors)
					{
						if (function.ProtectionType == ProtectionType.Public &&
							!function.HasAttribute<IgnoreAttribute>())
						{
							AddPublicConstructor(function);
						}
					}
				}
				else
				{
					AddPublicConstructor(null);
				}

				_sb.AppendLine();
			}

			base.AddPublicDeclarations();
		}

        protected virtual void AddPublicConstructor(DefFunction function)
		{
			if (function == null)
			{
                AddPublicConstructorOverload(function, 0);
			}
            else
            {
                int defcount = 0;

                if (!function.HasAttribute<NoDefaultParamOverloadsAttribute>())
                {
                    foreach (DefParam param in function.Parameters)
                        if (param.DefaultValue != null)
                            defcount++;
                }
				
				bool hideParams = function.HasAttribute<HideParamsWithDefaultValuesAttribute>();

                // The overloads (because of default values)
                for (int dc = 0; dc <= defcount; dc++)
                {
                    if (dc < defcount && function.HasAttribute<HideParamsWithDefaultValuesAttribute>())
                        continue;
                    AddPublicConstructorOverload(function, function.Parameters.Count - dc);
                }

            }
        }

        protected virtual void AddPublicConstructorOverload(DefFunction f, int count)
        {
            _sb.AppendIndent(GetClassName() + "::" + _t.CLRName);
            if (f == null)
                _sb.Append("()");
            else
                AddMethodParameters(f, count);

            string nativeType = GetTopClass(_t).FullNativeName;
            if (GetTopBaseClassName() == "Wrapper")
                nativeType = "CLRObject";

            if (GetBaseClassName() != null)
                _sb.Append(" : " + GetBaseClassName() + "((" + nativeType + "*) 0)");

            _sb.Append("\n");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();

            if (!_t.IsInterface)
                _sb.AppendLine("_createdByCLR = true;");

            string preCall = null, postCall = null;

            if (f != null)
            {
                preCall = GetMethodPreNativeCall(f, count);
                postCall = GetMethodPostNativeCall(f, count);

                if (!String.IsNullOrEmpty(preCall))
                    _sb.AppendLine(preCall);
            }

            _sb.AppendIndent("_native = new " + _t.FullNativeName + "(");

            if (count > 0)
            {
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

            if (!String.IsNullOrEmpty(postCall))
            {
                _sb.AppendLine();
                _sb.AppendLine(postCall);
                _sb.AppendLine();
            }

            AddConstructorBody();

            _sb.DecreaseIndent();
            _sb.AppendLine("}");
        }

        protected override void AddStaticConstructor()
        {
            if (_t.IsInterface)
                _sb.AppendLine("static " + _t.Name + "::" + _t.Name + "()");
            else
                _sb.AppendLine("static " + _t.CLRName + "::" + _t.CLRName + "()");

            _sb.AppendLine("{");
            _sb.IncreaseIndent();
            foreach (DefMember m in _cachedMembers)
            {
                if (m.IsStatic)
                {
                    _sb.AppendIndent(NameToPrivate(m) + " = ");
                    if (m.ProtectionType == ProtectionType.Protected)
                    {
                        _sb.Append(NativeProtectedTypesProxy.GetProtectedTypesProxyName(m.Class));
                        _sb.Append("::" + m.Name + ";\n");
                    }
                    else
                    {
                        _sb.Append(m.Class.FullNativeName);
                        _sb.Append("::" + m.Name + ";\n");
                    }
                }
            }
            _sb.DecreaseIndent();
            _sb.AppendLine("}");
        }

        protected override void AddPostNestedTypes()
        {
            base.AddPostNestedTypes();

            if (_t.HasAttribute<CustomCppDeclarationAttribute>())
            {
                string txt = _t.GetAttribute<CustomCppDeclarationAttribute>().DeclarationText;
                txt = ReplaceCustomVariables(txt);
                _sb.AppendLine(txt);
                _sb.AppendLine();
            }
        }

        protected override void AddNestedTypeBeforeMainType(DefType nested)
        {
            base.AddNestedType(nested);
            _wrapper.CppAddType(nested, _sb);
        }

        protected override void AddNestedType(DefType nested)
        {
            if (nested.HasWrapType(WrapTypes.NativeDirector))
            {
                //Interface and native director are already declared before the declaration of this class.
                return;
            }

            base.AddNestedType(nested);
            _wrapper.CppAddType(nested, _sb);
        }

        protected override void AddMethod(DefFunction f)
        {
            if (f.HasAttribute<CustomCppDeclarationAttribute>())
            {
                if (f.IsAbstract && AllowSubclassing)
                {
                    return;
                }
                else
                {
                    string txt = f.GetAttribute<CustomCppDeclarationAttribute>().DeclarationText;
                    txt = ReplaceCustomVariables(txt, f);
                    _sb.AppendLine(txt);
                    _sb.AppendLine();
                    return;
                }
            }

            int defcount = 0;

            if (!f.HasAttribute<NoDefaultParamOverloadsAttribute>())
            {
                foreach (DefParam param in f.Parameters)
                    if (param.DefaultValue != null)
                        defcount++;
            }

            bool methodIsVirtual = DeclareAsVirtual(f);

            for (int dc = 0; dc <= defcount; dc++)
            {
                if (dc == 0 && f.IsAbstract && AllowSubclassing)
                {
                    //It's abstract, no body definition
                    continue;
                }

                if (!AllowMethodOverloads && dc > 0)
                    continue;

                if (dc < defcount && f.HasAttribute<HideParamsWithDefaultValuesAttribute>())
                    continue;

                _sb.AppendIndent(GetCLRTypeName(f) + " " + GetClassName() + "::" + f.CLRName);
                AddMethodParameters(f, f.Parameters.Count - dc);
                _sb.Append("\n");
                _sb.AppendLine("{");
                _sb.IncreaseIndent();

                bool isVirtualOverload = dc > 0 && methodIsVirtual && AllowVirtualMethods;

                if (isVirtualOverload)
                {
                    // Overloads (because of default values)
                    // main method is virtual, call it with CLR default values if _isOverriden=true,
                    // else do a normal native call

                    _sb.AppendLine("if (_isOverriden)");
                    _sb.AppendLine("{");
                    _sb.IncreaseIndent();

                    bool hasPostConversions = false;
                    for (int i = f.Parameters.Count - dc; i < f.Parameters.Count; i++)
                    {
                        DefParam p = f.Parameters[i];
                        if (!String.IsNullOrEmpty(p.CLRDefaultValuePreConversion))
                            _sb.AppendLine(p.CLRDefaultValuePreConversion);
                        if (!String.IsNullOrEmpty(p.CLRDefaultValuePostConversion))
                            hasPostConversions = true;

                        string n1, n2, n3;
                        DefType dependancy;
                        p.Type.GetDefaultParamValueConversion(p, out n1, out n2, out n3, out dependancy);
                        if (dependancy != null)
                            AddTypeDependancy(dependancy);
                    }

                    _sb.AppendIndent("");
                    if (!f.IsVoid)
                    {
                        if (hasPostConversions)
                        {
                            _sb.Append(GetCLRTypeName(f) + " mp_return = ");
                        }
                        else
                        {
                            _sb.Append("return ");
                        }
                    }

                    _sb.Append(f.CLRName + "(");
                    for (int i = 0; i < f.Parameters.Count; i++)
                    {
                        DefParam p = f.Parameters[i];
                        _sb.Append(" ");
                        if (i < f.Parameters.Count - dc)
                            _sb.Append(p.Name);
                        else
                        {
                            _sb.Append(p.CLRDefaultValue);
                        }
                        if (i < f.Parameters.Count - 1) _sb.Append(",");
                    }
                    _sb.Append(" );\n");

                    for (int i = f.Parameters.Count - dc; i < f.Parameters.Count; i++)
                    {
                        DefParam p = f.Parameters[i];
                        if (!String.IsNullOrEmpty(p.CLRDefaultValuePostConversion))
                            _sb.AppendLine(p.CLRDefaultValuePostConversion);
                    }

                    if (!f.IsVoid && hasPostConversions)
                    {
                        _sb.AppendLine("return mp_return;");
                    }

                    _sb.DecreaseIndent();
                    _sb.AppendLine("}");
                    _sb.AppendLine("else");
                    _sb.AppendLine("{");
                    _sb.IncreaseIndent();
                }

                AddMethodBody(f, f.Parameters.Count - dc);

                if (isVirtualOverload)
                {
                    _sb.DecreaseIndent();
                    _sb.AppendLine("}");
                }

                _sb.DecreaseIndent();
                _sb.AppendLine("}");
            }
        }

        protected virtual void AddMethodParameters(DefFunction f, int count)
        {
            _sb.Append("(");
            for (int i = 0; i < count; i++)
            {
                DefParam p = f.Parameters[i];
                _sb.Append(" " + GetCLRParamTypeName(p) + " " + p.Name);
                if (i < count - 1) _sb.Append(",");
            }
            _sb.Append(" )");
        }
        protected void AddMethodParameters(DefFunction f)
        {
            AddMethodParameters(f, f.Parameters.Count);
        }

        protected virtual void AddMethodBody(DefFunction f, int count)
        {
            string preCall = GetMethodPreNativeCall(f, count);
            string nativeCall = GetMethodNativeCall(f, count);
            string postCall = GetMethodPostNativeCall(f, count);

            if (!String.IsNullOrEmpty(preCall))
                _sb.AppendLine(preCall);

            if (f.IsVoid)
            {
                _sb.AppendLine(nativeCall + ";");
                if (!String.IsNullOrEmpty(postCall))
                    _sb.AppendLine(postCall);
            }
            else
            {
                if (String.IsNullOrEmpty(postCall))
                {
                    _sb.AppendLine("return " + nativeCall + ";");
                }
                else
                {
                    _sb.AppendLine(GetCLRTypeName(f) + " retres = " + nativeCall + ";");
                    _sb.AppendLine(postCall);
                    _sb.AppendLine("return retres;");
                }
            }
        }

        protected virtual string GetMethodPreNativeCall(DefFunction f, int paramCount)
        {
            string res = String.Empty;

            for (int i = 0; i < paramCount; i++)
            {
                DefParam p = f.Parameters[i];
                string newname;
                res += p.Type.GetPreCallParamConversion(p, out newname);
            }

            return res;
        }

        protected virtual string GetMethodNativeCall(DefFunction f, int paramCount)
        {
            string invoke;
            if (f.IsStatic)
            {
                if (f.ProtectionType == ProtectionType.Protected)
                {
                    string classname = NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(_t);
                    invoke = classname + "::" + f.Name + "(";
                }
                else
                {
                    invoke = _t.FullNativeName + "::" + f.Name + "(";
                }
            }
            else
            {
                invoke = GetNativeInvokationTarget(f) + "(";
            }

            for (int i = 0; i < paramCount; i++)
            {
                DefParam p = f.Parameters[i];
                string newname;
                p.Type.GetPreCallParamConversion(p, out newname);
                invoke += " " + newname;
                if (i < paramCount - 1) invoke += ",";
            }

            invoke += " )";

            if (f.IsVoid)
                return invoke;
            else
                return f.Type.GetNativeCallConversion(invoke, f);
        }

        protected virtual string GetMethodPostNativeCall(DefFunction f, int paramCount)
        {
            string res = String.Empty;

            for (int i = 0; i < paramCount; i++)
            {
                DefParam p = f.Parameters[i];
                res += p.Type.GetPostCallParamConversionCleanup(p);
            }

            return res;
        }

        protected string AddParameterConversion(DefParam param)
        {
            string newname, expr, postcall;
            expr = param.Type.GetPreCallParamConversion(param, out newname);
            postcall = param.Type.GetPostCallParamConversionCleanup(param);
            if (!String.IsNullOrEmpty(postcall))
                throw new Exception("Unexpected");

            if (!String.IsNullOrEmpty(expr))
                _sb.AppendLine(expr);

            return newname;
        }

        protected override void AddProperty(DefProperty p)
        {
            string ptype = GetCLRTypeName(p);
            string pname =  GetClassName() + "::" + p.Name;
            if (p.CanRead)
            {
                if (!(p.GetterFunction.IsAbstract && AllowSubclassing))
                {
                    if (AllowProtectedMembers || p.GetterFunction.ProtectionType != ProtectionType.Protected)
                    {
                        string managedType = GetMethodNativeCall(p.GetterFunction, 0);

                        _sb.AppendLine(ptype + " " + pname + "::get()");
                        _sb.AppendLine("{");
                        if (_cachedMembers.Contains(p.GetterFunction))
                        {
                            string priv = NameToPrivate(p.Name);
                            _sb.AppendLine("\treturn ( CLR_NULL == " + priv + " ) ? (" + priv + " = " + managedType + ") : " + priv + ";");
                        }
                        else
                        {
                            _sb.AppendLine("\treturn " + managedType + ";");
                        }
                        _sb.AppendLine("}");
                    }
                }
            }

            if (p.CanWrite)
            {
                if (!(p.SetterFunction.IsAbstract && AllowSubclassing))
                {
                    if (AllowProtectedMembers || p.SetterFunction.ProtectionType != ProtectionType.Protected)
                    {
                        _sb.AppendLine("void " + pname + "::set( " + ptype + " " + p.SetterFunction.Parameters[0].Name + " )");
                        _sb.AppendLine("{");
                        _sb.IncreaseIndent();

                        string preCall = GetMethodPreNativeCall(p.SetterFunction, 1);
                        string nativeCall = GetMethodNativeCall(p.SetterFunction, 1);
                        string postCall = GetMethodPostNativeCall(p.SetterFunction, 1);

                        if (!String.IsNullOrEmpty(preCall))
                            _sb.AppendLine(preCall);

                        _sb.AppendLine(nativeCall + ";");

                        if (!String.IsNullOrEmpty(postCall))
                            _sb.AppendLine(postCall);

                        _sb.DecreaseIndent();
                        _sb.AppendLine("}");
                    }
                }
            }
        }

        protected override void AddPropertyField(DefField field)
        {
            string ptype = GetCLRTypeName(field);
            string pname = GetClassName () + "::" + (field.HasAttribute<RenameAttribute>() ? field.GetAttribute<RenameAttribute> ().Name : field.Name);

            if (field.IsNativeArray)
            {
                if (field.Type.HasAttribute<NativeValueContainerAttribute>()
                    || (field.Type.IsValueType && !field.Type.HasWrapType(WrapTypes.NativePtrValueType)))
                {
                    DefParam tmpParam = new DefParam(field, field.Name + "_array");
                    switch (field.PassedByType)
                    {
                        case PassedByType.Value:
                            tmpParam.PassedByType = PassedByType.Pointer;
                            break;
                        case PassedByType.Pointer:
                            tmpParam.PassedByType = PassedByType.PointerPointer;
                            break;
                        default:
                            throw new Exception("Unexpected");
                    }

                    ptype = GetCLRTypeName(tmpParam);
                    string managedType = field.Type.GetNativeCallConversion(GetNativeInvokationTarget(field), tmpParam);

                    _sb.AppendLine(ptype + " " + pname + "::get()");
                    _sb.AppendLine("{");
                    _sb.AppendLine("\treturn " + managedType + ";");
                    _sb.AppendLine("}");
                }
                else
                {
                    string managedType = field.Type.GetNativeCallConversion(GetNativeInvokationTarget(field) + "[index]", field);

                    _sb.AppendLine(ptype + " " + pname + "::get(int index)");
                    _sb.AppendLine("{");
                    _sb.AppendLine("\tif (index < 0 || index >= " + field.ArraySize + ") throw gcnew IndexOutOfRangeException();");
                    _sb.AppendLine("\treturn " + managedType + ";");
                    _sb.AppendLine("}");
                    _sb.AppendLine("void " + pname + "::set(int index, " + ptype + " value )");
                    _sb.AppendLine("{");
                    _sb.IncreaseIndent();
                    _sb.AppendLine("if (index < 0 || index >= " + field.ArraySize + ") throw gcnew IndexOutOfRangeException();");
                    string param = AddParameterConversion(new DefParam(field, "value"));
                    _sb.AppendLine(GetNativeInvokationTarget(field) + "[index] = " + param + ";");
                    _sb.DecreaseIndent();
                    _sb.AppendLine("}");
                }
            }
            else if (_cachedMembers.Contains(field))
            {
                string managedType;
                if (field.Type.IsSTLContainer)
                {
                    managedType = GetNativeInvokationTarget(field);
                }
                else
                {
                    managedType = field.Type.GetNativeCallConversion(GetNativeInvokationTarget(field), field);
                }
                string priv = NameToPrivate(field);

                _sb.AppendLine(ptype + " " + pname + "::get()");
                _sb.AppendLine("{");
                if (!field.IsStatic)
                    _sb.AppendLine("\treturn ( CLR_NULL == " + priv + " ) ? (" + priv + " = " + managedType + ") : " + priv + ";");
                else
                    _sb.AppendLine("\treturn " + priv + ";");
                _sb.AppendLine("}");
            }
            else
            {
                string managedType = field.Type.GetNativeCallConversion(GetNativeInvokationTarget(field), field);

                _sb.AppendLine(ptype + " " + pname + "::get()");
                _sb.AppendLine("{");
                _sb.AppendLine("\treturn " + managedType + ";");
                _sb.AppendLine("}");

                if ( // SharedPtrs can be copied by value. Let all be copied by value just to be sure (field.PassedByType == PassedByType.Pointer || field.Type.IsValueType)
                    !IsReadOnly && !field.Type.HasAttribute<ReadOnlyForFieldsAttribute>()
                    && !field.IsConst)
                {
                    _sb.AppendLine("void " + pname + "::set( " + ptype + " value )");
                    _sb.AppendLine("{");
                    _sb.IncreaseIndent();
                    string param = AddParameterConversion(new DefParam(field, "value"));
                    _sb.AppendLine(GetNativeInvokationTarget(field) + " = " + param + ";");
                    _sb.DecreaseIndent();
                    _sb.AppendLine("}");
                }
            }
        }

        protected override void AddMethodsForField(DefField field)
        {
            string managedType = field.Type.GetNativeCallConversion(GetNativeInvokationTarget(field), field);

            _sb.AppendLine(GetCLRTypeName(field) + " " + GetClassName() + "::get_" + field.Name + "()");
            _sb.AppendLine("{");
            _sb.AppendLine("\treturn " + managedType + ";");
            _sb.AppendLine("}");

            DefParam param = new DefParam(field, "value");
            _sb.AppendLine("void " + GetClassName() + "::set_" + field.Name + "(" + param.Type.GetCLRParamTypeName(param) + " value)");
            _sb.AppendLine("{");
            _sb.IncreaseIndent();
            _sb.AppendLine(GetNativeInvokationTarget(field) + " = " + AddParameterConversion(param) + ";");
            _sb.DecreaseIndent();
            _sb.AppendLine("}");
        }

        protected override void AddPredefinedMethods(PredefinedMethods pm)
        {
            string cls = GetClassName();
            switch (pm)
            {
                case PredefinedMethods.Equals:
                    _sb.AppendLine("bool " + cls + "::Equals(Object^ obj)");
                    _sb.AppendLine("{");
                    _sb.AppendLine("    " + cls + "^ clr = dynamic_cast<" + cls + "^>(obj);");
                    _sb.AppendLine("    if (clr == CLR_NULL)");
                    _sb.AppendLine("    {");
                    _sb.AppendLine("        return false;");
                    _sb.AppendLine("    }\n");
                    _sb.AppendLine("    if (_native == NULL) throw gcnew Exception(\"The underlying native object for the caller is null.\");");
                    _sb.AppendLine("    if (clr->_native == NULL) throw gcnew ArgumentException(\"The underlying native object for parameter 'obj' is null.\");");
                    _sb.AppendLine();
                    _sb.AppendLine("    return " + GetNativeInvokationTargetObject() + " == *(static_cast<" + _t.FullNativeName + "*>(clr->_native));");
                    _sb.AppendLine("}\n");

                    if (!_t.HasWrapType(WrapTypes.NativePtrValueType))
                    {
                        _sb.AppendLine("bool " + cls + "::Equals(" + cls + "^ obj)");
                        _sb.AppendLine("{");
                        _sb.AppendLine("    if (obj == CLR_NULL)");
                        _sb.AppendLine("    {");
                        _sb.AppendLine("        return false;");
                        _sb.AppendLine("    }\n");
                        _sb.AppendLine("    if (_native == NULL) throw gcnew Exception(\"The underlying native object for the caller is null.\");");
                        _sb.AppendLine("    if (obj->_native == NULL) throw gcnew ArgumentException(\"The underlying native object for parameter 'obj' is null.\");");
                        _sb.AppendLine();
                        _sb.AppendLine("    return " + GetNativeInvokationTargetObject() + " == *(static_cast<" + _wrapper.NativeNamespace + "::" + cls + "*>(obj->_native));");
                        _sb.AppendLine("}");

                        _sb.AppendLine();
                        _sb.AppendLine("bool " + cls + "::operator ==(" + cls + "^ obj1, " + cls + "^ obj2)");
                        _sb.AppendLine("{");
                        _sb.IncreaseIndent();
                        _sb.AppendLine("if ((Object^)obj1 == (Object^)obj2) return true;");
                        _sb.AppendLine("if ((Object^)obj1 == nullptr || (Object^)obj2 == nullptr) return false;");
                        _sb.AppendLine();
                        _sb.AppendLine("return obj1->Equals(obj2);");
                        _sb.DecreaseIndent();
                        _sb.AppendLine("}");

                        _sb.AppendLine();
                        _sb.AppendLine("bool " + cls + "::operator !=(" + cls + "^ obj1, " + cls + "^ obj2)");
                        _sb.AppendLine("{");
                        _sb.AppendLine("\treturn !(obj1 == obj2);");
                        _sb.AppendLine("}");
                    }
                    else
                    {
                        _sb.AppendLine("bool " + cls + "::Equals(" + cls + " obj)");
                        _sb.AppendLine("{");
                        _sb.AppendLine("    if (_native == NULL) throw gcnew Exception(\"The underlying native object for the caller is null.\");");
                        _sb.AppendLine("    if (obj._native == NULL) throw gcnew ArgumentException(\"The underlying native object for parameter 'obj' is null.\");");
                        _sb.AppendLine();
                        _sb.AppendLine("    return *_native == *obj._native;");
                        _sb.AppendLine("}");

                        _sb.AppendLine();
                        _sb.AppendLine("bool " + cls + "::operator ==(" + cls + " obj1, " + cls + " obj2)");
                        _sb.AppendLine("{");
                        _sb.AppendLine("\treturn obj1.Equals(obj2);");
                        _sb.AppendLine("}");

                        _sb.AppendLine();
                        _sb.AppendLine("bool " + cls + "::operator !=(" + cls + " obj1, " + cls + " obj2)");
                        _sb.AppendLine("{");
                        _sb.AppendLine("\treturn !obj1.Equals(obj2);");
                        _sb.AppendLine("}");
                    }
                    break;
            }
        }
    }
}
