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
    abstract class IncClassProducer : ClassProducer
    {
        public IncClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
            AddPreDeclarations();

            if (_t.BaseClass != null)
                AddTypeDependancy(_t.BaseClass);

            if (AllowSubclassing)
            {
                _wrapper.PreClassProducers.Add(new NativeProtectedTypesProxy(_wrapper, _t, _sb));
                _wrapper.PostClassProducers.Add(new NativeProtectedStaticsProxy(_wrapper, _t, _sb));
                //_wrapper.PreClassProducers.Add(new IncNativeProtectedTypesProxy(_wrapper, _t, _sb));
            }
        }

        protected override void AddTypeDependancy(DefType type)
        {
            base.AddTypeDependancy(type);
            _wrapper.AddTypeDependancy(type);
        }

        protected virtual void AddPreDeclarations()
        {
            if (!_t.IsNested)
            {
                _wrapper.AddPreDeclaration("ref class " + _t.CLRName + ";");
                _wrapper.AddPragmaMakePublicForType(_t);
            }
        }

        protected virtual void CheckTypeForDependancy(DefType type)
        {
            _wrapper.CheckTypeForDependancy(type);
        }

        protected virtual string GetCLRTypeName(ITypeMember m)
        {
            CheckTypeForDependancy(m.Type);
            return m.CLRTypeName;
        }

        protected virtual string GetCLRParamTypeName(DefParam param)
        {
            CheckTypeForDependancy(param.Type);
            return param.Type.GetCLRParamTypeName(param);
        }

        protected virtual void AddDefinition()
        {
            _sb.AppendIndent("");
            if (!_t.IsNested)
                _sb.Append("public ");
            else
                _sb.Append(GetProtectionString(_t.ProtectionType) + ": ");
            string baseclass = GetBaseAndInterfaces();
            if (baseclass != "")
                _sb.AppendFormat("ref class {0}{1} : {2}\n", _t.CLRName, (IsAbstractClass) ? " abstract" : "", baseclass);
            else
                _sb.AppendFormat("ref class {0}{1}\n", _t.CLRName, (IsAbstractClass) ? " abstract" : "");
        }

        protected override void AddInterfaceMethod(DefFunction f)
        {
            _sb.DecreaseIndent();
            _sb.AppendLine(GetProtectionString(f.ProtectionType) + ":");
            _sb.IncreaseIndent();
            base.AddInterfaceMethod(f);
        }

        protected override void AddInterfaceMethodsForField(DefField field)
        {
            _sb.DecreaseIndent();
            _sb.AppendLine(GetProtectionString(field.ProtectionType) + ":");
            _sb.IncreaseIndent();
            base.AddInterfaceMethodsForField(field);
        }

        protected override void AddPreBody()
        {
            base.AddPreBody();

            AddComments();
            AddDefinition();

            _sb.AppendLine("{");
            _sb.IncreaseIndent();
        }

        protected override void AddPostBody()
        {
            base.AddPostBody();

            _sb.DecreaseIndent();
            _sb.AppendLine("};\n");
        }

        protected override void AddPrivateDeclarations()
        {
            _sb.DecreaseIndent();
            if (IsNativeClass)
                _sb.AppendLine("private:");
            else
                _sb.AppendLine("private protected:");
            _sb.IncreaseIndent();
            base.AddPrivateDeclarations();

            AddCachedFields();

            if (_listeners.Count > 0)
            {
                _sb.AppendLine("\n//Event and Listener fields");
                AddEventFields();
            }
        }

        protected override void AddStaticConstructor()
        {
            if (_t.IsInterface)
                _sb.AppendLine("static " + _t.Name + "();");
            else
                _sb.AppendLine("static " + _t.CLRName + "();");
        }

        protected virtual void AddEventFields()
        {
            foreach (DefClass cls in _listeners)
            {
                _sb.AppendLine(GetNativeDirectorName(cls) + "* " + NameToPrivate(cls.Name) + ";");
                foreach (DefFunction f in cls.PublicMethods)
                {
                    if (f.IsDeclarableFunction)
                    {
                        _sb.AppendLine(cls.FullCLRName + "::" + f.CLRName + "Handler^ " + NameToPrivate(f.Name) + ";");
                    }
                    else
                        continue;

                    if (cls.HasAttribute<StopDelegationForReturnAttribute>())
                    {
                        _sb.AppendLine("array<Delegate^>^ " + NameToPrivate(f.Name) + "Delegates;");
                    }
                }

                _sb.AppendLine("");
            }
        }

        protected virtual bool DoCleanupInFinalizer
        {
            get { return _t.HasAttribute<DoCleanupInFinalizerAttribute>(); }
        }

        protected override void AddInternalDeclarations()
        {
            _sb.DecreaseIndent();
            _sb.AppendLine("public protected:");
            _sb.IncreaseIndent();
            base.AddInternalDeclarations();

            AddInternalConstructors();

            if (RequiresCleanUp)
            {
                if (DoCleanupInFinalizer)
                {
                    _sb.AppendLine("~" + _t.CLRName + "()\n{");
                    _sb.AppendLine("\tthis->!" + _t.CLRName + "();");
                    _sb.AppendLine("}");
                    _sb.AppendLine("!" + _t.CLRName + "()\n{");
                }
                else
                    _sb.AppendLine("~" + _t.CLRName + "()\n{");
                _sb.IncreaseIndent();
                AddDisposerBody();
                _sb.DecreaseIndent();
                _sb.AppendLine("}\n");
            }

            foreach (DefClass cls in _interfaces)
            {
                _sb.AppendLine("virtual " + cls.FullNativeName + "* _" + cls.CLRName + "_GetNativePtr() = " + cls.CLRName + "::_GetNativePtr;");
                _sb.AppendLine();
            }
        }

        protected virtual void AddInternalConstructors()
        {
        }

        protected virtual bool RequiresCleanUp
        {
            get { return _listeners.Count > 0; }
        }

        protected virtual void AddDisposerBody()
        {
            if (_t.HasAttribute<CustomDisposingAttribute>())
            {
                string text = _t.GetAttribute<CustomDisposingAttribute>().Text;
                _sb.AppendLine(text);
            }

            foreach (DefClass cls in _listeners)
            {
                DefFunction removerFunc = null;
                foreach (DefFunction func in _t.PublicMethods)
                {
                    if (func.IsListenerRemover && func.Parameters[0].Type == cls)
                    {
                        removerFunc = func;
                        break;
                    }
                }
                if (removerFunc == null)
                    throw new Exception("Unexpected");

                string native = "_native";
                _sb.AppendLine(String.Format("if ({0} != 0)\n{{\n\tif (" + native + " != 0) " + GetNativeInvokationTarget(removerFunc) + "({0});\n\tdelete {0}; {0} = 0;\n}}", NameToPrivate(cls.Name)));
            }
        }

        protected override void AddPublicDeclarations()
        {
            _sb.DecreaseIndent();
            _sb.AppendLine("public:");
            _sb.IncreaseIndent();

            if (IsConstructable)
            {
                AddPublicConstructors();
            }

            _sb.AppendLine("");
            AddPublicFields();
            _sb.AppendLine("");

            if (_listeners.Count > 0)
            {
                AddEventMethods();
            }
            base.AddPublicDeclarations();
        }

        protected override void AddPostNestedTypes()
        {
            base.AddPostNestedTypes();

            if (_t.HasAttribute<CustomIncDeclarationAttribute>())
            {
                string txt = _t.GetAttribute<CustomIncDeclarationAttribute>().DeclarationText;
                txt = ReplaceCustomVariables(txt);
                _sb.AppendLine(txt);
                _sb.AppendLine();
            }
        }

		protected virtual void AddPublicConstructors()
		{
			if (_t.IsNativeAbstractClass && !_t.IsInterface)
				return;

			if (_t.Constructors.Length > 0)
			{
				foreach (DefFunction func in _t.Constructors)
				{
					if (func.ProtectionType == ProtectionType.Public && 
						!func.HasAttribute<IgnoreAttribute>())
					{
						AddPublicConstructor(func);
					}
				}
			}
			else
			{
				AddPublicConstructor(null);
			}
		}

        protected virtual void AddPublicConstructor(DefFunction function)
        {
            string className = (_t.IsInterface) ? _t.Name : _t.CLRName;

            if (function == null)
			{
                _sb.AppendLine(className + "();");
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
					if (dc < defcount && hideParams)
                        continue;


                    _sb.AppendIndent(className);
                    AddMethodParameters(function, function.Parameters.Count - dc);
                    _sb.Append(";\n");
                }

            }
        }

        protected virtual void AddPublicFields()
        {
        }

        protected virtual void AddEventInvokers()
        {
            foreach (DefClass cls in _listeners)
            {
                foreach (DefFunction f in cls.PublicMethods)
                {
                    if (f.IsDeclarableFunction)
                    {
                        _sb.AppendIndent("virtual " + GetCLRTypeName(f) + " On" + f.CLRName);
                        AddMethodParameters(f);
                        _sb.Append(" = " + GetNativeDirectorReceiverInterfaceName(cls) + "::" + f.CLRName + "\n");
                        _sb.AppendLine("{");
                        _sb.AppendIndent("\t");
                        if (f.TypeName != "void") _sb.Append("return ");
                        _sb.Append(f.CLRName + "(");
                        for (int i = 0; i < f.Parameters.Count; i++)
                        {
                            DefParam param = f.Parameters[i];
                            _sb.Append(" " + param.Name);
                            if (i < f.Parameters.Count - 1) _sb.Append(",");
                        }
                        _sb.Append(" );\n");
                        _sb.AppendLine("}\n");
                    }
                }

                _sb.AppendLine("");
            }
        }

        protected virtual void AddEventMethods()
        {
            foreach (DefClass cls in _listeners)
            {
                DefFunction adderFunc = null;
                foreach (DefFunction func in _t.PublicMethods)
                {
                    if (func.IsListenerAdder && func.Parameters[0].Type == cls)
                    {
                        adderFunc = func;
                        break;
                    }
                }
                if (adderFunc == null)
                    throw new Exception("Unexpected");

                foreach (DefFunction f in cls.PublicMethods)
                {
                    if (f.IsDeclarableFunction)
                    {
                        string handler = cls.FullCLRName + "::" + f.CLRName + "Handler^";
                        string privField = NameToPrivate(f.Name);
                        string listener = NameToPrivate(cls.Name);
                        _sb.AppendLine("event " + handler + " " + f.CLRName);
                        _sb.AppendLine("{");
                        _sb.IncreaseIndent();
                        _sb.AppendLine("void add(" + handler + " hnd)");
                        _sb.AppendLine("{");
                        _sb.IncreaseIndent();
                        _sb.AppendLine("if (" + privField + " == CLR_NULL)");
                        _sb.AppendLine("{");
                        _sb.IncreaseIndent();
                        _sb.AppendLine("if (" + listener + " == 0)");
                        _sb.AppendLine("{");
                        _sb.AppendLine("\t" + listener + " = new " + GetNativeDirectorName(cls) + "(this);");
                        _sb.AppendLine("\t" + GetNativeInvokationTarget(adderFunc) + "(" + listener + ");");
                        _sb.AppendLine("}");
                        _sb.AppendLine(listener + "->doCallFor" + f.CLRName + " = true;");
                        _sb.DecreaseIndent();
                        _sb.AppendLine("}");
                        _sb.AppendLine(privField + " += hnd;");

                        if (cls.HasAttribute<StopDelegationForReturnAttribute>())
                        {
                            _sb.AppendLine(privField + "Delegates = " + privField + "->GetInvocationList();");
                        }

                        _sb.DecreaseIndent();
                        _sb.AppendLine("}");
                        _sb.AppendLine("void remove(" + handler + " hnd)");
                        _sb.AppendLine("{");
                        _sb.AppendLine("\t" + privField + " -= hnd;");
                        _sb.AppendLine("\tif (" + privField + " == CLR_NULL) " + listener + "->doCallFor" + f.CLRName + " = false;");

                        if (cls.HasAttribute<StopDelegationForReturnAttribute>())
                        {
                            _sb.AppendLine("\tif (" + privField + " == CLR_NULL) " + privField + "Delegates = nullptr; else " + privField + "Delegates = " + privField + "->GetInvocationList();");
                        }

                        _sb.AppendLine("}");
                        _sb.DecreaseIndent();
                        _sb.AppendLine("private:");
                        _sb.IncreaseIndent();
                        _sb.AppendIndent(GetCLRTypeName(f) + " raise");
                        AddMethodParameters(f);
                        _sb.Append("\n");
                        _sb.AppendLine("{");

                        if (cls.HasAttribute<StopDelegationForReturnAttribute>())
                        {
                            _sb.IncreaseIndent();
                            _sb.AppendLine("if (" + privField + ")");
                            _sb.AppendLine("{");
                            _sb.IncreaseIndent();
                            string list = privField + "Delegates";
                            string stopret = cls.GetAttribute<StopDelegationForReturnAttribute>().Return;
                            _sb.AppendLine(f.Type.FullCLRName + " mp_return;");
                            _sb.AppendLine("for (int i=0; i < " + list + "->Length; i++)");
                            _sb.AppendLine("{");
                            _sb.IncreaseIndent();
                            _sb.AppendIndent("mp_return = " + "static_cast<" + handler + ">(" + list + "[i])(");
                            for (int i = 0; i < f.Parameters.Count; i++)
                            {
                                DefParam param = f.Parameters[i];
                                _sb.Append(" " + param.Name);
                                if (i < f.Parameters.Count - 1) _sb.Append(",");
                            }
                            _sb.Append(" );\n");
                            _sb.AppendLine("if (mp_return == " + stopret + ") break;");
                            _sb.DecreaseIndent();
                            _sb.AppendLine("}");
                            _sb.AppendLine("return mp_return;");
                            _sb.DecreaseIndent();
                            _sb.AppendLine("}");
                            _sb.DecreaseIndent();
                        }
                        else
                        {
                            _sb.AppendLine("\tif (" + privField + ")");
                            _sb.AppendIndent("\t\t");
                            if (f.TypeName != "void") _sb.Append("return ");
                            _sb.Append(privField + "->Invoke(");
                            for (int i = 0; i < f.Parameters.Count; i++)
                            {
                                DefParam param = f.Parameters[i];
                                _sb.Append(" " + param.Name);
                                if (i < f.Parameters.Count - 1) _sb.Append(",");
                            }
                            _sb.Append(" );\n");
                        }

                        _sb.AppendLine("}");
                        _sb.DecreaseIndent();
                        _sb.AppendLine("}\n");
                    }
                }
                _sb.AppendLine("");
            }
        }

        protected override void AddProtectedDeclarations()
        {
            _sb.DecreaseIndent();
            _sb.AppendLine("protected public:");
            _sb.IncreaseIndent();
            base.AddProtectedDeclarations();

            if (_listeners.Count > 0)
            {
                AddEventInvokers();
            }
        }

        protected override void AddStaticField(DefField field)
        {
            base.AddStaticField(field);
            _sb.AppendIndent("");
            if (field.IsConst) _sb.Append("const ");
            if (field.IsStatic) _sb.Append("static ");
            _sb.Append(GetCLRTypeName(field) + " " + field.Name + " = " + field.Type.GetNativeCallConversion(field.FullNativeName, field) + ";\n\n");
        }

        protected override void AddNestedTypeBeforeMainType(DefType nested)
        {
            base.AddNestedType(nested);
            _wrapper.IncAddType(nested, _sb);
        }

        protected override void AddAllNestedTypes()
        {
            //Predeclare all nested classes in case there are classes referencing their "siblings"
            foreach (DefType nested in _t.NestedTypes)
            {
                if (nested.ProtectionType == ProtectionType.Public
                    || ((AllowProtectedMembers || AllowSubclassing) && nested.ProtectionType == ProtectionType.Protected))
                {
                    DefType expl = _t.FindType<DefType>(nested.Name);

                    if (expl.IsSTLContainer
                        || ( !nested.IsValueType && nested is DefClass && !(nested as DefClass).IsInterface && _wrapper.TypeIsWrappable(nested) ) )
                    {
                        _sb.AppendLine(GetProtectionString(nested.ProtectionType) + ": ref class " + nested.CLRName + ";");
                    }
                }
            }

            _sb.AppendLine();

            base.AddAllNestedTypes();
        }

        protected override void AddNestedType(DefType nested)
        {
            if (nested.HasWrapType(WrapTypes.NativeDirector))
            {
                //Interface and native director are already declared before the declaration of this class.
                //Just declare the method handlers of the class.
                IncNativeDirectorClassProducer.AddMethodHandlersClass((DefClass)nested, _sb);
                return;
            }

            base.AddNestedType(nested);
            _wrapper.IncAddType(nested, _sb);
        }

        protected virtual void AddCachedFields()
        {
            if (_cachedMembers.Count > 0)
            {
                _sb.AppendLine("//Cached fields");
                foreach (DefMember m in _cachedMembers)
                {
                    _sb.AppendIndent("");
                    if (m.IsStatic)
                    {
                        _sb.Append("static ");
                        _wrapper.UsedTypes.Add(m.Type);
                    }

                    _sb.Append(m.CLRTypeName + " " + NameToPrivate(m) + ";\n");
                }
            }
        }

        protected virtual string GetBaseAndInterfaces()
        {
            string baseclass = "";
            if (GetBaseClassName() != null)
                baseclass = "public " + GetBaseClassName();

            if (_interfaces.Count > 0)
            {
                if (baseclass != "") baseclass += ", ";

                foreach (DefClass it in _interfaces)
                {
                    AddTypeDependancy(it);
                    string itname = it.CLRName;
                    if (it.IsNested) itname = it.ParentClass.FullCLRName + "::" + itname;
                    baseclass += "public " + itname + ", ";
                }
                baseclass = baseclass.Substring(0, baseclass.Length - ", ".Length);
            }

            if (_listeners.Count > 0)
            {
                if (baseclass != "") baseclass += ", ";

                foreach (DefClass it in _listeners)
                {
                    AddTypeDependancy(it);
                    baseclass += "public " + GetNativeDirectorReceiverInterfaceName(it) + ", ";
                }
                baseclass = baseclass.Substring(0, baseclass.Length - ", ".Length);
            }

            return baseclass;
        }

        protected override void AddMethod(DefFunction f)
        {
            if (f.HasAttribute<CustomIncDeclarationAttribute>())
            {
                string txt = f.GetAttribute<CustomIncDeclarationAttribute>().DeclarationText;
                txt = ReplaceCustomVariables(txt, f);
                _sb.AppendLine(txt);
                _sb.AppendLine();
                return;
            }

            int defcount = 0;

            if (!f.HasAttribute<NoDefaultParamOverloadsAttribute>())
            {
                foreach (DefParam param in f.Parameters)
                    if (param.DefaultValue != null)
                        defcount++;
            }

            bool methodIsVirtual = DeclareAsVirtual(f);

            // The main method
            AddComments(f);

            if (AllowMethodIndexAttributes && f.IsVirtual && !f.IsAbstract)
                AddMethodIndexAttribute(f);

            _sb.AppendIndent("");
            if (f.IsStatic) _sb.Append("static ");
            if (methodIsVirtual) _sb.Append("virtual ");
            _sb.Append(GetCLRTypeName(f) + " " + f.CLRName);
            AddMethodParameters(f, f.Parameters.Count);
            if (DeclareAsOverride(f))
            {
                _sb.Append(" override");
            }
            else if (f.IsAbstract && AllowSubclassing)
            {
                _sb.Append(" abstract");
            }

            _sb.Append(";\n");

            if (AllowMethodOverloads)
            {
                // The overloads (because of default values)
                for (int dc = 1; dc <= defcount; dc++)
                {
                    if (dc < defcount && f.HasAttribute<HideParamsWithDefaultValuesAttribute>())
                        continue;

                    AddComments(f);
                    _sb.AppendIndent("");
                    if (f.IsStatic) _sb.Append("static ");
                    _sb.Append(GetCLRTypeName(f) + " " + f.CLRName);
                    AddMethodParameters(f, f.Parameters.Count - dc);
                    _sb.Append(";\n");
                }
            }
        }

        protected virtual void AddMethodIndexAttribute(DefFunction f)
        {
            _sb.AppendLine("[Implementation::MethodIndex( " + _methodIndices[f] + " )]");
        }

        protected virtual void AddMethodParameters(DefFunction f, int count)
        {
            _sb.Append("(");
            for (int i = 0; i < count; i++)
            {
                DefParam param = f.Parameters[i];

                _sb.Append(" " + GetCLRParamTypeName(param));
                _sb.Append(" " + param.Name);
                if (i < count - 1) _sb.Append(",");
            }
            _sb.Append(" )");
        }
        protected void AddMethodParameters(DefFunction f)
        {
            AddMethodParameters(f, f.Parameters.Count);
        }

        protected override void AddProperty(DefProperty p)
        {
            //TODO comments for properties
            //AddComments(p);
            string ptype = GetCLRTypeName(p);
            _sb.AppendFormatIndent("property {0} {1}\n{{\n", ptype, p.Name);
            if (p.CanRead)
            {
                DefFunction f = p.GetterFunction;
                bool methodIsVirtual = DeclareAsVirtual(f);

                if (p.GetterFunction.ProtectionType == ProtectionType.Public || (AllowProtectedMembers && p.GetterFunction.ProtectionType == ProtectionType.Protected) )
                {
                    _sb.AppendLine(GetProtectionString(p.GetterFunction.ProtectionType) + ":");

                    if (AllowMethodIndexAttributes && f.IsVirtual && !f.IsAbstract)
                    {
                        _sb.Append("\t");
                        AddMethodIndexAttribute(f);
                    }

                    _sb.AppendIndent("\t");
                    if (p.GetterFunction.IsStatic) _sb.Append("static ");
                    if (methodIsVirtual)
                        _sb.Append("virtual ");
                    _sb.Append(ptype + " get()");
                    if (DeclareAsOverride(p.GetterFunction))
                    {
                        _sb.Append(" override");
                    }
                    else if (f.IsAbstract && AllowSubclassing)
                    {
                        _sb.Append(" abstract");
                    }
                    _sb.Append(";\n");
                }
            }
            if (p.CanWrite)
            {
                DefFunction f = p.SetterFunction;
                bool methodIsVirtual = DeclareAsVirtual(f);

                if (p.SetterFunction.ProtectionType == ProtectionType.Public || (AllowProtectedMembers && p.SetterFunction.ProtectionType == ProtectionType.Protected) )
                {
                    _sb.AppendLine(GetProtectionString(p.SetterFunction.ProtectionType) + ":");

                    if (AllowMethodIndexAttributes && f.IsVirtual && !f.IsAbstract)
                    {
                        _sb.Append("\t");
                        AddMethodIndexAttribute(f);
                    }

                    _sb.AppendIndent("\t");
                    if (p.SetterFunction.IsStatic) _sb.Append("static ");
                    if (methodIsVirtual)
                        _sb.Append("virtual ");
                    _sb.Append("void set(" + ptype + " " + p.SetterFunction.Parameters[0].Name + ")");
                    if (DeclareAsOverride(p.SetterFunction))
                    {
                        _sb.Append(" override");
                    }
                    else if (f.IsAbstract && AllowSubclassing)
                    {
                        _sb.Append(" abstract");
                    }
                    _sb.Append(";\n");
                }
            }
            _sb.AppendLine("}");
        }

        protected override void AddPropertyField(DefField field)
        {
            //TODO comments for fields
            //AddComments(field);

            string ptype;

            if (field.IsNativeArray)
            {
                if (field.Type.HasAttribute<NativeValueContainerAttribute>()
                    || (field.Type.IsValueType && !field.Type.HasWrapType(WrapTypes.NativePtrValueType)))
                {
                    DefParam tmpParam = new DefParam(field, field.Name);
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

                    ptype = tmpParam.CLRTypeName;
                    _sb.AppendIndent("");
                    if (field.IsStatic) _sb.Append("static ");
                    _sb.AppendFormat("property {0} {1}\n", ptype, field.Name);
                    _sb.AppendLine("{");

                    _sb.AppendLine(GetProtectionString(field.ProtectionType) + ":");
                    _sb.AppendLine("\t" + ptype + " get();");

                    _sb.AppendLine("}");
                }
                else
                {
                    ptype = field.CLRTypeName;
                    _sb.AppendIndent("");
                    if (field.IsStatic) _sb.Append("static ");
                    _sb.AppendFormat("property {0} {1}[int]\n", ptype, field.Name);
                    _sb.AppendLine("{");

                    _sb.AppendLine(GetProtectionString(field.ProtectionType) + ":");
                    _sb.AppendLine("\t" + ptype + " get(int index);");
                    _sb.AppendLine("\tvoid set(int index, " + ptype + " value);");

                    _sb.AppendLine("}");
                }
            }
            else if (_cachedMembers.Contains(field))
            {
                ptype = field.CLRTypeName;
                _sb.AppendIndent("");
                if (field.IsStatic) _sb.Append("static ");
                _sb.AppendFormat("property {0} {1}\n", ptype, field.Name);
                _sb.AppendLine("{");

                _sb.AppendLine(GetProtectionString(field.ProtectionType) + ":");
                _sb.AppendLine("\t" + ptype + " get();");

                _sb.AppendLine("}");
            }
            else
            {
                ptype = GetCLRTypeName(field);
                _sb.AppendIndent("");
                if (field.IsStatic) _sb.Append("static ");
                if (field.HasAttribute<RenameAttribute> ()) {
                    _sb.AppendFormat ("property {0} {1}\n", ptype, field.GetAttribute<RenameAttribute> ().Name);
                }
                else {
                    _sb.AppendFormat ("property {0} {1}\n", ptype, field.Name);
                }
                _sb.AppendLine("{");

                _sb.AppendLine(GetProtectionString(field.ProtectionType) + ":");
                _sb.AppendLine("\t" + ptype + " get();");

                if ( // SharedPtrs can be copied by value. Let all be copied by value just to be sure (field.PassedByType == PassedByType.Pointer || field.Type.IsValueType)
                    !IsReadOnly && !field.Type.HasAttribute<ReadOnlyForFieldsAttribute>()
                    && !field.IsConst)
                {
                    _sb.AppendLine(GetProtectionString(field.ProtectionType) + ":");
                    _sb.AppendLine("\tvoid set(" + ptype + " value);");
                }

                _sb.AppendLine("}");
            }
        }

        protected override void AddMethodsForField(DefField field)
        {
            _sb.AppendLine(GetCLRTypeName(field) + " get_" + field.Name + "();");
            DefParam param = new DefParam(field, "value");
            _sb.AppendLine("void set_" + field.Name + "(" + param.Type.GetCLRParamTypeName(param) + " value);");
        }

        protected virtual void AddComments()
        {
            //TODO
        }
        protected virtual void AddComments(DefFunction f)
        {
            //TODO
        }

        protected override void AddPredefinedMethods(PredefinedMethods pm)
        {
            string clrType = _t.CLRName + (_t.IsValueType ? "" : "^");

            // For operator ==
            if ((PredefinedMethods.Equals & pm) != 0)
            {
                _sb.AppendLine("virtual bool Equals(Object^ obj) override;");
                _sb.AppendLine("bool Equals(" + clrType + " obj);");
                _sb.AppendLine("static bool operator == (" + clrType + " obj1, " + clrType + " obj2);");
                _sb.AppendLine("static bool operator != (" + clrType + " obj1, " + clrType + " obj2);");
            }

            // For operator =
            if ((PredefinedMethods.CopyTo & pm) != 0)
            {
                _sb.AppendLine("void CopyTo(" + clrType + " dest)");
                _sb.AppendLine("{");
                _sb.IncreaseIndent();
                _sb.AppendLine("if (_native == NULL) throw gcnew Exception(\"The underlying native object for the caller is null.\");");
                _sb.AppendLine("if (dest" + (_t.IsValueType ? "." : "->") + "_native == NULL) throw gcnew ArgumentException(\"The underlying native object for parameter 'dest' is null.\");");
                _sb.AppendLine();
                _sb.AppendLine("*(dest" + (_t.IsValueType ? "." : "->") + "_native) = *_native;");
                _sb.DecreaseIndent();
                _sb.AppendLine("}");
            }
        }
    }
}
