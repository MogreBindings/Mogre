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
    class NativeProtectedTypesProxy : ClassCodeProducer
    {
        public static string GetProtectedTypesProxyName(AbstractTypeDefinition type)
        {
            string name = type.FullNativeName;
            name = name.Substring(name.IndexOf("::") + 2);
            name = name.Replace("::", "_");
            name = "Mogre::" + name + "_ProtectedTypesProxy";
            return name;
        }

        public NativeProtectedTypesProxy(Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }

        public override void Add()
        {
            Init();

            if (HasProtectedTypes() || HasProtectedStaticFields())
            {
                string className = GetProtectedTypesProxyName(_definition);
                className = className.Substring(className.IndexOf("::") + 2);
                _sb.AppendLine("class " + className + " : public " + _definition.FullNativeName);
                _sb.AppendLine("{");
                _sb.AppendLine("public:");
                _sb.IncreaseIndent();

                className = _definition.FullCLRName;
                className = className.Substring(className.IndexOf("::") + 2);

                if (_definition.IsInterface)
                {
                    className = className.Replace(_definition.CLRName, _definition.Name);
                }

                className = _wrapper.ManagedNamespace + "::" + className;

                _sb.AppendLine("friend ref class " + className + ";");

                foreach (AbstractTypeDefinition nested in _definition.NestedTypes)
                {
                    AbstractTypeDefinition type = nested.FindType<AbstractTypeDefinition>(nested.Name);

                    if (type.ProtectionLevel == ProtectionLevel.Protected
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
            foreach (AbstractTypeDefinition nested in _definition.NestedTypes)
            {
                AbstractTypeDefinition type = nested.FindType<AbstractTypeDefinition>(nested.Name);

                if (type.ProtectionLevel == ProtectionLevel.Protected
                    && type.IsSTLContainer && _wrapper.TypeIsWrappable(type) )
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool HasProtectedStaticFields()
        {
            foreach (MemberFieldDefinition field in _definition.Fields)
            {
                if (field.ProtectionType == ProtectionLevel.Protected
                    && field.IsStatic
                    && !field.IsIgnored)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void AddNestedType(AbstractTypeDefinition nested)
        {
            if (nested.IsSTLContainer)
            {
                _sb.AppendLine("typedef " + _definition.FullNativeName + "::" + nested.Name + " " + nested.CLRName + ";");
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

    class NativeProtectedStaticsProxy : ClassCodeProducer
    {
        public static string GetProtectedStaticsProxyName(AbstractTypeDefinition type)
        {
            string name = type.FullNativeName;
            name = name.Substring(name.IndexOf("::") + 2);
            name = name.Replace("::", "_");
            name = "Mogre::" + name + "_ProtectedStaticsProxy";
            return name;
        }

        public NativeProtectedStaticsProxy(Wrapper wrapper, ClassDefinition t, SourceCodeStringBuilder sb)
            : base(wrapper, t, sb)
        {
        }

        public override void Add()
        {
            Init();

            if (HasProtectedStatics())
            {
                string className = GetProtectedStaticsProxyName(_definition);
                className = className.Substring(className.IndexOf("::") + 2);
                _sb.AppendLine("class " + className + " : public " + _definition.FullNativeName);
                _sb.AppendLine("{");
                _sb.AppendLine("public:");
                _sb.IncreaseIndent();

                className = _definition.FullCLRName;
                className = className.Substring(className.IndexOf("::") + 2);

                if (_definition.IsInterface)
                {
                    className = className.Replace(_definition.CLRName, _definition.Name);
                }

                className = _wrapper.ManagedNamespace + "::" + className;

                _sb.AppendLine("friend ref class " + className + ";");

                AddFriends(className, _definition);

                foreach (ClassDefinition iface in _interfaces)
                {
                    if (iface == _definition)
                        continue;

                    AddFriends(className, iface);
                }

                _sb.DecreaseIndent();
                _sb.AppendLine("};\n");
            }
        }

        protected virtual void AddFriends(string className, ClassDefinition type)
        {
            foreach (MemberFieldDefinition field in type.ProtectedFields)
            {
                if (!field.IsIgnored
                    && !(field.IsStatic && type != _definition) )
                {
                    _sb.AppendLine("friend ref class " + className + "::" + field.Name + ";");
                }
            }

            foreach (MemberMethodDefinition func in type.Functions)
            {
                if (func.IsDeclarableFunction
                    && func.ProtectionType == ProtectionLevel.Protected
                    && !(func.IsStatic && type != _definition)
                    && func.IsProperty
                    && !func.IsVirtual)
                {
                    _sb.AppendLine("friend ref class " + className + "::" + func.CLRName + ";");
                }
            }
        }

        protected virtual bool HasProtectedStatics()
        {
            if (HasProtectedStatics(_definition))
                return true;

            foreach (ClassDefinition iface in _interfaces)
            {
                if (iface == _definition)
                    continue;

                if (HasProtectedStatics(iface))
                    return true;
            }

            return false;
        }

        protected virtual bool HasProtectedStatics(ClassDefinition type)
        {
            foreach (MemberFieldDefinition field in type.ProtectedFields)
            {
                if (!field.IsIgnored
                    && !(field.IsStatic && type != _definition))
                {
                    return true;
                }
            }

            foreach (MemberMethodDefinition func in type.Functions)
            {
                if (func.IsDeclarableFunction
                    && func.ProtectionType == ProtectionLevel.Protected
                    && !(func.IsStatic && type != _definition)
                    && !func.IsVirtual)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

