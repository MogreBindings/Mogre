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
    public class ClassCodeProducer : AbstractCodeProducer 
    {
        protected readonly Wrapper _wrapper;
        protected readonly ClassDefinition _definition;
        protected SourceCodeStringBuilder _code;
        protected readonly List<ClassDefinition> _listeners = new List<ClassDefinition>();
        protected readonly List<PropertyDefinition> _interfaceProperties = new List<PropertyDefinition>();
        protected readonly List<MemberMethodDefinition> _abstractFunctions = new List<MemberMethodDefinition>();
        protected readonly List<PropertyDefinition> _abstractProperties = new List<PropertyDefinition>();
        protected readonly List<ClassDefinition> _interfaces = new List<ClassDefinition>();

        protected readonly List<MemberMethodDefinition> _overridableFunctions = new List<MemberMethodDefinition>();
        protected PropertyDefinition[] _overridableProperties;
        //protected List<DefField> _protectedFields = new List<DefField>();
        protected readonly Dictionary<MemberMethodDefinition, int> _methodIndices = new Dictionary<MemberMethodDefinition, int>();
        protected int _methodIndicesCount = 0;

        protected readonly List<AbstractMemberDefinition> _cachedMembers = new List<AbstractMemberDefinition>();

        public ClassCodeProducer(Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
        {
            this._wrapper = wrapper;
            this._definition = t;
            this._code = sb;

            foreach (ClassDefinition iface in _definition.GetInterfaces())
            {
                AddTypeDependancy(iface);
                _interfaces.Add(iface);
            }

            if (_definition.IsInterface)
            {
                // Declaring an overridable class for interface
                _interfaces.Add(_definition);
            }
        }

        private bool _initCalled = false;
        protected virtual void Init()
        {
            if (_initCalled)
                return;

            _initCalled = true;

            foreach (MemberMethodDefinition f in _definition.PublicMethods)
            {
                if (f.IsListenerAdder && !f.IsIgnored)
                {
                    _listeners.Add((ClassDefinition)f.Parameters[0].Type);
                }
            }

            foreach (ClassDefinition iface in _definition.GetInterfaces())
            {
                // Add attributes of interface methods from the interface classes
                foreach (MemberMethodDefinition f in iface.Functions)
                {
                    MemberMethodDefinition tf = _definition.GetFunctionWithSignature(f.Signature);
                    if (tf != null)
                        tf.Attributes.AddRange(f.Attributes);
                }

                //Store properties of interface classes. They have precedence over type's properties.
                foreach (PropertyDefinition ip in iface.GetProperties())
                {
                    if (IsPropertyAllowed(ip) &&
                        (ip.ProtectionLevel == ProtectionLevel.Public
                          || (AllowProtectedMembers && ip.ProtectionLevel == ProtectionLevel.Protected)))
                    {
                        _interfaceProperties.Add(ip);
                    }
                }
            }

            foreach (MemberFieldDefinition field in _definition.Fields)
            {
                if (!field.IsIgnored && field.Type.IsSTLContainer)
                {
                    if (field.ProtectionType == ProtectionLevel.Public
                        || ( (AllowSubclassing || AllowProtectedMembers) && field.ProtectionType == ProtectionLevel.Protected))
                        MarkCachedMember(field);
                }
            }

            foreach (ClassDefinition iface in _interfaces)
            {
                if (iface == _definition)
                    continue;

                foreach (MemberFieldDefinition field in iface.Fields)
                {
                    if (!field.IsIgnored && field.Type.IsSTLContainer
                        && !field.IsStatic)
                    {
                        if (field.ProtectionType == ProtectionLevel.Public
                            || ((AllowSubclassing || AllowProtectedMembers) && field.ProtectionType == ProtectionLevel.Protected))
                            MarkCachedMember(field);
                    }
                }
            }

            foreach (MemberMethodDefinition func in _definition.AbstractFunctions)
            {
                if (func.ProtectionType == ProtectionLevel.Public
                        || (AllowProtectedMembers && func.ProtectionType == ProtectionLevel.Protected))
                {
                    if ((func.Class.AllowSubClassing || (func.Class == _definition && AllowSubclassing)) && !func.IsProperty)
                    {
                        _isAbstractClass = true;
                        _abstractFunctions.Add(func);
                    }
                }
            }

            foreach (PropertyDefinition prop in _definition.AbstractProperties)
            {
                if (IsPropertyAllowed(prop) && (prop.ContainingClass.AllowSubClassing
                    || (prop.ContainingClass == _definition && AllowSubclassing)))
                {
                    if (prop.ProtectionLevel == ProtectionLevel.Public
                        || (AllowProtectedMembers && prop.ProtectionLevel == ProtectionLevel.Protected))
                    {

                        _isAbstractClass = true;
                        _abstractProperties.Add(prop);
                    }
                }
            }

            SearchOverridableFunctions(_definition);
            //SearchProtectedFields(_t);

            foreach (ClassDefinition iface in _interfaces)
            {
                SearchOverridableFunctions(iface);
                //SearchProtectedFields(iface);
            }

            _overridableProperties = GetPropertiesFromFunctions(_overridableFunctions);

            //Find cached members

            foreach (AbstractMemberDefinition m in _definition.Members)
            {
                if (m.HasAttribute<CachedAttribute>())
                    MarkCachedMember(m);
            }

            foreach (ClassDefinition iface in _interfaces)
            {
                if (iface == _definition)
                    continue;

                foreach (AbstractMemberDefinition m in iface.Members)
                {
                    if (m.HasAttribute<CachedAttribute>())
                        MarkCachedMember(m);
                }
            }
        }

        /// <summary>
        /// Converts the specified methods into CLR properties.
        /// </summary>
        /// <param name="funcs">The methods to convert. Must only contain getter and setter
        /// methods.</param>
        public static PropertyDefinition[] GetPropertiesFromFunctions(List<MemberMethodDefinition> funcs)
        {
            SortedList<string, PropertyDefinition> props = new SortedList<string, PropertyDefinition>();

            foreach (MemberMethodDefinition f in funcs)
            {
                if (f.IsProperty && f.IsDeclarableFunction)
                {
                    PropertyDefinition p = null;

                    if (props.ContainsKey(f.CLRName))
                        p = props[f.CLRName];
                    else
                    {
                        p = new PropertyDefinition(f.CLRName);
                        if (f.IsGetProperty)
                        {
                            p.MemberTypeName = f.TypeName;
                            p.PassedByType = f.PassedByType;
                        }
                        else
                        {
                            p.MemberTypeName = f.Parameters[0].TypeName;
                            p.PassedByType = f.Parameters[0].PassedByType;
                        }

                        props.Add(f.CLRName, p);
                    }

                    if (f.IsGetProperty)
                    {
                        p.GetterFunction = f;
                    }
                    else if (f.IsSetProperty)
                    {
                        p.SetterFunction = f;
                    }
                }
            }

            PropertyDefinition[] parr = new PropertyDefinition[props.Count];
            for (int i = 0; i < props.Count; i++)
                parr[i] = props.Values[i];

            return parr;
        }

        public virtual string ClassFullNativeName
        {
            get { return _definition.FullNativeName; }
        }

        public virtual bool IsNativeClass
        {
            get { return false; }
        }

        private bool _isAbstractClass;
        public bool IsAbstractClass
        {
            get { return _isAbstractClass; }
        }

        protected virtual bool IsConstructable
        {
            get { return !_definition.HasAttribute<NotConstructableAttribute>(); }
        }

        protected virtual bool AllowVirtualMethods
        {
            get { return _definition.AllowSubClassing; }
        }

        protected virtual bool AllowProtectedMembers
        {
            get { return false; }
        }

        protected virtual bool AllowSubclassing
        {
            get { return _definition.AllowSubClassing; }
        }

        protected virtual bool AllowMethodOverloads
        {
            get { return true; }
        }

        protected virtual bool AllowMethodIndexAttributes
        {
            get { return false; }
        }

        protected virtual bool AllowCachedMemberFields
        {
            get { return !_definition.HasWrapType(WrapTypes.NativePtrValueType) && !_definition.HasWrapType(WrapTypes.ValueType); }
        }

        protected virtual bool IsReadOnly
        {
            get { return _definition.HasAttribute<ReadOnlyAttribute>(); }
        }

        protected virtual bool DeclareAsVirtual(MemberMethodDefinition f)
        {
            return (f.IsVirtual && AllowVirtualMethods) || f.IsVirtualInterfaceMethod
                || (f.IsVirtual && f.BaseFunction != null && f.BaseFunction.Class.AllowVirtuals);
        }

        protected virtual bool DeclareAsOverride(MemberMethodDefinition f)
        {
            return (f.IsOverride && DeclareAsVirtual(f))
                || (f.IsVirtualInterfaceMethod && !f.Class.IsInterface && !f.Class.ContainsInterfaceFunctionSignature(f.Signature, false));
        }

        protected ClassDefinition GetTopClass(ClassDefinition type)
        {
            if (type.BaseClass == null)
                return type;
            else
                return GetTopClass(type.BaseClass);
        }

        protected virtual void MarkCachedMember(AbstractMemberDefinition m)
        {
            if (m.IsStatic || AllowCachedMemberFields)
                _cachedMembers.Add(m);
        }

        protected virtual string GetNativeInvokationTarget(bool isConst)
        {
            string ret = "static_cast<";
            if (isConst)
                ret += "const ";
            return ret + ClassFullNativeName + "*>(_native)";
        }
        protected string GetNativeInvokationTarget()
        {
            return GetNativeInvokationTarget(false);
        }

        protected virtual string GetNativeInvokationTarget(MemberMethodDefinition f)
        {
            if (!f.IsStatic)
            {
                if (f.ProtectionType == ProtectionLevel.Public)
                {
                    return GetNativeInvokationTarget(f.IsConstFunctionCall) + "->" + f.Name;
                }
                else if (f.ProtectionType == ProtectionLevel.Protected)
                {
                    if (!f.IsVirtual)
                    {
                        string proxyName = NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(_definition);
                        return "static_cast<" + proxyName + "*>(_native)->" + f.Name;
                    }
                    else
                        throw new Exception("Unexpected");
                }
                else
                    throw new Exception("Unexpected");
            }
            else
            {
                if (f.ProtectionType == ProtectionLevel.Public)
                    return f.Class.FullNativeName + "::" + f.Name;
                else
                    return NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(f.Class) + "::" + f.Name;
            }
        }
        protected virtual string GetNativeInvokationTarget(MemberFieldDefinition field)
        {
            if (!field.IsStatic)
            {
                if (field.ProtectionType == ProtectionLevel.Public)
                {
                    return GetNativeInvokationTarget() + "->" + field.Name;
                }
                else if (field.ProtectionType == ProtectionLevel.Protected)
                {
                    string proxyName = NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(_definition);
                    return "static_cast<" + proxyName + "*>(_native)->" + field.Name;
                }
                else
                    throw new Exception("Unexpected");
            }
            else
            {
                if (field.ProtectionType == ProtectionLevel.Public)
                    return field.Class.FullNativeName + "::" + field.Name;
                else
                    return NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(field.Class) + "::" + field.Name;
            }
        }

        protected virtual string GetNativeInvokationTargetObject()
        {
            return "*(static_cast<" + ClassFullNativeName + "*>(_native))";
        }

        protected virtual void SearchOverridableFunctions(ClassDefinition type)
        {
            foreach (MemberMethodDefinition func in type.Functions)
            {
                if (func.IsDeclarableFunction && func.IsVirtual)
                {
                    if (!ContainsFunction(func, _overridableFunctions))
                    {
                        _overridableFunctions.Add(func);

                        if (!func.IsAbstract)
                        {
                            _methodIndices.Add(func, _methodIndicesCount);
                            _methodIndicesCount++;
                        }
                    }
                }
            }

            foreach (ClassDefinition iface in type.GetInterfaces())
                SearchOverridableFunctions(iface);

            if (type.BaseClass != null)
                SearchOverridableFunctions(type.BaseClass);
        }

        protected bool ContainsFunction(MemberMethodDefinition func, List<MemberMethodDefinition> list)
        {
            foreach (MemberMethodDefinition lf in list)
            {
                if (lf.Signature == func.Signature)
                    return true;
            }

            return false;
        }

        //protected virtual void SearchProtectedFields(DefClass type)
        //{
        //    foreach (DefField field in type.Fields)
        //    {
        //        if (!field.IsIgnored
        //            && field.ProtectionType == ProtectionType.Protected
        //            && !field.IsStatic
        //            && !ContainsField(field, _protectedFields))
        //        {
        //            _protectedFields.Add(field);
        //        }
        //    }

        //    foreach (DefClass iface in type.GetInterfaces())
        //        SearchProtectedFields(iface);

        //    if (type.BaseClass != null)
        //        SearchProtectedFields(type.BaseClass);
        //}

        //protected bool ContainsField(DefField field, List<DefField> list)
        //{
        //    foreach (DefField lf in list)
        //    {
        //        if (lf.Name == field.Name)
        //            return true;
        //    }

        //    return false;
        //}

        public virtual void Add()
        {
            Init();

            AddPreBody();
            AddBody();
            AddPostBody();
        }

        public virtual void AddFirst()
        {
            SourceCodeStringBuilder orig = _code;
            _code = new SourceCodeStringBuilder();

            Add();

            orig.InsertAt(0, _code.ToString());
            _code = orig;
        }

        protected virtual string ReplaceCustomVariables(string txt)
        {
            string targ = "static_cast<" + ClassFullNativeName + "*>(_native)";
            txt = txt.Replace("@NATIVE@", targ);
            txt = txt.Replace("@CLASS@", GetClassName());
            return txt;
        }

        protected virtual string ReplaceCustomVariables(string txt, MemberMethodDefinition func)
        {
            txt = ReplaceCustomVariables(txt);
            string replace;
            if (DeclareAsOverride(func))
                replace = "override";
            else if (func.IsAbstract && AllowSubclassing)
                replace = "abstract";
            else
                replace = "";
            txt = txt.Replace("@OVERRIDE@", replace);

            txt = txt.Replace("@NATIVE_INVOKATION_TARGET_FOR_FUNCTION@", GetNativeInvokationTarget(func));
            return txt;
        }

        protected virtual string GetClassName()
        {
            string full = _definition.FullCLRName;
            int index = full.IndexOf("::");
            return full.Substring(index + 2);
        }

        protected virtual string GetTopBaseClassName()
        {
            return null;
        }

        protected virtual string GetBaseClassName()
        {
            return (_definition.BaseClass == null || _definition.BaseClass.HasWrapType(WrapTypes.NativeDirector)) ? GetTopBaseClassName() : _definition.BaseClass.Name;
        }

        protected bool HasStaticCachedFields()
        {
            foreach (AbstractMemberDefinition m in _cachedMembers)
            {
                if (m.IsStatic)
                    return true;
            }

            return false;
        }

        protected virtual void AddPreBody()
        {
            // If there are nested NativeDirectors, declare them before the declaration
            // of this class
            foreach (AbstractTypeDefinition nested in _definition.NestedTypes)
            {
                if (nested.ProtectionLevel == ProtectionLevel.Public
                    || ((AllowProtectedMembers || AllowSubclassing) && nested.ProtectionLevel == ProtectionLevel.Protected))
                {
                    if (nested.HasWrapType(WrapTypes.NativeDirector))
                        AddNestedTypeBeforeMainType(nested);
                }
            }

            _code.AppendLine("//################################################################");
            _code.AppendLine("//" + _definition.CLRName);
            _code.AppendLine("//################################################################\n");
        }

        protected virtual void AddNestedTypeBeforeMainType(AbstractTypeDefinition nested)
        {
        }

        protected virtual void AddBody()
        {
            AddPreNestedTypes();
            _code.AppendLine("//Nested Types");
            AddAllNestedTypes();
            AddPostNestedTypes();
            _code.AppendLine("//Private Declarations");
            AddPrivateDeclarations();
            _code.Append("\n");
            _code.AppendLine("//Internal Declarations");
            AddInternalDeclarations();
            _code.Append("\n");
            _code.AppendLine("//Public Declarations");
            AddPublicDeclarations();
            _code.Append("\n");
            _code.AppendLine("//Protected Declarations");
            AddProtectedDeclarations();
            _code.Append("\n");
        }

        protected virtual void AddPostBody()
        {
        }

        protected virtual bool IsConstructorBodyEmpty
        {
            get { return (_interfaces.Count == 0); }
        }

        protected virtual void AddConstructorBody()
        {
        }

        protected virtual void AddPrivateDeclarations()
        {
            if (HasStaticCachedFields())
            {
                AddStaticConstructor();
                _code.AppendEmptyLine();
            }
        }

        protected virtual void AddStaticConstructor()
        {
        }

        protected virtual void AddInternalDeclarations()
        {
        }

        protected virtual void AddPreNestedTypes()
        {
        }

        protected virtual void AddAllNestedTypes()
        {
            List<AbstractTypeDefinition> enums = new List<AbstractTypeDefinition>();
            List<AbstractTypeDefinition> nativePtrClasses = new List<AbstractTypeDefinition>();
            List<AbstractTypeDefinition> rest = new List<AbstractTypeDefinition>();
            List<AbstractTypeDefinition> typedefs = new List<AbstractTypeDefinition>();

            // Only output nested types on interfaces if we are the abstract class
            if(_definition.IsInterface && !((this is IncOverridableClassProducer) || (this is CppOverridableClassProducer))) {
                return;
            }

            foreach (AbstractTypeDefinition nested in _definition.NestedTypes)
            {
                if (nested.ProtectionLevel == ProtectionLevel.Public
                    || ((AllowProtectedMembers || AllowSubclassing) && nested.ProtectionLevel == ProtectionLevel.Protected))
                {
                    if (nested is EnumDefinition || _wrapper.TypeIsWrappable(nested))
                    {
                        if (nested is EnumDefinition)
                        {
                            enums.Add(nested);
                        }
                        else if (nested.HasWrapType(WrapTypes.NativePtrValueType))
                        {
                            if (nested.HasAttribute<DefinitionIndexAttribute>())
                                nativePtrClasses.Insert(nested.GetAttribute<DefinitionIndexAttribute>().Index, nested);
                            else
                                nativePtrClasses.Add(nested);
                        }
                        else if (nested is TypedefDefinition)
                        {
                            typedefs.Add(nested);
                        }
                        else
                            rest.Add(nested);
                    }
                }
            }

            foreach (TypedefDefinition nested in typedefs)
                if (nested.BaseType.Name == "uint32" || nested.BaseType.HasAttribute<ValueTypeAttribute>())
                    AddNestedType(nested);

            foreach (AbstractTypeDefinition nested in enums)
                AddNestedType(nested);

            foreach (AbstractTypeDefinition nested in nativePtrClasses)
                AddNestedType(nested);

            foreach (AbstractTypeDefinition nested in rest)
                AddNestedType(nested);

            List<AbstractTypeDefinition> iterators = new List<AbstractTypeDefinition>();

            //Add typedefs after class declarations
            foreach (TypedefDefinition nested in typedefs)
            {
                if (nested.BaseType.Name == "uint32" || nested.BaseType.HasAttribute<ValueTypeAttribute>())
                    continue;

                if (AbstractCodeProducer.IsIteratorWrapper((TypedefDefinition)nested))
                    iterators.Add(nested);
                else
                    AddNestedType(nested);
            }

            //Add iterators last
            foreach (AbstractTypeDefinition nested in iterators)
                AddNestedType(nested);

            // Exit out here if this is CPP
            if((this is CppOverridableClassProducer)) {
                return;
            }

            //Add STL wrappers for fields that doesn't have proper typedefs

            List<string> stls = new List<string>();

            foreach (AbstractMemberDefinition m in _definition.Members)
            {
                if ((m is MemberFieldDefinition || (m is MemberMethodDefinition && (m as MemberMethodDefinition).IsDeclarableFunction))
                    && !m.IsIgnored
                    && ( m.ProtectionType == ProtectionLevel.Public
                        || ((AllowSubclassing || AllowProtectedMembers) && m.ProtectionType == ProtectionLevel.Protected)) )
                {
                    if (m.Type.IsUnnamedSTLContainer
                        && !stls.Contains(m.Type.CLRName))
                    {
                        AddNestedType(m.Type);
                        stls.Add(m.Type.CLRName);
                    }
                }
            }

            foreach (ClassDefinition iface in _interfaces)
            {
                if (iface == _definition)
                    continue;

                foreach (AbstractMemberDefinition m in iface.Members)
                {
                    if ((m is MemberFieldDefinition || (m is MemberMethodDefinition && (m as MemberMethodDefinition).IsDeclarableFunction))
                        && !m.IsIgnored
                        && (m.ProtectionType == ProtectionLevel.Public
                            || ((AllowSubclassing || AllowProtectedMembers) && m.ProtectionType == ProtectionLevel.Protected)))
                    {
                        if (m.Type.IsUnnamedSTLContainer
                            && !stls.Contains(m.Type.CLRName))
                        {
                            AddNestedType(m.Type);
                            stls.Add(m.Type.CLRName);
                        }
                    }
                }
            }
        }

        protected virtual void AddPostNestedTypes()
        {
        }

        protected virtual void AddPublicDeclarations()
        {
            foreach (MemberFieldDefinition field in _definition.PublicFields)
            {
                if (!field.IsIgnored)
                {
                    //if (CheckTypeMemberForGetProperty(field) == false)
                    //    AddMethodsForField(field);
                    //else
                        AddPropertyField(field);

                    _code.AppendEmptyLine();
                }
            }

            foreach (PropertyDefinition p in _definition.GetProperties())
            {
                if (IsPropertyAllowed(p) &&
                    ( p.ProtectionLevel == ProtectionLevel.Public
                     || ( AllowSubclassing && (p.IsStatic || !p.IsVirtual) )
                     || (AllowProtectedMembers && p.ProtectionLevel == ProtectionLevel.Protected) ) )
                {
                    AddProperty(EnhanceProperty(p));
                    _code.Append("\n");
                }
            }

            foreach (MemberMethodDefinition f in _definition.PublicMethods)
            {
                if (f.IsOperatorOverload)
                {
                    if (f.Name.EndsWith("=="))
                    {
                      if( !f.IsIgnored )
                        AddPredefinedMethods( PredefinedMethods.Equals );
                        _code.AppendEmptyLine();
                    }
                    else if (f.Name.EndsWith("="))
                    {
                      if(!f.IsIgnored)
                        AddPredefinedMethods(PredefinedMethods.CopyTo);
                        _code.AppendEmptyLine();
                    }
                }
                else if (f.IsDeclarableFunction)
                {
                    AddMethod(f);
                    _code.Append("\n");
                }
            }

            if (_definition.HasAttribute<IncludePredefinedMethodAttribute>())
            {
                AddPredefinedMethods(_definition.GetAttribute<IncludePredefinedMethodAttribute>().Methods);
                _code.AppendEmptyLine();
            }

            foreach (ClassDefinition cls in _interfaces)
            {
                if (cls == _definition)
                    continue;

                AddInterfaceImplementation(cls);
            }
        }

        protected virtual void AddInterfaceImplementation(ClassDefinition iface)
        {
            _code.AppendLine("//------------------------------------------------------------");
            _code.AppendLine("// Implementation for " + iface.CLRName);
            _code.AppendLine("//------------------------------------------------------------\n");

            foreach (PropertyDefinition ip in iface.GetProperties())
            {
                if (ip.IsStatic)
                    continue;

                if (ip.ProtectionLevel == ProtectionLevel.Public
                    || (AllowSubclassing && !ip.IsVirtual)
                    || (AllowProtectedMembers && ip.ProtectionLevel == ProtectionLevel.Protected))
                {
                    if (!ip.IsContainedIn(_definition, true))
                    {
                        AddInterfaceProperty(ip);
                        _code.Append("\n");
                    }
                }
            }

            foreach (MemberMethodDefinition inf in iface.DeclarableMethods)
            {
                if (inf.IsStatic)
                    continue;

                if (inf.ProtectionType == ProtectionLevel.Public
                    || (AllowSubclassing && !inf.IsVirtual)
                    || (AllowProtectedMembers && inf.ProtectionType == ProtectionLevel.Protected))
                {
                    if (!_definition.ContainsFunctionSignature(inf.Signature, false))
                    {
                        AddInterfaceMethod(inf);
                        _code.Append("\n");
                    }
                }
            }

            foreach (MemberFieldDefinition field in iface.Fields)
            {
                if (!field.HasAttribute<IgnoreAttribute>())
                {
                    if (field.IsStatic)
                        continue;

                    if (field.ProtectionType == ProtectionLevel.Public
                        || AllowSubclassing
                        || (AllowProtectedMembers && field.ProtectionType == ProtectionLevel.Protected))
                    {
                        //if (CheckTypeMemberForGetProperty(field) == false)
                        //    AddInterfaceMethodsForField(field);
                        //else
                            AddInterfacePropertyField(field);

                        _code.AppendEmptyLine();
                    }
                }
            }
        }

        protected virtual void AddInterfaceProperty(PropertyDefinition prop)
        {
            AddProperty(prop);
        }

        protected virtual void AddInterfaceMethod(MemberMethodDefinition f)
        {
            AddMethod(f);
        }

        protected virtual void AddInterfacePropertyField(MemberFieldDefinition field)
        {
            AddPropertyField(field);
        }

        protected virtual void AddInterfaceMethodsForField(MemberFieldDefinition field)
        {
            AddMethodsForField(field);
        }

        protected virtual void AddProtectedDeclarations()
        {
            if (AllowSubclassing)
            {
                foreach (MemberFieldDefinition field in _definition.ProtectedFields)
                {
                    if (!field.IsIgnored)
                    {
                        //if (CheckTypeMemberForGetProperty(field) == false)
                        //    AddMethodsForField(field);
                        //else
                            AddPropertyField(field);

                        _code.AppendEmptyLine();
                    }
                }

                foreach (MemberMethodDefinition f in _definition.ProtectedMethods)
                {
                    if (f.IsDeclarableFunction &&
                        (AllowProtectedMembers || f.IsStatic || !f.IsVirtual) )
                    {
                        AddMethod(f);
                        _code.Append("\n");
                    }
                }
            }
        }

        protected PropertyDefinition EnhanceProperty(PropertyDefinition property)
        {
            PropertyDefinition prop = property.Clone();
            if (_definition.BaseClass != null)
            {
                if (!prop.CanWrite)
                {
                    // There's a chance the class overrides only the get function. If the
                    // property is not declared virtual then it will be read-only, so if
                    // it's not virtual check if a base class contains the set property
                    // and if it does, include it.
                    // If it's virtual and its base function will be declared virtual, this
                    // function will be declared virtual too, so it's not necessary to add
                    // the set function.

                    if (!DeclareAsVirtual(prop.GetterFunction))
                    {
                        PropertyDefinition bp = _definition.BaseClass.GetProperty(prop.Name, true);
                        if (bp != null && bp.CanWrite)
                        {
                            prop.SetterFunction = bp.SetterFunction;
                        }
                    }
                }

                if (!prop.CanRead)
                {
                    // There's a chance the class overrides only the set function. If the
                    // property is not declared virtual then it will be write-only, so if
                    // it's not virtual check if a base class contains the set property
                    // and if it does, include it.
                    // If it's virtual and its base function will be declared virtual, this
                    // function will be declared virtual too, so it's not necessary to add
                    // the get function.

                    if (!DeclareAsVirtual(prop.SetterFunction))
                    {
                        PropertyDefinition bp = _definition.BaseClass.GetProperty(prop.Name, true);
                        if (bp != null && bp.CanRead)
                        {
                            prop.GetterFunction = bp.GetterFunction;
                        }
                    }
                }
            }

            return prop;
        }

        protected virtual void AddStaticField(MemberFieldDefinition field)
        {
        }

        protected virtual void AddMethodsForField(MemberFieldDefinition field)
        {
        }

        protected virtual void AddPropertyField(MemberFieldDefinition field)
        {
        }

        protected virtual void AddNestedType(AbstractTypeDefinition nested)
        {
        }

        protected virtual void AddMethod(MemberMethodDefinition func)
        {
        }

        protected virtual void AddProperty(PropertyDefinition prop)
        {
        }

        protected virtual void AddPredefinedMethods(PredefinedMethods pm)
        {
        }
    }
}
