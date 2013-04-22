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
    abstract class ClassInclCodeProducer : ClassCodeProducer 
    {
        public ClassInclCodeProducer(MetaDefinition metaDef, Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(metaDef, wrapper, t, sb)
        {
            AddPreDeclarations();

            if (_definition.BaseClass != null)
                AddTypeDependancy(_definition.BaseClass);

            if (AllowSubclassing)
            {
                _wrapper.PreClassProducers.Add(new NativeProtectedTypesProxy(metaDef, _wrapper, _definition, _code));
                _wrapper.PostClassProducers.Add(new NativeProtectedStaticsProxy(metaDef, _wrapper, _definition, _code));
                //_wrapper.PreClassProducers.Add(new IncNativeProtectedTypesProxy(_wrapper, _t, _code));
            }
        }

        protected override void AddTypeDependancy(AbstractTypeDefinition type)
        {
            base.AddTypeDependancy(type);
            _wrapper.AddTypeDependancy(type);
        }

        protected virtual void AddPreDeclarations()
        {
            if (!_definition.IsNested)
            {
                _wrapper.AddPreDeclaration("ref class " + _definition.CLRName + ";");
                _wrapper.AddPragmaMakePublicForType(_definition);
            }
        }

        protected virtual void CheckTypeForDependancy(AbstractTypeDefinition type)
        {
            _wrapper.CheckTypeForDependancy(type);
        }

        protected virtual string GetCLRTypeName(ITypeMember m)
        {
            CheckTypeForDependancy(m.MemberType);
            return m.MemberTypeCLRName;
        }

        protected virtual string GetCLRParamTypeName(ParamDefinition param)
        {
            CheckTypeForDependancy(param.Type);
            return param.Type.GetCLRParamTypeName(param);
        }

        protected virtual void AddDefinition()
        {
            _code.AppendIndent("");
            if (!_definition.IsNested)
                _code.Append("public ");
            else
                _code.Append(_definition.ProtectionLevel.GetCLRProtectionName() + ": ");
            string baseclass = GetBaseAndInterfaces();
            if (baseclass != "")
                _code.AppendFormat("ref class {0}{1} : {2}\n", _definition.CLRName, (IsAbstractClass) ? " abstract" : "", baseclass);
            else
                _code.AppendFormat("ref class {0}{1}\n", _definition.CLRName, (IsAbstractClass) ? " abstract" : "");
        }

        protected override void AddInterfaceMethod(MemberMethodDefinition f)
        {
            _code.DecreaseIndent();
            _code.AppendLine(f.ProtectionLevel.GetCLRProtectionName() + ":");
            _code.IncreaseIndent();
            base.AddInterfaceMethod(f);
        }

        protected override void AddInterfaceMethodsForField(MemberFieldDefinition field)
        {
            _code.DecreaseIndent();
            _code.AppendLine(field.ProtectionLevel.GetCLRProtectionName() + ":");
            _code.IncreaseIndent();
            base.AddInterfaceMethodsForField(field);
        }

        protected override void AddPreBody()
        {
            base.AddPreBody();

            AddComments();
            AddDefinition();

            _code.AppendLine("{");
            _code.IncreaseIndent();
        }

        protected override void AddPostBody()
        {
            base.AddPostBody();

            _code.DecreaseIndent();
            _code.AppendLine("};\n");
        }

        protected override void AddPrivateDeclarations()
        {
            _code.DecreaseIndent();
            if (IsNativeClass)
                _code.AppendLine("private:");
            else
                _code.AppendLine("private protected:");
            _code.IncreaseIndent();
            base.AddPrivateDeclarations();

            AddCachedFields();

            if (_listeners.Count > 0)
            {
                _code.AppendLine("\n//Event and Listener fields");
                AddEventFields();
            }
        }

        protected override void AddStaticConstructor()
        {
            if (_definition.IsInterface)
                _code.AppendLine("static " + _definition.Name + "();");
            else
                _code.AppendLine("static " + _definition.CLRName + "();");
        }

        protected virtual void AddEventFields()
        {
            foreach (ClassDefinition cls in _listeners)
            {
                _code.AppendLine(GetNativeDirectorName(cls) + "* " + NameToPrivate(cls.Name) + ";");
                foreach (MemberMethodDefinition f in cls.PublicMethods)
                {
                    if (f.IsDeclarableFunction)
                    {
                        _code.AppendLine(cls.FullCLRName + "::" + f.CLRName + "Handler^ " + NameToPrivate(f.NativeName) + ";");
                    }
                    else
                        continue;

                    if (cls.HasAttribute<StopDelegationForReturnAttribute>())
                    {
                        _code.AppendLine("array<Delegate^>^ " + NameToPrivate(f.NativeName) + "Delegates;");
                    }
                }

                _code.AppendEmptyLine();
            }
        }

        protected virtual bool DoCleanupInFinalizer
        {
            get { return _definition.HasAttribute<DoCleanupInFinalizerAttribute>(); }
        }

        protected override void AddInternalDeclarations()
        {
            _code.DecreaseIndent();
            _code.AppendLine("public protected:");
            _code.IncreaseIndent();
            base.AddInternalDeclarations();

            AddInternalConstructors();

            if (RequiresCleanUp)
            {
                if (DoCleanupInFinalizer)
                {
                    _code.AppendLine("~" + _definition.CLRName + "()\n{");
                    _code.AppendLine("\tthis->!" + _definition.CLRName + "();");
                    _code.AppendLine("}");
                    _code.AppendLine("!" + _definition.CLRName + "()\n{");
                }
                else
                    _code.AppendLine("~" + _definition.CLRName + "()\n{");
                _code.IncreaseIndent();
                AddDisposerBody();
                _code.DecreaseIndent();
                _code.AppendLine("}\n");
            }

            foreach (ClassDefinition cls in _interfaces)
            {
                _code.AppendLine("virtual " + cls.FullNativeName + "* _" + cls.CLRName + "_GetNativePtr() = " + cls.CLRName + "::_GetNativePtr;");
                _code.AppendEmptyLine();
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
            if (_definition.HasAttribute<CustomDisposingAttribute>())
            {
                string text = _definition.GetAttribute<CustomDisposingAttribute>().Text;
                _code.AppendLine(text);
            }

            foreach (ClassDefinition cls in _listeners)
            {
                MemberMethodDefinition removerFunc = null;
                foreach (MemberMethodDefinition func in _definition.PublicMethods)
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
                _code.AppendLine(String.Format("if ({0} != 0)\n{{\n\tif (" + native + " != 0) " + GetNativeInvokationTarget(removerFunc) + "({0});\n\tdelete {0}; {0} = 0;\n}}", NameToPrivate(cls.Name)));
            }
        }

        protected override void AddPublicDeclarations()
        {
            _code.DecreaseIndent();
            _code.AppendLine("public:");
            _code.IncreaseIndent();

            if (IsConstructable)
            {
                AddPublicConstructors();
            }

            _code.AppendEmptyLine();
            AddPublicFields();
            _code.AppendEmptyLine();

            if (_listeners.Count > 0)
            {
                AddEventMethods();
            }
            base.AddPublicDeclarations();
        }

        protected override void AddPreNestedTypes()
        {
            base.AddPreNestedTypes();

            if( _definition.HasAttribute<CustomIncPreDeclarationAttribute>())
            {
                string txt = _definition.GetAttribute<CustomIncPreDeclarationAttribute>().DeclarationText;
                txt = ReplaceCustomVariables(txt);
                _code.AppendLine(txt);
                _code.AppendEmptyLine();
            }
        }

        protected override void AddPostNestedTypes()
        {
            base.AddPostNestedTypes();

            if (_definition.HasAttribute<CustomIncDeclarationAttribute>())
            {
                string txt = _definition.GetAttribute<CustomIncDeclarationAttribute>().DeclarationText;
                txt = ReplaceCustomVariables(txt);
                _code.AppendLine(txt);
                _code.AppendEmptyLine();
            }
        }

        protected virtual void AddPublicConstructors()
        {
            if (_definition.IsNativeAbstractClass && !_definition.IsInterface)
                return;

            if (_definition.Constructors.Length > 0)
            {
                foreach (MemberMethodDefinition func in _definition.Constructors)
                {
                    if (func.ProtectionLevel == ProtectionLevel.Public &&
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

        protected virtual void AddPublicConstructor(MemberMethodDefinition function)
        {
            string className = (_definition.IsInterface) ? _definition.Name : _definition.CLRName;

            if (function == null)
            {
                _code.AppendLine(className + "();");
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
                    if (dc < defcount && hideParams)
                        continue;


                    _code.AppendIndent(className);
                    AddMethodParameters(function, function.Parameters.Count - dc);
                    _code.Append(";\n");
                }
            }
        }

        protected virtual void AddPublicFields()
        {
        }

        protected virtual void AddEventInvokers()
        {
            foreach (ClassDefinition cls in _listeners)
            {
                foreach (MemberMethodDefinition f in cls.PublicMethods)
                {
                    if (f.IsDeclarableFunction)
                    {
                        _code.AppendIndent("virtual " + GetCLRTypeName(f) + " On" + f.CLRName);
                        AddMethodParameters(f);
                        _code.Append(" = " + GetNativeDirectorReceiverInterfaceName(cls) + "::" + f.CLRName + "\n");
                        _code.AppendLine("{");
                        _code.AppendIndent("\t");
                        if (f.MemberTypeName != "void")
                            _code.Append("return ");
                        _code.Append(f.CLRName + "(");
                        for (int i = 0; i < f.Parameters.Count; i++)
                        {
                            ParamDefinition param = f.Parameters[i];
                            _code.Append(" " + param.Name);
                            if (i < f.Parameters.Count - 1)
                                _code.Append(",");
                        }
                        _code.Append(" );\n");
                        _code.AppendLine("}\n");
                    }
                }

                _code.AppendEmptyLine();
            }
        }

        protected virtual void AddEventMethods()
        {
            foreach (ClassDefinition cls in _listeners)
            {
                MemberMethodDefinition adderFunc = null;
                foreach (MemberMethodDefinition func in _definition.PublicMethods)
                {
                    if (func.IsListenerAdder && func.Parameters[0].Type == cls)
                    {
                        adderFunc = func;
                        break;
                    }
                }
                if (adderFunc == null)
                    throw new Exception("Unexpected");

                foreach (MemberMethodDefinition f in cls.PublicMethods)
                {
                    if (f.IsDeclarableFunction)
                    {
                        string handler = cls.FullCLRName + "::" + f.CLRName + "Handler^";
                        string privField = NameToPrivate(f.NativeName);
                        string listener = NameToPrivate(cls.Name);
                        _code.AppendLine("event " + handler + " " + f.CLRName);
                        _code.AppendLine("{");
                        _code.IncreaseIndent();
                        _code.AppendLine("void add(" + handler + " hnd)");
                        _code.AppendLine("{");
                        _code.IncreaseIndent();
                        _code.AppendLine("if (" + privField + " == CLR_NULL)");
                        _code.AppendLine("{");
                        _code.IncreaseIndent();
                        _code.AppendLine("if (" + listener + " == 0)");
                        _code.AppendLine("{");
                        _code.AppendLine("\t" + listener + " = new " + GetNativeDirectorName(cls) + "(this);");
                        _code.AppendLine("\t" + GetNativeInvokationTarget(adderFunc) + "(" + listener + ");");
                        _code.AppendLine("}");
                        _code.AppendLine(listener + "->doCallFor" + f.CLRName + " = true;");
                        _code.DecreaseIndent();
                        _code.AppendLine("}");
                        _code.AppendLine(privField + " += hnd;");

                        if (cls.HasAttribute<StopDelegationForReturnAttribute>())
                        {
                            _code.AppendLine(privField + "Delegates = " + privField + "->GetInvocationList();");
                        }

                        _code.DecreaseIndent();
                        _code.AppendLine("}");
                        _code.AppendLine("void remove(" + handler + " hnd)");
                        _code.AppendLine("{");
                        _code.AppendLine("\t" + privField + " -= hnd;");
                        _code.AppendLine("\tif (" + privField + " == CLR_NULL) " + listener + "->doCallFor" + f.CLRName + " = false;");

                        if (cls.HasAttribute<StopDelegationForReturnAttribute>())
                        {
                            _code.AppendLine("\tif (" + privField + " == CLR_NULL) " + privField + "Delegates = nullptr; else " + privField + "Delegates = " + privField + "->GetInvocationList();");
                        }

                        _code.AppendLine("}");
                        _code.DecreaseIndent();
                        _code.AppendLine("private:");
                        _code.IncreaseIndent();
                        _code.AppendIndent(GetCLRTypeName(f) + " raise");
                        AddMethodParameters(f);
                        _code.Append("\n");
                        _code.AppendLine("{");

                        if (cls.HasAttribute<StopDelegationForReturnAttribute>())
                        {
                            _code.IncreaseIndent();
                            _code.AppendLine("if (" + privField + ")");
                            _code.AppendLine("{");
                            _code.IncreaseIndent();
                            string list = privField + "Delegates";
                            string stopret = cls.GetAttribute<StopDelegationForReturnAttribute>().Return;
                            _code.AppendLine(f.MemberType.FullCLRName + " mp_return;");
                            _code.AppendLine("for (int i=0; i < " + list + "->Length; i++)");
                            _code.AppendLine("{");
                            _code.IncreaseIndent();
                            _code.AppendIndent("mp_return = " + "static_cast<" + handler + ">(" + list + "[i])(");
                            for (int i = 0; i < f.Parameters.Count; i++)
                            {
                                ParamDefinition param = f.Parameters[i];
                                _code.Append(" " + param.Name);
                                if (i < f.Parameters.Count - 1)
                                    _code.Append(",");
                            }
                            _code.Append(" );\n");
                            _code.AppendLine("if (mp_return == " + stopret + ") break;");
                            _code.DecreaseIndent();
                            _code.AppendLine("}");
                            _code.AppendLine("return mp_return;");
                            _code.DecreaseIndent();
                            _code.AppendLine("}");
                            _code.DecreaseIndent();
                        }
                        else
                        {
                            _code.AppendLine("\tif (" + privField + ")");
                            _code.AppendIndent("\t\t");
                            if (f.MemberTypeName != "void")
                                _code.Append("return ");
                            _code.Append(privField + "->Invoke(");
                            for (int i = 0; i < f.Parameters.Count; i++)
                            {
                                ParamDefinition param = f.Parameters[i];
                                _code.Append(" " + param.Name);
                                if (i < f.Parameters.Count - 1)
                                    _code.Append(",");
                            }
                            _code.Append(" );\n");
                        }

                        _code.AppendLine("}");
                        _code.DecreaseIndent();
                        _code.AppendLine("}\n");
                    }
                }
                _code.AppendEmptyLine();
            }
        }

        protected override void AddProtectedDeclarations()
        {
            _code.DecreaseIndent();
            _code.AppendLine("protected public:");
            _code.IncreaseIndent();
            base.AddProtectedDeclarations();

            if (_listeners.Count > 0)
            {
                AddEventInvokers();
            }
        }

        protected override void AddStaticField(MemberFieldDefinition field)
        {
            base.AddStaticField(field);
            _code.AppendIndent("");
            if (field.IsConst)
                _code.Append("const ");
            if (field.IsStatic)
                _code.Append("static ");
            _code.Append(GetCLRTypeName(field) + " " + field.NativeName + " = " + field.MemberType.ProduceNativeCallConversionCode(field.FullNativeName, field) + ";\n\n");
        }

        protected override void AddNestedTypeBeforeMainType(AbstractTypeDefinition nested)
        {
            base.AddNestedType(nested);
            _wrapper.IncAddType(nested, _code);
        }

        protected override void AddAllNestedTypes()
        {
            //Predeclare all nested classes in case there are classes referencing their "siblings"
            foreach (AbstractTypeDefinition nested in _definition.NestedTypes)
            {
                if (nested.ProtectionLevel == ProtectionLevel.Public
                    || ((AllowProtectedMembers || AllowSubclassing) && nested.ProtectionLevel == ProtectionLevel.Protected))
                {
                    AbstractTypeDefinition expl = _definition.FindType<AbstractTypeDefinition>(nested.Name);

                    if (expl.IsSTLContainer
                        || (!nested.IsValueType && nested is ClassDefinition && !(nested as ClassDefinition).IsInterface && _wrapper.TypeIsWrappable(nested)))
                    {
                        _code.AppendLine(nested.ProtectionLevel.GetCLRProtectionName() + ": ref class " + nested.CLRName + ";");
                    }
                }
            }

            _code.AppendEmptyLine();

            base.AddAllNestedTypes();
        }

        protected override void AddNestedType(AbstractTypeDefinition nested)
        {
            if (nested.HasWrapType(WrapTypes.NativeDirector))
            {
                //Interface and native director are already declared before the declaration of this class.
                //Just declare the method handlers of the class.
                IncNativeDirectorClassProducer.AddMethodHandlersClass((ClassDefinition) nested, _code);
                return;
            }

            base.AddNestedType(nested);
            _wrapper.IncAddType(nested, _code);
        }

        protected virtual void AddCachedFields()
        {
            if (_cachedMembers.Count > 0)
            {
                _code.AppendLine("//Cached fields");
                foreach (MemberDefinitionBase m in _cachedMembers)
                {
                    _code.AppendIndent("");
                    if (m.IsStatic)
                    {
                        _code.Append("static ");
                        _wrapper.UsedTypes.Add(m.MemberType);
                    }

                    _code.Append(m.MemberTypeCLRName + " " + NameToPrivate(m) + ";\n");
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
                if (baseclass != "")
                    baseclass += ", ";

                foreach (ClassDefinition it in _interfaces)
                {
                    AddTypeDependancy(it);
                    string itname = it.CLRName;
                    if (it.IsNested)
                        itname = it.SurroundingClass.FullCLRName + "::" + itname;
                    baseclass += "public " + itname + ", ";
                }
                baseclass = baseclass.Substring(0, baseclass.Length - ", ".Length);
            }

            if (_listeners.Count > 0)
            {
                if (baseclass != "")
                    baseclass += ", ";

                foreach (ClassDefinition it in _listeners)
                {
                    AddTypeDependancy(it);
                    baseclass += "public " + GetNativeDirectorReceiverInterfaceName(it) + ", ";
                }
                baseclass = baseclass.Substring(0, baseclass.Length - ", ".Length);
            }

            return baseclass;
        }

        protected override void AddMethod(MemberMethodDefinition f)
        {
            if (f.HasAttribute<CustomIncDeclarationAttribute>())
            {
                string txt = f.GetAttribute<CustomIncDeclarationAttribute>().DeclarationText;
                txt = ReplaceCustomVariables(txt, f);
                _code.AppendLine(txt);
                _code.AppendEmptyLine();
                return;
            }

            int defcount = 0;

            if (!f.HasAttribute<NoDefaultParamOverloadsAttribute>())
            {
                foreach (ParamDefinition param in f.Parameters)
                    if (param.DefaultValue != null)
                        defcount++;
            }

            bool methodIsVirtual = DeclareAsVirtual(f);

            // The main method
            AddComments(f);

            if (AllowMethodIndexAttributes && f.IsVirtual && !f.IsAbstract)
                AddMethodIndexAttribute(f);

            _code.AppendIndent("");
            if (f.IsStatic)
                _code.Append("static ");
            if (methodIsVirtual)
                _code.Append("virtual ");
            _code.Append(GetCLRTypeName(f) + " " + f.CLRName);
            AddMethodParameters(f, f.Parameters.Count);
            if (DeclareAsOverride(f))
            {
                _code.Append(" override");
            }
            else if (f.IsAbstract && AllowSubclassing)
            {
                _code.Append(" abstract");
            }

            _code.Append(";\n");

            if (AllowMethodOverloads)
            {
                // The overloads (because of default values)
                for (int dc = 1; dc <= defcount; dc++)
                {
                    if (dc < defcount && f.HasAttribute<HideParamsWithDefaultValuesAttribute>())
                        continue;

                    AddComments(f);
                    _code.AppendIndent("");
                    if (f.IsStatic)
                        _code.Append("static ");
                    _code.Append(GetCLRTypeName(f) + " " + f.CLRName);
                    AddMethodParameters(f, f.Parameters.Count - dc);
                    _code.Append(";\n");
                }
            }
        }

        protected virtual void AddMethodIndexAttribute(MemberMethodDefinition f)
        {
            _code.AppendLine("[Implementation::MethodIndex( " + _methodIndices[f] + " )]");
        }

        protected virtual void AddMethodParameters(MemberMethodDefinition f, int count)
        {
            _code.Append("(");
            for (int i = 0; i < count; i++)
            {
                ParamDefinition param = f.Parameters[i];

                _code.Append(" " + GetCLRParamTypeName(param));
                _code.Append(" " + param.Name);
                if (i < count - 1)
                    _code.Append(",");
            }
            _code.Append(" )");
        }

        protected void AddMethodParameters(MemberMethodDefinition f)
        {
            AddMethodParameters(f, f.Parameters.Count);
        }

        protected override void AddProperty(MemberPropertyDefinition p)
        {
            //TODO comments for properties
            //AddComments(p);
            string ptype = GetCLRTypeName(p);
            _code.AppendFormatIndent("property {0} {1}\n{{\n", ptype, p.Name);
            if (p.CanRead)
            {
                MemberMethodDefinition f = p.GetterFunction;
                bool methodIsVirtual = DeclareAsVirtual(f);

                if (p.GetterFunction.ProtectionLevel == ProtectionLevel.Public || (AllowProtectedMembers && p.GetterFunction.ProtectionLevel == ProtectionLevel.Protected))
                {
                    _code.AppendLine(p.GetterFunction.ProtectionLevel.GetCLRProtectionName() + ":");

                    if (AllowMethodIndexAttributes && f.IsVirtual && !f.IsAbstract)
                    {
                        _code.Append("\t");
                        AddMethodIndexAttribute(f);
                    }

                    _code.AppendIndent("\t");
                    if (p.GetterFunction.IsStatic)
                        _code.Append("static ");
                    if (methodIsVirtual)
                        _code.Append("virtual ");
                    _code.Append(ptype + " get()");
                    if (DeclareAsOverride(p.GetterFunction))
                    {
                        _code.Append(" override");
                    }
                    else if (f.IsAbstract && AllowSubclassing)
                    {
                        _code.Append(" abstract");
                    }
                    _code.Append(";\n");
                }
            }
            if (p.CanWrite)
            {
                MemberMethodDefinition f = p.SetterFunction;
                bool methodIsVirtual = DeclareAsVirtual(f);

                if (p.SetterFunction.ProtectionLevel == ProtectionLevel.Public || (AllowProtectedMembers && p.SetterFunction.ProtectionLevel == ProtectionLevel.Protected))
                {
                    _code.AppendLine(p.SetterFunction.ProtectionLevel.GetCLRProtectionName() + ":");

                    if (AllowMethodIndexAttributes && f.IsVirtual && !f.IsAbstract)
                    {
                        _code.Append("\t");
                        AddMethodIndexAttribute(f);
                    }

                    _code.AppendIndent("\t");
                    if (p.SetterFunction.IsStatic)
                        _code.Append("static ");
                    if (methodIsVirtual)
                        _code.Append("virtual ");
                    _code.Append("void set(" + ptype + " " + p.SetterFunction.Parameters[0].Name + ")");
                    if (DeclareAsOverride(p.SetterFunction))
                    {
                        _code.Append(" override");
                    }
                    else if (f.IsAbstract && AllowSubclassing)
                    {
                        _code.Append(" abstract");
                    }
                    _code.Append(";\n");
                }
            }
            _code.AppendLine("}");
        }

        protected override void AddPropertyField(MemberFieldDefinition field)
        {
            //TODO comments for fields
            //AddComments(field);

            string ptype;

            if (field.IsNativeArray)
            {
                if (field.MemberType.HasAttribute<NativeValueContainerAttribute>()
                    || (field.MemberType.IsValueType && !field.MemberType.HasWrapType(WrapTypes.NativePtrValueType)))
                {
                    ParamDefinition tmpParam = new ParamDefinition(this.MetaDef, field, field.NativeName);
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

                    ptype = tmpParam.MemberTypeCLRName;
                    _code.AppendIndent("");
                    if (field.IsStatic)
                        _code.Append("static ");
                    _code.AppendFormat("property {0} {1}\n", ptype, field.NativeName);
                    _code.AppendLine("{");

                    _code.AppendLine(field.ProtectionLevel.GetCLRProtectionName() + ":");
                    _code.AppendLine("\t" + ptype + " get();");

                    _code.AppendLine("}");
                }
                else
                {
                    ptype = field.MemberTypeCLRName;
                    _code.AppendIndent("");
                    if (field.IsStatic)
                        _code.Append("static ");
                    _code.AppendFormat("property {0} {1}[int]\n", ptype, field.NativeName);
                    _code.AppendLine("{");

                    _code.AppendLine(field.ProtectionLevel.GetCLRProtectionName() + ":");
                    _code.AppendLine("\t" + ptype + " get(int index);");
                    _code.AppendLine("\tvoid set(int index, " + ptype + " value);");

                    _code.AppendLine("}");
                }
            }
            else if (_cachedMembers.Contains(field))
            {
                ptype = field.MemberTypeCLRName;
                _code.AppendIndent("");
                if (field.IsStatic)
                    _code.Append("static ");
                _code.AppendFormat("property {0} {1}\n", ptype, field.NativeName);
                _code.AppendLine("{");

                _code.AppendLine(field.ProtectionLevel.GetCLRProtectionName() + ":");
                _code.AppendLine("\t" + ptype + " get();");

                _code.AppendLine("}");
            }
            else
            {
                ptype = GetCLRTypeName(field);
                _code.AppendIndent("");
                if (field.IsStatic)
                    _code.Append("static ");
                if (field.HasAttribute<RenameAttribute>())
                {
                    _code.AppendFormat("property {0} {1}\n", ptype, field.GetAttribute<RenameAttribute>().Name);
                }
                else
                {
                    _code.AppendFormat("property {0} {1}\n", ptype, field.NativeName);
                }
                _code.AppendLine("{");

                _code.AppendLine(field.ProtectionLevel.GetCLRProtectionName() + ":");
                _code.AppendLine("\t" + ptype + " get();");

                if ( // SharedPtrs can be copied by value. Let all be copied by value just to be sure (field.PassedByType == PassedByType.Pointer || field.Type.IsValueType)
                   !IsReadOnly && !field.MemberType.HasAttribute<ReadOnlyForFieldsAttribute>()
                    && !field.IsConst)
                {
                    _code.AppendLine(field.ProtectionLevel.GetCLRProtectionName() + ":");
                    _code.AppendLine("\tvoid set(" + ptype + " value);");
                }

                _code.AppendLine("}");
            }
        }

        protected override void AddMethodsForField(MemberFieldDefinition field)
        {
            _code.AppendLine(GetCLRTypeName(field) + " get_" + field.NativeName + "();");
            ParamDefinition param = new ParamDefinition(this.MetaDef, field, "value");
            _code.AppendLine("void set_" + field.NativeName + "(" + param.Type.GetCLRParamTypeName(param) + " value);");
        }

        protected virtual void AddComments()
        {
            //TODO
        }

        protected virtual void AddComments(MemberMethodDefinition f)
        {
            //TODO
        }

        protected override void AddPredefinedMethods(PredefinedMethods pm)
        {
            string clrType = _definition.CLRName + (_definition.IsValueType ? "" : "^");

            // For operator ==
            if ((PredefinedMethods.Equals & pm) != 0)
            {
                _code.AppendLine("virtual bool Equals(Object^ obj) override;");
                _code.AppendLine("bool Equals(" + clrType + " obj);");
                _code.AppendLine("static bool operator == (" + clrType + " obj1, " + clrType + " obj2);");
                _code.AppendLine("static bool operator != (" + clrType + " obj1, " + clrType + " obj2);");
            }

            // For operator =
            if ((PredefinedMethods.CopyTo & pm) != 0)
            {
                _code.AppendLine("void CopyTo(" + clrType + " dest)");
                _code.AppendLine("{");
                _code.IncreaseIndent();
                _code.AppendLine("if (_native == NULL) throw gcnew Exception(\"The underlying native object for the caller is null.\");");
                _code.AppendLine("if (dest" + (_definition.IsValueType ? "." : "->") + "_native == NULL) throw gcnew ArgumentException(\"The underlying native object for parameter 'dest' is null.\");");
                _code.AppendEmptyLine();
                _code.AppendLine("*(dest" + (_definition.IsValueType ? "." : "->") + "_native) = *_native;");
                _code.DecreaseIndent();
                _code.AppendLine("}");
            }
        }
    }
}
