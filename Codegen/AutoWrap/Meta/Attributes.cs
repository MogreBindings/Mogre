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
using System.Xml;

namespace AutoWrap.Meta
{
    public enum WrapTypes { NonOverridable, Overridable, NativeDirector, Interface,
        Singleton, SharedPtr, ReadOnlyStruct, ValueType, NativePtrValueType, CLRHandle, PlainWrapper }

    public class AutoWrapAttribute : Attribute
    {
        public virtual void ProcessHolder(AttributeHolder holder)
        {
        }

        protected void AddAttributeToInheritanceChain(DefClass type, AutoWrapAttribute attr)
        {
            bool hasit = false;
            foreach (AutoWrapAttribute a in type.Attributes)
            {
                if (a.GetType() == attr.GetType())
                {
                    hasit = true;
                    break;
                }
            }

            if (!hasit)
                type.Attributes.Add(attr);

            if (type.BaseClass != null)
                AddAttributeToInheritanceChain(type.BaseClass, attr);
        }
    }

    public class WrapTypeAttribute : AutoWrapAttribute
    {
        public override void ProcessHolder(AttributeHolder holder)
        {
            switch (WrapType)
            {
                case WrapTypes.NativePtrValueType:
                case WrapTypes.ValueType:
                    if (!holder.HasAttribute<ValueTypeAttribute>())
                        holder.Attributes.Add(new ValueTypeAttribute());
                    break;
                case WrapTypes.Overridable:
                    DefClass type = (DefClass)holder;
                    AddAttributeToInheritanceChain(type, new BaseForSubclassingAttribute());
                    break;
            }
        }

        public WrapTypes WrapType;

        public WrapTypeAttribute(WrapTypes type)
        {
            this.WrapType = type;
        }

        public static WrapTypeAttribute FromElement(XmlElement elem)
        {
            WrapTypes wt = (WrapTypes)Enum.Parse(typeof(WrapTypes), elem.InnerText, true);
            return new WrapTypeAttribute(wt);
        }
    }

    public class CachedAttribute : AutoWrapAttribute
    {
        public static CachedAttribute FromElement(XmlElement elem)
        {
            return new CachedAttribute();
        }
    }

    public class PropertyAttribute : AutoWrapAttribute
    {
        public static PropertyAttribute FromElement(XmlElement elem)
        {
            return new PropertyAttribute();
        }
    }

    public class MethodAttribute : AutoWrapAttribute
    {
        public static MethodAttribute FromElement(XmlElement elem)
        {
            return new MethodAttribute();
        }
    }

    [Flags]
    public enum PredefinedMethods { Equals = 1, CopyTo = 2 }
    public class IncludePredefinedMethodAttribute : AutoWrapAttribute
    {
        public PredefinedMethods Methods;

        public IncludePredefinedMethodAttribute(PredefinedMethods methods)
        {
            this.Methods = methods;
        }

        public static IncludePredefinedMethodAttribute FromElement(XmlElement elem)
        {
            PredefinedMethods pm = (PredefinedMethods)Enum.Parse(typeof(PredefinedMethods), elem.InnerText, true);
            return new IncludePredefinedMethodAttribute(pm);
        }
    }

    public class ValueTypeAttribute : AutoWrapAttribute
    {
        public static ValueTypeAttribute FromElement(XmlElement elem)
        {
            return new ValueTypeAttribute();
        }
    }

    public class CLRObjectAttribute : AutoWrapAttribute
    {
        public static CLRObjectAttribute FromElement(XmlElement elem)
        {
            return new CLRObjectAttribute();
        }
    }

    // Put it on a top public class to make the public class and all its subclasses have WrapType.Overridable
    public class OverridableAttribute : AutoWrapAttribute
    {
        public static OverridableAttribute FromElement(XmlElement elem)
        {
            return new OverridableAttribute();
        }
    }

    public class NotConstructableAttribute : AutoWrapAttribute
    {
        public static NotConstructableAttribute FromElement(XmlElement elem)
        {
            return new NotConstructableAttribute();
        }
    }

    public class IgnoreAttribute : AutoWrapAttribute
    {
        public static IgnoreAttribute FromElement(XmlElement elem)
        {
            return new IgnoreAttribute();
        }
    }

    public class ArrayTypeAttribute : AutoWrapAttribute
    {
        public int Length;

        public ArrayTypeAttribute(int length)
        {
            this.Length = length;
        }
        public static ArrayTypeAttribute FromElement(XmlElement elem)
        {
            int len = 0;
            if (elem.InnerText.Trim() != "")
                len = int.Parse(elem.InnerText);

            return new ArrayTypeAttribute(len);
        }
    }

    public class CustomIncDeclarationAttribute : AutoWrapAttribute
    {
        public string DeclarationText;

        public CustomIncDeclarationAttribute(string decl)
        {
            this.DeclarationText = decl;
        }
        public static CustomIncDeclarationAttribute FromElement(XmlElement elem)
        {
            return new CustomIncDeclarationAttribute(elem.InnerText);
        }
    }

