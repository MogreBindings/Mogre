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
    abstract class ClassCppCodeProducer : ClassCodeProducer
    {
        public ClassCppCodeProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
            //if (AllowSubclassing)
            //{
            //    _wrapper.PreClassProducers.Add(new CppNativeProtectedTypesProxy(_wrapper, _t, _sb));
            //}
        }

        protected override void AddTypeDependancy(AbstractTypeDefinition type)
        {
            _wrapper.CppCheckTypeForDependancy(type);
        }

        protected virtual string GetCLRTypeName(ITypeMember m)
        {
            AddTypeDependancy(m.MemberType);
            if (m.MemberType.IsUnnamedSTLContainer)
                return GetClassName() + "::" + m.MemberTypeCLRName;
            else
                return m.MemberTypeCLRName;
        }

        protected virtual string GetCLRParamTypeName(ParamDefinition param)
        {
            AddTypeDependancy(param.Type);
            return param.Type.GetCLRParamTypeName(param);
        }

        protected override void AddPostBody()
        {
            base.AddPostBody();
            _code.AppendEmptyLine();

            if (_definition.HasAttribute<CLRObjectAttribute>(true)) {
                _code.AppendLine("__declspec(dllexport) " + _wrapper.GetInitCLRObjectFuncSignature(_definition));
                _code.AppendLine("{");
                _code.AppendLine("\t*pClrObj = gcnew " + _definition.FullCLRName + "(pClrObj);");
                _code.AppendLine("}");
            }

            _code.AppendEmptyLine();
        }

        protected override void AddInternalDeclarations()
        {
            base.AddInternalDeclarations();

            foreach (ClassDefinition cls in _interfaces)
            {
                _code.AppendLine(cls.FullNativeName + "* " + GetClassName() + "::_" + cls.CLRName + "_GetNativePtr()");
                _code.AppendLine("{");
                _code.AppendLine("\treturn static_cast<" + cls.FullNativeName + "*>( " + GetNativeInvokationTarget() + " );");
                _code.AppendLine("}");
                _code.AppendEmptyLine();
            }
        }

		protected override void AddPublicDeclarations()
		{
			if ((!_definition.IsNativeAbstractClass || _definition.IsInterface)
					&& IsConstructable)
			{
				if (_definition.Constructors.Length > 0)
				{
					foreach (MemberMethodDefinition function in _definition.Constructors)
					{
						if (function.ProtectionLevel == ProtectionLevel.Public &&
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

				_code.AppendEmptyLine();
			}

			base.AddPublicDeclarations();
		}

        protected virtual void AddPublicConstructor(MemberMethodDefinition function)
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
                    foreach (ParamDefinition param in function.Parameters)
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

        protected virtual void AddPublicConstructorOverload(MemberMethodDefinition f, int count)
        {
            _code.AppendIndent(GetClassName() + "::" + _definition.CLRName);
            if (f == null)
                _code.Append("()");
            else
                AddMethodParameters(f, count);

            string nativeType = GetTopClass(_definition).FullNativeName;
            if (GetTopBaseClassName() == "Wrapper")
                nativeType = "CLRObject";

            if (GetBaseClassName() != null)
                _code.Append(" : " + GetBaseClassName() + "((" + nativeType + "*) 0)");

            _code.Append("\n");
            _code.AppendLine("{");
            _code.IncreaseIndent();

            if (!_definition.IsInterface)
                _code.AppendLine("_createdByCLR = true;");

            string preCall = null, postCall = null;

            if (f != null)
            {
                preCall = GetMethodPreNativeCall(f, count);
                postCall = GetMethodPostNativeCall(f, count);

                if (!String.IsNullOrEmpty(preCall))
                    _code.AppendLine(preCall);
            }

            _code.AppendIndent("_native = new " + _definition.FullNativeName + "(");

            if (count > 0)
            {
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

            if (!String.IsNullOrEmpty(postCall))
            {
                _code.AppendEmptyLine();
                _code.AppendLine(postCall);
                _code.AppendEmptyLine();
            }

            AddConstructorBody();

            _code.DecreaseIndent();
            _code.AppendLine("}");
        }

        protected override void AddStaticConstructor()
        {
            if (_definition.IsInterface)
                _code.AppendLine("static " + _definition.Name + "::" + _definition.Name + "()");
            else
                _code.AppendLine("static " + _definition.CLRName + "::" + _definition.CLRName + "()");

            _code.AppendLine("{");
            _code.IncreaseIndent();
            foreach (MemberDefinitionBase m in _cachedMembers)
            {
                if (m.IsStatic)
                {
                    _code.AppendIndent(NameToPrivate(m) + " = ");
                    if (m.ProtectionLevel == ProtectionLevel.Protected)
                    {
                        _code.Append(NativeProtectedTypesProxy.GetProtectedTypesProxyName(m.Class));
                        _code.Append("::" + m.Name + ";\n");
                    }
                    else
                    {
                        _code.Append(m.Class.FullNativeName);
                        _code.Append("::" + m.Name + ";\n");
                    }
                }
            }
            _code.DecreaseIndent();
            _code.AppendLine("}");
        }

        protected override void AddPostNestedTypes()
        {
            base.AddPostNestedTypes();

            if (_definition.HasAttribute<CustomCppDeclarationAttribute>())
            {
                string txt = _definition.GetAttribute<CustomCppDeclarationAttribute>().DeclarationText;
                txt = ReplaceCustomVariables(txt);
                _code.AppendLine(txt);
                _code.AppendEmptyLine();
            }
        }

        protected override void AddNestedTypeBeforeMainType(AbstractTypeDefinition nested)
        {
            base.AddNestedType(nested);
            _wrapper.CppAddType(nested, _code);
        }

        protected override void AddNestedType(AbstractTypeDefinition nested)
        {
            if (nested.HasWrapType(WrapTypes.NativeDirector))
            {
                //Interface and native director are already declared before the declaration of this class.
                return;
            }

            base.AddNestedType(nested);
            _wrapper.CppAddType(nested, _code);
        }

        protected override void AddMethod(MemberMethodDefinition f)
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
                    _code.AppendLine(txt);
                    _code.AppendEmptyLine();
                    return;
                }
            }

            int defcount = 0;

            if (!f.HasAttribute<NoDefaultParamOverloadsAttribute>())
            {
                foreach (ParamDefinition param in f.Parameters)
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

                _code.AppendIndent(GetCLRTypeName(f) + " " + GetClassName() + "::" + f.CLRName);
                AddMethodParameters(f, f.Parameters.Count - dc);
                _code.Append("\n");
                _code.AppendLine("{");
                _code.IncreaseIndent();

                bool isVirtualOverload = dc > 0 && methodIsVirtual && AllowVirtualMethods;

                if (isVirtualOverload)
                {
                    // Overloads (because of default values)
                    // main method is virtual, call it with CLR default values if _isOverriden=true,
                    // else do a normal native call

                    _code.AppendLine("if (_isOverriden)");
                    _code.AppendLine("{");
                    _code.IncreaseIndent();

                    bool hasPostConversions = false;
                    for (int i = f.Parameters.Count - dc; i < f.Parameters.Count; i++)
                    {
                        ParamDefinition p = f.Parameters[i];
                        if (!String.IsNullOrEmpty(p.CLRDefaultValuePreConversion))
                            _code.AppendLine(p.CLRDefaultValuePreConversion);
                        if (!String.IsNullOrEmpty(p.CLRDefaultValuePostConversion))
                            hasPostConversions = true;

                        string n1, n2, n3;
                        AbstractTypeDefinition dependancy;
                        p.Type.ProduceDefaultParamValueConversionCode(p, out n1, out n2, out n3, out dependancy);
                        if (dependancy != null)
                            AddTypeDependancy(dependancy);
                    }

                    _code.AppendIndent("");
                    if (!f.HasReturnValue)
                    {
                        if (hasPostConversions)
                        {
                            _code.Append(GetCLRTypeName(f) + " mp_return = ");
                        }
                        else
                        {
                            _code.Append("return ");
                        }
                    }

                    _code.Append(f.CLRName + "(");
                    for (int i = 0; i < f.Parameters.Count; i++)
                    {
                        ParamDefinition p = f.Parameters[i];
                        _code.Append(" ");
                        if (i < f.Parameters.Count - dc)
                            _code.Append(p.Name);
                        else
                        {
                            _code.Append(p.CLRDefaultValue);
                        }
                        if (i < f.Parameters.Count - 1) _code.Append(",");
                    }
                    _code.Append(" );\n");

                    for (int i = f.Parameters.Count - dc; i < f.Parameters.Count; i++)
                    {
                        ParamDefinition p = f.Parameters[i];
                        if (!String.IsNullOrEmpty(p.CLRDefaultValuePostConversion))
                            _code.AppendLine(p.CLRDefaultValuePostConversion);
                    }

                    if (!f.HasReturnValue && hasPostConversions)
                    {
                        _code.AppendLine("return mp_return;");
                    }

                    _code.DecreaseIndent();
                    _code.AppendLine("}");
                    _code.AppendLine("else");
                    _code.AppendLine("{");
                    _code.IncreaseIndent();
                }

                AddMethodBody(f, f.Parameters.Count - dc);

                if (isVirtualOverload)
                {
                    _code.DecreaseIndent();
                    _code.AppendLine("}");
                }

                _code.DecreaseIndent();
                _code.AppendLine("}");
            }
        }

        protected virtual void AddMethodParameters(MemberMethodDefinition f, int count)
        {
            _code.Append("(");
            for (int i = 0; i < count; i++)
            {
                ParamDefinition p = f.Parameters[i];
                _code.Append(" " + GetCLRParamTypeName(p) + " " + p.Name);
                if (i < count - 1) _code.Append(",");
            }
            _code.Append(" )");
        }
        protected void AddMethodParameters(MemberMethodDefinition f)
        {
            AddMethodParameters(f, f.Parameters.Count);
        }

        protected virtual void AddMethodBody(MemberMethodDefinition f, int count)
        {
            string preCall = GetMethodPreNativeCall(f, count);
            string nativeCall = GetMethodNativeCall(f, count);
            string postCall = GetMethodPostNativeCall(f, count);

            if (!String.IsNullOrEmpty(preCall))
                _code.AppendLine(preCall);

            if (f.HasReturnValue)
            {
                _code.AppendLine(nativeCall + ";");
                if (!String.IsNullOrEmpty(postCall))
                    _code.AppendLine(postCall);
            }
            else
            {
                if (String.IsNullOrEmpty(postCall))
                {
                    _code.AppendLine("return " + nativeCall + ";");
                }
                else
                {
                    _code.AppendLine(GetCLRTypeName(f) + " retres = " + nativeCall + ";");
                    _code.AppendLine(postCall);
                    _code.AppendLine("return retres;");
                }
            }
        }

        protected virtual string GetMethodPreNativeCall(MemberMethodDefinition f, int paramCount)
        {
            string res = String.Empty;

            for (int i = 0; i < paramCount; i++)
            {
                ParamDefinition p = f.Parameters[i];
                string newname;
                res += p.Type.ProducePreCallParamConversionCode(p, out newname);
            }

            return res;
        }

        protected virtual string GetMethodNativeCall(MemberMethodDefinition f, int paramCount)
        {
            string invoke;
            if (f.IsStatic)
            {
                if (f.ProtectionLevel == ProtectionLevel.Protected)
                {
                    string classname = NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(_definition);
                    invoke = classname + "::" + f.Name + "(";
                }
                else
                {
                    invoke = _definition.FullNativeName + "::" + f.Name + "(";
                }
            }
            else
            {
                invoke = GetNativeInvokationTarget(f) + "(";
            }

            for (int i = 0; i < paramCount; i++)
            {
                ParamDefinition p = f.Parameters[i];
                string newname;
                p.Type.ProducePreCallParamConversionCode(p, out newname);
                invoke += " " + newname;
                if (i < paramCount - 1) invoke += ",";
            }

            invoke += " )";

            if (f.HasReturnValue)
                return invoke;
            else
                return f.MemberType.ProduceNativeCallConversionCode(invoke, f);
        }

        protected virtual string GetMethodPostNativeCall(MemberMethodDefinition f, int paramCount)
        {
            string res = String.Empty;

            for (int i = 0; i < paramCount; i++)
            {
                ParamDefinition p = f.Parameters[i];
                res += p.Type.ProducePostCallParamConversionCleanupCode(p);
            }

            return res;
        }

        protected string AddParameterConversion(ParamDefinition param)
        {
            string newname, expr, postcall;
            expr = param.Type.ProducePreCallParamConversionCode(param, out newname);
            postcall = param.Type.ProducePostCallParamConversionCleanupCode(param);
            if (!String.IsNullOrEmpty(postcall))
                throw new Exception("Unexpected");

            if (!String.IsNullOrEmpty(expr))
                _code.AppendLine(expr);

            return newname;
        }

        protected override void AddProperty(PropertyDefinition p)
        {
            string ptype = GetCLRTypeName(p);
            string pname =  GetClassName() + "::" + p.Name;
            if (p.CanRead)
            {
                if (!(p.GetterFunction.IsAbstract && AllowSubclassing))
                {
                    if (AllowProtectedMembers || p.GetterFunction.ProtectionLevel != ProtectionLevel.Protected)
                    {
                        string managedType = GetMethodNativeCall(p.GetterFunction, 0);

                        _code.AppendLine(ptype + " " + pname + "::get()");
                        _code.AppendLine("{");
                        if (_cachedMembers.Contains(p.GetterFunction))
                        {
                            string priv = NameToPrivate(p.Name);
                            _code.AppendLine("\treturn ( CLR_NULL == " + priv + " ) ? (" + priv + " = " + managedType + ") : " + priv + ";");
                        }
                        else
                        {
                            _code.AppendLine("\treturn " + managedType + ";");
                        }
                        _code.AppendLine("}");
                    }
                }
            }

            if (p.CanWrite)
            {
                if (!(p.SetterFunction.IsAbstract && AllowSubclassing))
                {
                    if (AllowProtectedMembers || p.SetterFunction.ProtectionLevel != ProtectionLevel.Protected)
                    {
                        _code.AppendLine("void " + pname + "::set( " + ptype + " " + p.SetterFunction.Parameters[0].Name + " )");
                        _code.AppendLine("{");
                        _code.IncreaseIndent();

                        string preCall = GetMethodPreNativeCall(p.SetterFunction, 1);
                        string nativeCall = GetMethodNativeCall(p.SetterFunction, 1);
                        string postCall = GetMethodPostNativeCall(p.SetterFunction, 1);

                        if (!String.IsNullOrEmpty(preCall))
                            _code.AppendLine(preCall);

                        _code.AppendLine(nativeCall + ";");

                        if (!String.IsNullOrEmpty(postCall))
                            _code.AppendLine(postCall);

                        _code.DecreaseIndent();
                        _code.AppendLine("}");
                    }
                }
            }
        }

        protected override void AddPropertyField(MemberFieldDefinition field)
        {
            string ptype = GetCLRTypeName(field);
            string pname = GetClassName () + "::" + (field.HasAttribute<RenameAttribute>() ? field.GetAttribute<RenameAttribute> ().Name : field.Name);

            if (field.IsNativeArray)
            {
                if (field.MemberType.HasAttribute<NativeValueContainerAttribute>()
                    || (field.MemberType.IsValueType && !field.MemberType.HasWrapType(WrapTypes.NativePtrValueType)))
                {
                    ParamDefinition tmpParam = new ParamDefinition(this.MetaDef, field, field.Name + "_array");
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
                    string managedType = field.MemberType.ProduceNativeCallConversionCode(GetNativeInvokationTarget(field), tmpParam);

                    _code.AppendLine(ptype + " " + pname + "::get()");
                    _code.AppendLine("{");
                    _code.AppendLine("\treturn " + managedType + ";");
                    _code.AppendLine("}");
                }
                else
                {
                    string managedType = field.MemberType.ProduceNativeCallConversionCode(GetNativeInvokationTarget(field) + "[index]", field);

                    _code.AppendLine(ptype + " " + pname + "::get(int index)");
                    _code.AppendLine("{");
                    _code.AppendLine("\tif (index < 0 || index >= " + field.ArraySize + ") throw gcnew IndexOutOfRangeException();");
                    _code.AppendLine("\treturn " + managedType + ";");
                    _code.AppendLine("}");
                    _code.AppendLine("void " + pname + "::set(int index, " + ptype + " value )");
                    _code.AppendLine("{");
                    _code.IncreaseIndent();
                    _code.AppendLine("if (index < 0 || index >= " + field.ArraySize + ") throw gcnew IndexOutOfRangeException();");
                    string param = AddParameterConversion(new ParamDefinition(this.MetaDef, field, "value"));
                    _code.AppendLine(GetNativeInvokationTarget(field) + "[index] = " + param + ";");
                    _code.DecreaseIndent();
                    _code.AppendLine("}");
                }
            }
            else if (_cachedMembers.Contains(field))
            {
                string managedType;
                if (field.MemberType.IsSTLContainer)
                {
                    managedType = GetNativeInvokationTarget(field);
                }
                else
                {
                    managedType = field.MemberType.ProduceNativeCallConversionCode(GetNativeInvokationTarget(field), field);
                }
                string priv = NameToPrivate(field);

                _code.AppendLine(ptype + " " + pname + "::get()");
                _code.AppendLine("{");
                if (!field.IsStatic)
                    _code.AppendLine("\treturn ( CLR_NULL == " + priv + " ) ? (" + priv + " = " + managedType + ") : " + priv + ";");
                else
                    _code.AppendLine("\treturn " + priv + ";");
                _code.AppendLine("}");
            }
            else
            {
                string managedType = field.MemberType.ProduceNativeCallConversionCode(GetNativeInvokationTarget(field), field);

                _code.AppendLine(ptype + " " + pname + "::get()");
                _code.AppendLine("{");
                _code.AppendLine("\treturn " + managedType + ";");
                _code.AppendLine("}");

                if ( // SharedPtrs can be copied by value. Let all be copied by value just to be sure (field.PassedByType == PassedByType.Pointer || field.Type.IsValueType)
                    !IsReadOnly && !field.MemberType.HasAttribute<ReadOnlyForFieldsAttribute>()
                    && !field.IsConst)
                {
                    _code.AppendLine("void " + pname + "::set( " + ptype + " value )");
                    _code.AppendLine("{");
                    _code.IncreaseIndent();
                    string param = AddParameterConversion(new ParamDefinition(this.MetaDef, field, "value"));
                    _code.AppendLine(GetNativeInvokationTarget(field) + " = " + param + ";");
                    _code.DecreaseIndent();
                    _code.AppendLine("}");
                }
            }
        }

        protected override void AddMethodsForField(MemberFieldDefinition field)
        {
            string managedType = field.MemberType.ProduceNativeCallConversionCode(GetNativeInvokationTarget(field), field);

            _code.AppendLine(GetCLRTypeName(field) + " " + GetClassName() + "::get_" + field.Name + "()");
            _code.AppendLine("{");
            _code.AppendLine("\treturn " + managedType + ";");
            _code.AppendLine("}");

            ParamDefinition param = new ParamDefinition(this.MetaDef, field, "value");
            _code.AppendLine("void " + GetClassName() + "::set_" + field.Name + "(" + param.Type.GetCLRParamTypeName(param) + " value)");
            _code.AppendLine("{");
            _code.IncreaseIndent();
            _code.AppendLine(GetNativeInvokationTarget(field) + " = " + AddParameterConversion(param) + ";");
            _code.DecreaseIndent();
            _code.AppendLine("}");
        }

        protected override void AddPredefinedMethods(PredefinedMethods pm)
        {
            string cls = GetClassName();
            switch (pm)
            {
                case PredefinedMethods.Equals:
                    _code.AppendLine("bool " + cls + "::Equals(Object^ obj)");
                    _code.AppendLine("{");
                    _code.AppendLine("    " + cls + "^ clr = dynamic_cast<" + cls + "^>(obj);");
                    _code.AppendLine("    if (clr == CLR_NULL)");
                    _code.AppendLine("    {");
                    _code.AppendLine("        return false;");
                    _code.AppendLine("    }\n");
                    _code.AppendLine("    if (_native == NULL) throw gcnew Exception(\"The underlying native object for the caller is null.\");");
                    _code.AppendLine("    if (clr->_native == NULL) throw gcnew ArgumentException(\"The underlying native object for parameter 'obj' is null.\");");
                    _code.AppendEmptyLine();
                    _code.AppendLine("    return " + GetNativeInvokationTargetObject() + " == *(static_cast<" + _definition.FullNativeName + "*>(clr->_native));");
                    _code.AppendLine("}\n");

                    if (!_definition.HasWrapType(WrapTypes.NativePtrValueType))
                    {
                        _code.AppendLine("bool " + cls + "::Equals(" + cls + "^ obj)");
                        _code.AppendLine("{");
                        _code.AppendLine("    if (obj == CLR_NULL)");
                        _code.AppendLine("    {");
                        _code.AppendLine("        return false;");
                        _code.AppendLine("    }\n");
                        _code.AppendLine("    if (_native == NULL) throw gcnew Exception(\"The underlying native object for the caller is null.\");");
                        _code.AppendLine("    if (obj->_native == NULL) throw gcnew ArgumentException(\"The underlying native object for parameter 'obj' is null.\");");
                        _code.AppendEmptyLine();
                        _code.AppendLine("    return " + GetNativeInvokationTargetObject() + " == *(static_cast<" + _wrapper.NativeNamespace + "::" + cls + "*>(obj->_native));");
                        _code.AppendLine("}");

                        _code.AppendEmptyLine();
                        _code.AppendLine("bool " + cls + "::operator ==(" + cls + "^ obj1, " + cls + "^ obj2)");
                        _code.AppendLine("{");
                        _code.IncreaseIndent();
                        _code.AppendLine("if ((Object^)obj1 == (Object^)obj2) return true;");
                        _code.AppendLine("if ((Object^)obj1 == nullptr || (Object^)obj2 == nullptr) return false;");
                        _code.AppendEmptyLine();
                        _code.AppendLine("return obj1->Equals(obj2);");
                        _code.DecreaseIndent();
                        _code.AppendLine("}");

                        _code.AppendEmptyLine();
                        _code.AppendLine("bool " + cls + "::operator !=(" + cls + "^ obj1, " + cls + "^ obj2)");
                        _code.AppendLine("{");
                        _code.AppendLine("\treturn !(obj1 == obj2);");
                        _code.AppendLine("}");
                    }
                    else
                    {
                        _code.AppendLine("bool " + cls + "::Equals(" + cls + " obj)");
                        _code.AppendLine("{");
                        _code.AppendLine("    if (_native == NULL) throw gcnew Exception(\"The underlying native object for the caller is null.\");");
                        _code.AppendLine("    if (obj._native == NULL) throw gcnew ArgumentException(\"The underlying native object for parameter 'obj' is null.\");");
                        _code.AppendEmptyLine();
                        _code.AppendLine("    return *_native == *obj._native;");
                        _code.AppendLine("}");

                        _code.AppendEmptyLine();
                        _code.AppendLine("bool " + cls + "::operator ==(" + cls + " obj1, " + cls + " obj2)");
                        _code.AppendLine("{");
                        _code.AppendLine("\treturn obj1.Equals(obj2);");
                        _code.AppendLine("}");

                        _code.AppendEmptyLine();
                        _code.AppendLine("bool " + cls + "::operator !=(" + cls + " obj1, " + cls + " obj2)");
                        _code.AppendLine("{");
                        _code.AppendLine("\treturn !obj1.Equals(obj2);");
                        _code.AppendLine("}");
                    }
                    break;
            }
        }
    }
}
