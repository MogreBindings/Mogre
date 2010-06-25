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
    public class ClassProducer : Producer 
    {
        protected Wrapper _wrapper;
        protected DefClass _t;
        protected IndentStringBuilder _sb;
        protected List<DefClass> _listeners = new List<DefClass>();
        protected List<DefProperty> _interfaceProperties = new List<DefProperty>();
        protected List<DefFunction> _abstractFunctions = new List<DefFunction>();
        protected List<DefProperty> _abstractProperties = new List<DefProperty>();
        protected List<DefClass> _interfaces = new List<DefClass>();

        protected List<DefFunction> _overridableFunctions = new List<DefFunction>();
        protected DefProperty[] _overridableProperties;
        //protected List<DefField> _protectedFields = new List<DefField>();
        protected Dictionary<DefFunction, int> _methodIndices = new Dictionary<DefFunction, int>();
        protected int _methodIndicesCount = 0;

        protected List<DefMember> _cachedMembers = new List<DefMember>();

        public ClassProducer(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
        {
            this._wrapper = wrapper;
            this._t = t;
            this._sb = sb;

            foreach (DefClass iface in _t.GetInterfaces())
            {
                AddTypeDependancy(iface);
                _interfaces.Add(iface);
            }

            if (_t.IsInterface)
            {
                // Declaring an overridable class for interface
                _interfaces.Add(_t);
            }
        }

        private bool _initCalled = false;
        protected virtual void Init()
        {
            if (_initCalled)
                return;

            _initCalled = true;

            foreach (DefFunction f in _t.PublicMethods)
            {
                if (f.IsListenerAdder && !f.IsIgnored)
                {
                    _listeners.Add((DefClass)f.Parameters[0].Type);
                }
            }

            foreach (DefClass iface in _t.GetInterfaces())
            {
                // Add attributes of interface methods from the interface classes
                foreach (DefFunction f in iface.Functions)
                {
                    DefFunction tf = _t.GetFunctionWithSignature(f.Signature);
                    if (tf != null)
                        tf.Attributes.AddRange(f.Attributes);
                }

                //Store properties of interface classes. They have precedence over type's properties.
                foreach (DefProperty ip in iface.GetProperties())
                {
                    if (AllowProperty(ip) &&
                        (ip.Function.ProtectionType == ProtectionType.Public
                          || (AllowProtectedMembers && ip.Function.ProtectionType == ProtectionType.Protected)))
                    {
                        _interfaceProperties.Add(ip);
                    }
                }
            }

            foreach (DefField field in _t.Fields)
            {
                if (!field.IsIgnored && field.Type.IsSTLContainer)
                {
                    if (field.ProtectionType == ProtectionType.Public
                        || ( (AllowSubclassing || AllowProtectedMembers) && field.ProtectionType == ProtectionType.Protected))
                        MarkCachedMember(field);
                }
            }

            foreach (DefClass iface in _interfaces)
            {
                if (iface == _t)
                    continue;

                foreach (DefField field in iface.Fields)
                {
                    if (!field.IsIgnored && field.Type.IsSTLContainer
                        && !field.IsStatic)
                    {
                        if (field.ProtectionType == ProtectionType.Public
                            || ((AllowSubclassing || AllowProtectedMembers) && field.ProtectionType == ProtectionType.Protected))
                            MarkCachedMember(field);
                    }
                }
            }

            foreach (DefFunction func in _t.AbstractFunctions)
            {
                if (func.ProtectionType == ProtectionType.Public
                        || (AllowProtectedMembers && func.ProtectionType == ProtectionType.Protected))
                {
                    if ((func.Class.IsBaseForSubclassing || (func.Class == _t && AllowSubclassing)) && !func.IsProperty)
                    {
                        _isAbstractClass = true;
                        _abstractFunctions.Add(func);
                    }
                }
            }

            foreach (DefProperty prop in _t.AbstractProperties)
            {
                if (AllowProperty(prop) && (prop.Function.Class.IsBaseForSubclassing || (prop.Function.Class == _t && AllowSubclassing)))
                {
                    if (prop.Function.ProtectionType == ProtectionType.Public
                        || (AllowProtectedMembers && prop.Function.ProtectionType == ProtectionType.Protected))
                    {

                        _isAbstractClass = true;
                        _abstractProperties.Add(prop);
                    }
                }
            }

            SearchOverridableFunctions(_t);
            //SearchProtectedFields(_t);

            foreach (DefClass iface in _interfaces)
            {
                SearchOverridableFunctions(iface);
                //SearchProtectedFields(iface);
            }

            _overridableProperties = GetPropertiesFromFunctions(_overridableFunctions);

            //Find cached members

            foreach (DefMember m in _t.Members)
            {
                if (m.HasAttribute<CachedAttribute>())
                    MarkCachedMember(m);
            }

            foreach (DefClass iface in _interfaces)
            {
                if (iface == _t)
                    continue;

                foreach (DefMember m in iface.Members)
                {
                    if (m.HasAttribute<CachedAttribute>())
                        MarkCachedMember(m);
                }
            }
        }

        public static DefProperty[] GetPropertiesFromFunctions(List<DefFunction> funcs)
        {
            SortedList<string, DefProperty> props = new SortedList<string, DefProperty>();

            foreach (DefFunction f in funcs)
            {
                if (f.IsProperty)
                {
                    DefProperty p = null;

                    if (props.ContainsKey(f.CLRName))
                        p = props[f.CLRName];
                    else
                    {
                        p = new DefProperty();
                        p.Name = f.CLRName;
                        if (f.IsGetProperty)
                        {
                            p.TypeName = f.TypeName;
                            p.PassedByType = f.PassedByType;
                        }
                        else
                        {
                            p.TypeName = f.Parameters[0].TypeName;
                            p.PassedByType = f.Parameters[0].PassedByType;
                        }

                        props.Add(f.CLRName, p);
                    }

                    if (f.IsGetProperty)
                    {
                        p.CanRead = true;
                        p.GetterFunction = f;
                    }
                    else if (f.IsSetProperty)
                    {
                        p.CanWrite = true;
                        p.SetterFunction = f;
                    }
                }
            }

            DefProperty[] parr = new DefProperty[props.Count];
            for (int i = 0; i < props.Count; i++)
                parr[i] = props.Values[i];

            return parr;
        }

        public virtual string ClassFullNativeName
        {
            get { return _t.FullNativeName; }
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
            get { return !_t.HasAttribute<NotConstructableAttribute>(); }
        }

        protected virtual bool AllowVirtualMethods
        {
            get { return _t.IsBaseForSubclassing; }
        }

        protected virtual bool AllowProtectedMembers
        {
            get { return false; }
        }

        protected virtual bool AllowSubclassing
        {
            get { return _t.IsBaseForSubclassing; }
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
            get { return !_t.HasWrapType(WrapTypes.NativePtrValueType) && !_t.HasWrapType(WrapTypes.ValueType); }
        }

        protected virtual bool IsReadOnly
        {
            get { return _t.HasAttribute<ReadOnlyAttribute>(); }
        }

        protected virtual bool DeclareAsVirtual(DefFunction f)
        {
            return (f.IsVirtual && AllowVirtualMethods) || f.IsVirtualInterfaceMethod
                || (f.IsVirtual && f.BaseFunction != null && f.BaseFunction.Class.AllowVirtuals);
        }

        protected virtual bool DeclareAsOverride(DefFunction f)
        {
            return (f.IsOverride && DeclareAsVirtual(f))
                || (f.IsVirtualInterfaceMethod && !f.Class.IsInterface && !f.Class.ContainsInterfaceFunctionSignature(f.Signature, false));
        }

        protected DefClass GetTopClass(DefClass type)
        {
            if (type.BaseClass == null)
                return type;
            else
                return GetTopClass(type.BaseClass);
        }

        protected virtual void MarkCachedMember(DefMember m)
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

        protected virtual string GetNativeInvokationTarget(DefFunction f)
        {
            if (!f.IsStatic)
            {
                if (f.ProtectionType == ProtectionType.Public)
                {
                    return GetNativeInvokationTarget(f.IsConstFunctionCall) + "->" + f.Name;
                }
                else if (f.ProtectionType == ProtectionType.Protected)
                {
                    if (!f.IsVirtual)
                    {
                        string proxyName = NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(_t);
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
                if (f.ProtectionType == ProtectionType.Public)
                    return f.Class.FullNativeName + "::" + f.Name;
                else
                    return NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(f.Class) + "::" + f.Name;
            }
        }
        protected virtual string GetNativeInvokationTarget(DefField field)
        {
            if (!field.IsStatic)
            {
                if (field.ProtectionType == ProtectionType.Public)
                {
                    return GetNativeInvokationTarget() + "->" + field.Name;
                }
                else if (field.ProtectionType == ProtectionType.Protected)
                {
                    string proxyName = NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(_t);
                    return "static_cast<" + proxyName + "*>(_native)->" + field.Name;
                }
                else
                    throw new Exception("Unexpected");
            }
            else
            {
                if (field.ProtectionType == ProtectionType.Public)
                    return field.Class.FullNativeName + "::" + field.Name;
                else
                    return NativeProtectedStaticsProxy.GetProtectedStaticsProxyName(field.Class) + "::" + field.Name;
            }
        }

        protected virtual string GetNativeInvokationTargetObject()
        {
            return "*(static_cast<" + ClassFullNativeName + "*>(_native))";
        }

        protected virtual void SearchOverridableFunctions(DefClass type)
        {
            foreach (DefFunction func in type.Functions)
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

            foreach (DefClass iface in type.GetInterfaces())
                SearchOverridableFunctions(iface);

            if (type.BaseClass != null)
                SearchOverridableFunctions(type.BaseClass);
        }

        protected bool ContainsFunction(DefFunction func, List<DefFunction> list)
        {
            foreach (DefFunction lf in list)
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
            IndentStringBuilder orig = _sb;
            _sb = new IndentStringBuilder();

            Add();

            orig.sb.Insert(0, _sb.ToString());
            _sb = orig;
        }

        protected virtual string ReplaceCustomVariables(string txt)
        {
            string targ = "static_cast<" + ClassFullNativeName + "*>(_native)";
            txt = txt.Replace("@NATIVE@", targ);
            txt = txt.Replace("@CLASS@", GetClassName());
            return txt;
        }

        protected virtual string ReplaceCustomVariables(string txt, DefFunction func)
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
            string full = _t.FullCLRName;
            int index = full.IndexOf("::");
            return full.Substring(index + 2);
        }

        protected virtual string GetTopBaseClassName()
        {
            return null;
        }

        protected virtual string GetBaseClassName()
        {
            return (_t.BaseClass == null || _t.BaseClass.HasWrapType(WrapTypes.NativeDirector)) ? GetTopBaseClassName() : _t.BaseClass.Name;
        }

        protected bool HasStaticCachedFields()
        {
            foreach (DefMember m in _cachedMembers)
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
            foreach (DefType nested in _t.NestedTypes)
            {
                if (nested.ProtectionType == ProtectionType.Public
                    || ((AllowProtectedMembers || AllowSubclassing) && nested.ProtectionType == ProtectionType.Protected))
                {
                    if (nested.HasWrapType(WrapTypes.NativeDirector))
                        AddNestedTypeBeforeMainType(nested);
                }
            }

            _sb.AppendLine("//################################################################");
            _sb.AppendLine("//" + _t.CLRName);
            _sb.AppendLine("//################################################################\n");
        }

        protected virtual void AddNestedTypeBeforeMainType(DefType nested)
        {
        }

        protected virtual void AddBody()
        {
            AddPreNestedTypes();
            _sb.AppendLine("//Nested Types");
            AddAllNestedTypes();
            AddPostNestedTypes();
            _sb.AppendLine("//Private Declarations");
            AddPrivateDeclarations();
            _sb.Append("\n");
            _sb.AppendLine("//Internal Declarations");
            AddInternalDeclarations();
            _sb.Append("\n");
            _sb.AppendLine("//Public Declarations");
            AddPublicDeclarations();
            _sb.Append("\n");
            _sb.AppendLine("//Protected Declarations");
            AddProtectedDeclarations();
            _sb.Append("\n");
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
                _sb.AppendLine();
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
            List<DefType> enums = new List<DefType>();
            List<DefType> nativePtrClasses = new List<DefType>();
            List<DefType> rest = new List<DefType>();
            List<DefType> typedefs = new List<DefType>();

            // Only output nested types on interfaces if we are the abstract class
            if(_t.IsInterface && !((this is IncOverridableClassProducer) || (this is CppOverridableClassProducer))) {
                return;
            }

            foreach (DefType nested in _t.NestedTypes)
            {
                if (nested.ProtectionType == ProtectionType.Public
                    || ((AllowProtectedMembers || AllowSubclassing) && nested.ProtectionType == ProtectionType.Protected))
                {
                    if (nested is DefEnum || _wrapper.TypeIsWrappable(nested))
                    {
                        if (nested is DefEnum)
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
                        else if (nested is DefTypeDef)
                        {
                            typedefs.Add(nested);
                        }
                        else
                            rest.Add(nested);
                    }
                }
            }

            foreach (DefTypeDef nested in typedefs)
                if (nested.BaseType.Name == "uint32" || nested.BaseType.HasAttribute<ValueTypeAttribute>())
                    AddNestedType(nested);

            foreach (DefType nested in enums)
                AddNestedType(nested);

            foreach (DefType nested in nativePtrClasses)
                AddNestedType(nested);

            foreach (DefType nested in rest)
                AddNestedType(nested);

            List<DefType> iterators = new List<DefType>();

            //Add typedefs after class declarations
            foreach (DefTypeDef nested in typedefs)
            {
                if (nested.BaseType.Name == "uint32" || nested.BaseType.HasAttribute<ValueTypeAttribute>())
                    continue;

                if (Producer.IsIteratorWrapper((DefTypeDef)nested))
                    iterators.Add(nested);
                else
                    AddNestedType(nested);
            }

            //Add iterators last
            foreach (DefType nested in iterators)
                AddNestedType(nested);

            // Exit out here if this is CPP
            if((this is CppOverridableClassProducer)) {
                return;
            }

            //Add STL wrappers for fields that doesn't have proper typedefs

            List<string> stls = new List<string>();

            foreach (DefMember m in _t.Members)
            {
                if ((m is DefField || (m is DefFunction && (m as DefFunction).IsDeclarableFunction))
                    && !m.IsIgnored
                    && ( m.ProtectionType == ProtectionType.Public
                        || ((AllowSubclassing || AllowProtectedMembers) && m.ProtectionType == ProtectionType.Protected)) )
                {
                    if (m.Type.IsUnnamedSTLContainer
                        && !stls.Contains(m.Type.CLRName))
                    {
                        AddNestedType(m.Type);
                        stls.Add(m.Type.CLRName);
                    }
                }
            }

            foreach (DefClass iface in _interfaces)
            {
                if (iface == _t)
                    continue;

                foreach (DefMember m in iface.Members)
                {
                    if ((m is DefField || (m is DefFunction && (m as DefFunction).IsDeclarableFunction))
                        && !m.IsIgnored
                        && (m.ProtectionType == ProtectionType.Public
                            || ((AllowSubclassing || AllowProtectedMembers) && m.ProtectionType == ProtectionType.Protected)))
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
            foreach (DefField field in _t.PublicFields)
            {
                if (!field.IsIgnored)
                {
                    //if (CheckTypeMemberForGetProperty(field) == false)
                    //    AddMethodsForField(field);
                    //else
                        AddPropertyField(field);

                    _sb.AppendLine();
                }
            }

            foreach (DefProperty p in _t.GetProperties())
            {
                if (AllowProperty(p) &&
                    ( p.Function.ProtectionType == ProtectionType.Public
                     || ( AllowSubclassing && (p.Function.IsStatic || !p.Function.IsVirtual) )
                     || (AllowProtectedMembers && p.Function.ProtectionType == ProtectionType.Protected) ) )
                {
                    AddProperty(EnhanceProperty(p));
                    _sb.Append("\n");
                }
            }

            foreach (DefFunction f in _t.PublicMethods)
            {
                if (f.IsOperatorOverload)
                {
                    if (f.Name.EndsWith("=="))
                    {
                        AddPredefinedMethods(PredefinedMethods.Equals);
                        _sb.AppendLine();
                    }
                    else if (f.Name.EndsWith("="))
                    {
                        AddPredefinedMethods(PredefinedMethods.CopyTo);
                        _sb.AppendLine();
                    }
                }
                else if (f.IsDeclarableFunction)
                {
                    AddMethod(f);
                    _sb.Append("\n");
                }
            }

            if (_t.HasAttribute<IncludePredefinedMethodAttribute>())
            {
                AddPredefinedMethods(_t.GetAttribute<IncludePredefinedMethodAttribute>().Methods);
                _sb.AppendLine();
            }

            foreach (DefClass cls in _interfaces)
            {
                if (cls == _t)
                    continue;

                AddInterfaceImplementation(cls);
            }
        }

        protected virtual void AddInterfaceImplementation(DefClass iface)
        {
            _sb.AppendLine("//------------------------------------------------------------");
            _sb.AppendLine("// Implementation for " + iface.CLRName);
            _sb.AppendLine("//------------------------------------------------------------\n");

            foreach (DefProperty ip in iface.GetProperties())
            {
                DefFunction inf = ip.Function;

                if (inf.IsStatic)
                    continue;

                if (inf.ProtectionType == ProtectionType.Public
                    || (AllowSubclassing && !inf.IsVirtual)
                    || (AllowProtectedMembers && inf.ProtectionType == ProtectionType.Protected))
                {
                    if (!_t.ContainsFunctionSignature(inf.Signature, true))
                    {
                        AddInterfaceProperty(ip);
                        _sb.Append("\n");
                    }
                }
            }

            foreach (DefFunction inf in iface.DeclarableMethods)
            {
                if (inf.IsStatic)
                    continue;

                if (inf.ProtectionType == ProtectionType.Public
                    || (AllowSubclassing && !inf.IsVirtual)
                    || (AllowProtectedMembers && inf.ProtectionType == ProtectionType.Protected))
                {
                    if (!_t.ContainsFunctionSignature(inf.Signature, false))
                    {
                        AddInterfaceMethod(inf);
                        _sb.Append("\n");
                    }
                }
            }

            foreach (DefField field in iface.Fields)
            {
                if (!field.HasAttribute<IgnoreAttribute>())
                {
                    if (field.IsStatic)
                        continue;

                    if (field.ProtectionType == ProtectionType.Public
                        || AllowSubclassing
                        || (AllowProtectedMembers && field.ProtectionType == ProtectionType.Protected))
                    {
                        //if (CheckTypeMemberForGetProperty(field) == false)
                        //    AddInterfaceMethodsForField(field);
                        //else
                            AddInterfacePropertyField(field);

                        _sb.AppendLine();
                    }
                }
            }
        }

        protected virtual void AddInterfaceProperty(DefProperty prop)
        {
            AddProperty(prop);
        }

        protected virtual void AddInterfaceMethod(DefFunction f)
        {
            AddMethod(f);
        }

        protected virtual void AddInterfacePropertyField(DefField field)
        {
            AddPropertyField(field);
        }

        protected virtual void AddInterfaceMethodsForField(DefField field)
        {
            AddMethodsForField(field);
        }

        protected virtual void AddProtectedDeclarations()
        {
            if (AllowSubclassing)
            {
                foreach (DefField field in _t.ProtectedFields)
                {
                    if (!field.IsIgnored)
                    {
                        //if (CheckTypeMemberForGetProperty(field) == false)
                        //    AddMethodsForField(field);
                        //else
                            AddPropertyField(field);

                        _sb.AppendLine();
                    }
                }

                foreach (DefFunction f in _t.ProtectedMethods)
                {
                    if (f.IsDeclarableFunction &&
                        (AllowProtectedMembers || f.IsStatic || !f.IsVirtual) )
                    {
                        AddMethod(f);
                        _sb.Append("\n");
                    }
                }
            }
        }

        protected DefProperty EnhanceProperty(DefProperty property)
        {
            DefProperty prop = property.Clone();
            if (_t.BaseClass != null)
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
                        DefProperty bp = _t.BaseClass.GetProperty(prop.Name, true);
                        if (bp != null && bp.CanWrite)
                        {
                            prop.CanWrite = true;
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
                        DefProperty bp = _t.BaseClass.GetProperty(prop.Name, true);
                        if (bp != null && bp.CanRead)
                        {
                            prop.CanRead = true;
                            prop.GetterFunction = bp.GetterFunction;
                        }
                    }
                }
            }

            return prop;
        }

        protected virtual void AddStaticField(DefField field)
        {
        }

        protected virtual void AddMethodsForField(DefField field)
        {
        }

        protected virtual void AddPropertyField(DefField field)
        {
        }

        protected virtual void AddNestedType(DefType nested)
        {
        }

        protected virtual void AddMethod(DefFunction func)
        {
        }

        protected virtual void AddProperty(DefProperty prop)
        {
        }

        protected virtual void AddPredefinedMethods(PredefinedMethods pm)
        {
        }
    }
}