    public class CustomCppDeclarationAttribute : AutoWrapAttribute
    {
        public string DeclarationText;

        public CustomCppDeclarationAttribute(string decl)
        {
            this.DeclarationText = decl;
        }
        public static CustomCppDeclarationAttribute FromElement(XmlElement elem)
        {
            return new CustomCppDeclarationAttribute(elem.InnerText);
        }
    }

    public class CustomNativeProxyDeclarationAttribute : AutoWrapAttribute
    {
        public string DeclarationText;

        public CustomNativeProxyDeclarationAttribute(string decl)
        {
            this.DeclarationText = decl;
        }
        public static CustomNativeProxyDeclarationAttribute FromElement(XmlElement elem)
        {
            return new CustomNativeProxyDeclarationAttribute(elem.InnerText);
        }
    }

    public class CustomIncClassDefinitionAttribute : AutoWrapAttribute
    {
        public string Text;

        public CustomIncClassDefinitionAttribute(string decl)
        {
            this.Text = decl;
        }
        public static CustomIncClassDefinitionAttribute FromElement(XmlElement elem)
        {
            return new CustomIncClassDefinitionAttribute(elem.InnerText);
        }
    }

    public class CustomCppClassDefinitionAttribute : AutoWrapAttribute
    {
        public string Text;

        public CustomCppClassDefinitionAttribute(string decl)
        {
            this.Text = decl;
        }
        public static CustomCppClassDefinitionAttribute FromElement(XmlElement elem)
        {
            return new CustomCppClassDefinitionAttribute(elem.InnerText);
        }
    }

    public class BaseClassAttribute : AutoWrapAttribute
    {
        public string Name;

        public BaseClassAttribute(string name)
        {
            this.Name = name;
        }
        public static BaseClassAttribute FromElement(XmlElement elem)
        {
            return new BaseClassAttribute(elem.InnerText);
        }
    }

    public class RenameAttribute : AutoWrapAttribute
    {
        public string Name;

        public RenameAttribute(string name)
        {
            this.Name = name;
        }
        public static RenameAttribute FromElement(XmlElement elem)
        {
            return new RenameAttribute(elem.InnerText);
        }
    }

    public class DefaultReturnValueAttribute : AutoWrapAttribute
    {
        public string Name;

        public DefaultReturnValueAttribute(string name)
        {
            this.Name = name;
        }
        public static DefaultReturnValueAttribute FromElement(XmlElement elem)
        {
            return new DefaultReturnValueAttribute(elem.InnerText);
        }
    }

    /// <summary>
    /// Used to replace one type by another
    /// </summary>
    public class ReplaceByAttribute : AutoWrapAttribute
    {
        public string Name;

        public ReplaceByAttribute(string name)
        {
            this.Name = name;
        }
        public static ReplaceByAttribute FromElement(XmlElement elem)
        {
            return new ReplaceByAttribute(elem.InnerText);
        }
    }

    public class StopDelegationForReturnAttribute : AutoWrapAttribute
    {
        public string Return;

        public StopDelegationForReturnAttribute(string ret  )
        {
            this.Return = ret;
        }
        public static StopDelegationForReturnAttribute FromElement(XmlElement elem)
        {
            return new StopDelegationForReturnAttribute(elem.InnerText);
        }
    }

    public class FlagsEnumAttribute : AutoWrapAttribute
    {
        public static FlagsEnumAttribute FromElement(XmlElement elem)
        {
            return new FlagsEnumAttribute();
        }
    }

    public class ReturnOnlyByMethodAttribute : AutoWrapAttribute
    {
        public static ReturnOnlyByMethodAttribute FromElement(XmlElement elem)
        {
            return new ReturnOnlyByMethodAttribute();
        }
    }

    public class PureManagedClassAttribute : AutoWrapAttribute
    {
        public string FirstMember;

        public PureManagedClassAttribute(string firstMember)
        {
            this.FirstMember = firstMember;
        }
        public static PureManagedClassAttribute FromElement(XmlElement elem)
        {
            return new PureManagedClassAttribute(elem.InnerText);
        }
    }

    public class ReadOnlyAttribute : AutoWrapAttribute
    {
        public static ReadOnlyAttribute FromElement(XmlElement elem)
        {
            return new ReadOnlyAttribute();
        }
    }

    public class ReadOnlyForFieldsAttribute : AutoWrapAttribute
    {
        public static ReadOnlyForFieldsAttribute FromElement(XmlElement elem)
        {
            return new ReadOnlyForFieldsAttribute();
        }
    }

    public class NativeValueContainerAttribute : AutoWrapAttribute
    {
        public static NativeValueContainerAttribute FromElement(XmlElement elem)
        {
            return new NativeValueContainerAttribute();
        }
    }

    public class ExplicitCastingForParamsAttribute : AutoWrapAttribute
    {
        public static ExplicitCastingForParamsAttribute FromElement(XmlElement elem)
        {
            return new ExplicitCastingForParamsAttribute();
        }
    }

    public class BaseForSubclassingAttribute : AutoWrapAttribute
    {
        public override void ProcessHolder(AttributeHolder holder)
        {
            DefClass type = (DefClass)holder;
            AddAttributeToInheritanceChain(type, new BaseForSubclassingAttribute());
        }

