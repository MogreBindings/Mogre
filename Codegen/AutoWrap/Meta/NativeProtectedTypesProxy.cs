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
    class NativeProtectedTypesProxy : ClassProducer
    {
        public static string GetProtectedTypesProxyName(DefType type)
        {
            string name = type.FullNativeName;
            name = name.Substring(name.IndexOf("::") + 2);
            name = name.Replace("::", "_");
            name = "Mogre::" + name + "_ProtectedTypesProxy";
            return name;
        }

        public NativeProtectedTypesProxy(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }

        public override void Add()
        {
            Init();

            if (HasProtectedTypes() || HasProtectedStaticFields())
            {
                string className = GetProtectedTypesProxyName(_t);
                className = className.Substring(className.IndexOf("::") + 2);
                _sb.AppendLine("class " + className + " : public " + _t.FullNativeName);
                _sb.AppendLine("{");
                _sb.AppendLine("public:");
                _sb.IncreaseIndent();

                className = _t.FullCLRName;
                className = className.Substring(className.IndexOf("::") + 2);

                if (_t.IsInterface)
                {
                    className = className.Replace(_t.CLRName, _t.Name);
                }

                className = _wrapper.ManagedNamespace + "::" + className;

                _sb.AppendLine("friend ref class " + className + ";");

                foreach (DefType nested in _t.NestedTypes)
                {
                    DefType type = nested.FindType<DefType>(nested.Name);

                    if (type.ProtectionType == ProtectionType.Protected
                        && type.IsSTLContainer && _wrapper.TypeIsWrappable(type))
                    {
                        AddNestedType(type);
                    }
                }

                _sb.DecreaseIndent();
                _sb.AppendLine("};\n");
            }
        }

        protected virtual bool HasProtectedTypes()
        {
            foreach (DefType nested in _t.NestedTypes)
            {
                DefType type = nested.FindType<DefType>(nested.Name);

                if (type.ProtectionType == ProtectionType.Protected
                    && type.IsSTLContainer && _wrapper.TypeIsWrappable(type) )
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool HasProtectedStaticFields()
        {
            foreach (DefField field in _t.Fields)
            {
                if (field.ProtectionType == ProtectionType.Protected
                    && field.IsStatic
                    && !field.IsIgnored)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void AddNestedType(DefType nested)
        {
            if (nested.IsSTLContainer)
            {
                _sb.AppendLine("typedef " + _t.FullNativeName + "::" + nested.Name + " " + nested.CLRName + ";");
            }
            else
                throw new Exception("Unexpected");
        }

    //    protected override void AddBody()
    //    {
    //        AddAllNestedTypes();

    //        foreach (DefField field in _t.ProtectedFields)
    //        {
    //            if (!field.HasAttribute<IgnoreAttribute>()
    //                && field.IsStatic)
    //            {
    //                AddStaticField(field);
    //                _sb.AppendLine();
    //                continue;
    //            }
    //        }

    //        foreach (DefFunction f in _t.ProtectedMethods)
    //        {
    //            if (f.IsDeclarableFunction && f.IsStatic)
    //            {
    //                AddMethod(f);
    //                _sb.AppendLine();
    //            }
    //        }
    //    }

    //    protected override void AddAllNestedTypes()
    //    {
    //        foreach (DefType nested in _t.NestedTypes)
    //        {
    //            DefType type = (nested.IsNested) ? nested.ParentClass.FindType<DefType>(nested.Name) : nested.NameSpace.FindType<DefType>(nested.Name);

    //            if (type.ProtectionType == ProtectionType.Protected
    //                && (type is DefEnum || (type.IsSTLContainer && _wrapper.TypeIsWrappable(type))))
    //            {
    //                type.ProtectionType = ProtectionType.Public;
    //                AddNestedType(type);
    //                type.ProtectionType = ProtectionType.Protected;
    //            }
    //        }
    //    }

    //    protected virtual void AddNativeMethodParams(DefFunction f)
    //    {
    //        for (int i = 0; i < f.Parameters.Count; i++)
    //        {
    //            DefParam param = f.Parameters[i];
    //            _sb.Append(" ");

    //            _sb.Append(param.NativeTypeName);
    //            _sb.Append(" " + param.Name);

    //            if (i < f.Parameters.Count - 1) _sb.Append(",");
    //        }
    //    }
    }

    class NativeProtectedStaticsProxy : ClassProducer
    {
        public static string GetProtectedStaticsProxyName(DefType type)
        {
            string name = type.FullNativeName;
            name = name.Substring(name.IndexOf("::") + 2);
            name = name.Replace("::", "_");
            name = "Mogre::" + name + "_ProtectedStaticsProxy";
            return name;
        }

        public NativeProtectedStaticsProxy(Wrapper wrapper, DefClass t, IndentStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }

        public override void Add()
        {
            Init();

            if (HasProtectedStatics())
            {
                string className = GetProtectedStaticsProxyName(_t);
                className = className.Substring(className.IndexOf("::") + 2);
                _sb.AppendLine("class " + className + " : public " + _t.FullNativeName);
                _sb.AppendLine("{");
                _sb.AppendLine("public:");
                _sb.IncreaseIndent();

                className = _t.FullCLRName;
                className = className.Substring(className.IndexOf("::") + 2);

                if (_t.IsInterface)
                {
                    className = className.Replace(_t.CLRName, _t.Name);
                }

                className = _wrapper.ManagedNamespace + "::" + className;

                _sb.AppendLine("friend ref class " + className + ";");

                AddFriends(className, _t);

                foreach (DefClass iface in _interfaces)
                {
                    if (iface == _t)
                        continue;

                    AddFriends(className, iface);
                }

                _sb.DecreaseIndent();
                _sb.AppendLine("};\n");
            }
        }

        protected virtual void AddFriends(string className, DefClass type)
        {
            foreach (DefField field in type.ProtectedFields)
            {
                if (!field.IsIgnored
                    && !(field.IsStatic && type != _t) )
                {
                    _sb.AppendLine("friend ref class " + className + "::" + field.Name + ";");
                }
            }

            foreach (DefFunction func in type.Functions)
            {
                if (func.IsDeclarableFunction
                    && func.ProtectionType == ProtectionType.Protected
                    && !(func.IsStatic && type != _t)
                    && func.IsProperty
                    && !func.IsVirtual)
                {
                    _sb.AppendLine("friend ref class " + className + "::" + func.CLRName + ";");
                }
            }
        }

        protected virtual bool HasProtectedStatics()
        {
            if (HasProtectedStatics(_t))
                return true;

            foreach (DefClass iface in _interfaces)
            {
                if (iface == _t)
                    continue;

                if (HasProtectedStatics(iface))
                    return true;
            }

            return false;
        }

        protected virtual bool HasProtectedStatics(DefClass type)
        {
            foreach (DefField field in type.ProtectedFields)
            {
                if (!field.IsIgnored
                    && !(field.IsStatic && type != _t))
                {
                    return true;
                }
            }

            foreach (DefFunction func in type.Functions)
            {
                if (func.IsDeclarableFunction
                    && func.ProtectionType == ProtectionType.Protected
                    && !(func.IsStatic && type != _t)
                    && !func.IsVirtual)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