        public static BaseForSubclassingAttribute FromElement(XmlElement elem)
        {
            return new BaseForSubclassingAttribute();
        }
    }

    public class InterfacesForOverridableAttribute : AutoWrapAttribute
    {
        public List<DefClass[]> Interfaces = new List<DefClass[]>();

        private List<string[]> _interfaceNames = new List<string[]>();

        public override void ProcessHolder(AttributeHolder holder)
        {
            DefClass type = (DefClass)holder;

            foreach (string[] names in _interfaceNames)
            {
                List<DefClass> ifaces = new List<DefClass>();
                foreach (string ifacename in names)
                {
                    ifaces.Add(type.FindType<DefClass>(ifacename));
                }
                Interfaces.Add(ifaces.ToArray());
            }
        }

        public InterfacesForOverridableAttribute(string interfaces)
        {
            string[] lists = interfaces.Split('|');
            foreach (string list in lists)
            {
                string[] names = list.Split(',');
                for (int i=0; i < names.Length; i++)
                    names[i] = names[i].Trim();

                _interfaceNames.Add(names);
            }
        }

        public static InterfacesForOverridableAttribute FromElement(XmlElement elem)
        {
            return new InterfacesForOverridableAttribute(elem.InnerText);
        }
    }

    public class CustomDisposingAttribute : AutoWrapAttribute
    {
        public string Text;

        public CustomDisposingAttribute(string decl)
        {
            this.Text = decl;
        }
        public static CustomDisposingAttribute FromElement(XmlElement elem)
        {
            return new CustomDisposingAttribute(elem.InnerText);
        }
    }

    public class DefinitionIndexAttribute : AutoWrapAttribute
    {
        public int Index;

        public DefinitionIndexAttribute(string strIndex)
        {
            this.Index = int.Parse(strIndex);
        }
        public static DefinitionIndexAttribute FromElement(XmlElement elem)
        {
            return new DefinitionIndexAttribute(elem.InnerText);
        }
    }

    /// <summary>
    /// For STL lists that contain objects that can't be checked for equality, thus Remove and Unique methods cause compile errors 
    /// </summary>
    public class STLListNoRemoveAndUniqueAttribute : AutoWrapAttribute
    {
        public static STLListNoRemoveAndUniqueAttribute FromElement(XmlElement elem)
        {
            return new STLListNoRemoveAndUniqueAttribute();
        }
    }

    /// <summary>
    /// For functions that cause ambiguity when defining overloads based on the default values of the parameters
    /// </summary>
    public class NoDefaultParamOverloadsAttribute : AutoWrapAttribute
    {
        public static NoDefaultParamOverloadsAttribute FromElement(XmlElement elem)
        {
            return new NoDefaultParamOverloadsAttribute();
        }
    }

    /// <summary>
    /// Functions that get this do not expose the parameters that have default values. Cannot be used with NoDefaultParamOverloadsAttribute.
    /// </summary>
    public class HideParamsWithDefaultValuesAttribute : AutoWrapAttribute
    {
        public static HideParamsWithDefaultValuesAttribute FromElement(XmlElement elem)
        {
            return new HideParamsWithDefaultValuesAttribute();
        }
    }

    /// <summary>
    /// For parameters that should be exposed as pointers, not out parameters
    /// </summary>
    public class RawPointerParamAttribute : AutoWrapAttribute
    {
        public static RawPointerParamAttribute FromElement(XmlElement elem)
        {
            return new RawPointerParamAttribute();
        }
    }

    /// <summary>
    /// For including finalizer to do the cleaning up
    /// </summary>
    public class DoCleanupInFinalizerAttribute : AutoWrapAttribute
    {
        public static DoCleanupInFinalizerAttribute FromElement(XmlElement elem)
        {
            return new DoCleanupInFinalizerAttribute();
        }
    }

    /// <summary>
    /// For classes that by default get finalizers for cleaning up (like PlainWrapper classes)
    /// </summary>
    public class NoFinalizerAttribute : AutoWrapAttribute
    {
        public static NoFinalizerAttribute FromElement(XmlElement elem)
        {
            return new NoFinalizerAttribute();
        }
    }

    /// <summary>
    /// For classes that are defined manually. The name of the public class must be the same as the native public class and it should
    /// define conversion operators for managed handle -> native pointer and native pointer -> managed handle if needed.
    /// </summary>
    public class CustomClassAttribute : AutoWrapAttribute
    {
        public static CustomClassAttribute FromElement(XmlElement elem)
        {
            return new CustomClassAttribute();
        }
    }

    public class SequentialLayoutAttribute : AutoWrapAttribute {
        public static SequentialLayoutAttribute FromElement(XmlElement elem) {
            return new SequentialLayoutAttribute ();
        }
    }

    public class GlobalScopeAttribute : AutoWrapAttribute {
        public static GlobalScopeAttribute FromElement(XmlElement elem) {
            return new GlobalScopeAttribute ();
        }
    }
}
