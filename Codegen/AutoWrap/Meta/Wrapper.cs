#region GPL license
/*
 * This source file is part of the AutoWrap code generator of the
 * MOGRE project (http://mogre.sourceforge.net), copyright (c) 
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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace AutoWrap.Meta
{
    public class IndentStringBuilder
    {
        string indstr = "";

        public StringBuilder sb = new StringBuilder();

        public IndentStringBuilder()
        {
        }

        public void IncreaseIndent()
        {
            indstr += "\t";
        }
        public void DecreaseIndent()
        {
            indstr = indstr.Substring(1);
        }

        public void Append(string str)
        {
            sb.Append(str);
        }
        public void AppendIndent(string str)
        {
            sb.Append(indstr + str);
        }
        public void AppendLine()
        {
            sb.AppendLine("");
        }
        public void AppendLine(string str)
        {
            sb.AppendLine(AddIndentation(str));
        }
        public void AppendFormat(string str, params object[] args)
        {
            sb.AppendFormat(str, args);
        }
        public void AppendFormatIndent(string str, params object[] args)
        {
            sb.AppendFormat(AddIndentation(str), args);
        }
        public override string ToString()
        {
            return sb.ToString();
        }

        private string AddIndentation(string str)
        {
            string res = indstr + String.Join("\n" + indstr, str.Replace("\r\n", "\n").Split('\n'));
            if (res.EndsWith(indstr)) res = res.Substring(0, res.Length - indstr.Length);
            return res;
        }
    }

    public class IncludeFileWrapEventArgs : EventArgs
    {
        private string _include;

        public string IncludeFile
        {
            get { return _include; }
        }

        public IncludeFileWrapEventArgs(string include)
        {
            _include = include;
        }
    }

    public class Wrapper
    {
        public event EventHandler<IncludeFileWrapEventArgs> IncludeFileWrapped;

        string _includePath;
        string _sourcePath;

        public string NativeNamespace;
        public string ManagedNamespace;
        public List<string> PreDeclarations = new List<string>();
        public List<DefType> PragmaMakePublicTypes = new List<DefType>();
        public List<DefType> UsedTypes = new List<DefType>();
        public SortedList<string, List<DefType>> IncludeFiles = new SortedList<string, List<DefType>>();
        public List<DefClass> Overridables = new List<DefClass>();
        public List<ClassProducer> PostClassProducers = new List<ClassProducer>();
        public List<ClassProducer> PreClassProducers = new List<ClassProducer>();

        public Wrapper(MetaDefinition meta, string includePath, string sourcePath, string mngNamespace, string ntvNamespace)
        {
            this._includePath = includePath;
            this._sourcePath = sourcePath;
            this.ManagedNamespace = mngNamespace;
            this.NativeNamespace = ntvNamespace;

            foreach (DefNameSpace space in meta.NameSpaces)
            {
                foreach (DefType type in space.Types)
                {

                    if (TypeIsWrappable(type))
                    {
                        List<DefType> list;
                        if (!IncludeFiles.TryGetValue(type.IncludeFile, out list))
                        {
                            list = new List<DefType>();
                            IncludeFiles.Add(type.IncludeFile, list);
                        }

                        if (type is DefEnum || Producer.IsInternalTypeDef(type))
                        {
                            list.Insert(0, type);
                        }
                        else if (type.HasWrapType(WrapTypes.NativePtrValueType))
                        {
                            //put it after enums and before other classes
                            int i;
                            for (i = 0; i < list.Count; i++)
                            {
                                if (!(type is DefEnum || Producer.IsInternalTypeDef(type)))
                                    break;
                            }

                            list.Insert(i, type);
                        }
                        else
                            list.Add(type);

                        if (type.HasWrapType(WrapTypes.Overridable))
                            Overridables.Add((DefClass)type);
                    }
                }

                foreach (DefType type in space.Types)
                {
                    if (type is DefEnum && IncludeFiles.ContainsKey(type.IncludeFile))
                        if (!IncludeFiles[type.IncludeFile].Contains(type))
                            IncludeFiles[type.IncludeFile].Insert(0, type);
                }
            }
        }

        public bool TypeIsWrappable(DefType type)
        {
            if (type.Name.StartsWith("DLL_"))
            {
                //It's DLL function pointers of OgrePlatformManager.h
                return false;
            }
            
            // For now, ignore un-named enums in the namespace
            if(type.Name.StartsWith("@")) {
                return false;
            }

            // Get explicit type or a new type if type has ReplaceBy attribute
            type = (type.IsNested) ? type.ParentClass.FindType<DefType>(type.Name) : type.NameSpace.FindType<DefType>(type.Name);

            if (type.HasAttribute<CustomIncClassDefinitionAttribute>())
                return true;

            if (type.IsIgnored)
                return false;

            if (type.HasAttribute<WrapTypeAttribute>())
                return true;

            if (type.IsSharedPtr)
            {
                type.Attributes.Add(new WrapTypeAttribute(WrapTypes.SharedPtr));
                return true;
            }
            else if (type is DefClass)
            {
                DefClass cls = type as DefClass;
                if (cls.HasAttribute<CLRObjectAttribute>(true))
                {
                    if (cls.HasAttribute<OverridableAttribute>(true))
                        cls.Attributes.Add(new WrapTypeAttribute(WrapTypes.Overridable));
                    else
                        cls.Attributes.Add(new WrapTypeAttribute(WrapTypes.NonOverridable));
                    return true;
                }

                if (cls.IsSingleton)
                {
                    cls.Attributes.Add(new WrapTypeAttribute(WrapTypes.Singleton));
                    return true;
                }

                return false;
            }
            else if (type is DefTypeDef)
            {
                if (type.IsSTLContainer)
                {
                    foreach (ITypeMember m in (type as DefTypeDef).TypeMembers)
                    {
                        DefType mt = m.Type;
                        if (!mt.IsValueType && !mt.IsPureManagedClass
                            && !TypeIsWrappable(mt))
                            return false;
                    }

                    return true;
                }
                else if (type is DefIterator)
                {
                    if (TypeIsWrappable((type as DefIterator).TypeMembers[0].Type))
                    {
                        if ((type as DefIterator).IsConstIterator)
                        {
                            try
                            {
                                DefType notconst = type.FindType<DefType>(type.Name.Substring("Const".Length), true);
                                return false;
                            }
                            catch
                            {
                                return true;
                            }
                        }
                        else
                            return true;
                    }
                    else
                        return false;
                }
                else if ((type as DefTypeDef).BaseType is DefInternal
                         || (type as DefTypeDef).BaseType.HasAttribute<ValueTypeAttribute>())
                    return true;
                else
                    return TypeIsWrappable((type as DefTypeDef).BaseType);
            }
            else
                return false;
        }

        public void ProduceCodeFiles()
        {
            PreDeclarations.Clear();
            PragmaMakePublicTypes.Clear();

            foreach (string include in IncludeFiles.Keys)
            {
                if (include == "OgreCamera.h")
                {

                }

                string wrapFile = include.Replace(".h", "");
                wrapFile = GetManagedIncludeFileName(wrapFile);

                string incFile = _includePath + "\\" + wrapFile + ".h";
                string cppFile = _sourcePath + "\\" + wrapFile + ".cpp";

                using (StreamWriter write = new StreamWriter(new MemoryStream()))
                {
                    write.Write(Copyright);
                    write.Write(CreateIncludeCodeForIncludeFile(include));
                    write.Flush();
                    WriteToFile(write.BaseStream as MemoryStream, incFile);
                }

                bool hasContent;
                string txt = CreateCppCodeForIncludeFile(include, out hasContent);
                if (hasContent)
                {
                    using (StreamWriter write = new StreamWriter(new MemoryStream()))
                    {
                        write.Write(Copyright);
                        write.Write(txt);
                        write.Flush();
                        WriteToFile(write.BaseStream as MemoryStream, cppFile);
                    }
                }

                IncludeFileWrapped(this, new IncludeFileWrapEventArgs(include));
            }

            //Create PreDeclarations.h

            using (StreamWriter write = new StreamWriter(new MemoryStream()))
            {
                foreach (string decl in PreDeclarations)
                {
                    write.WriteLine(decl);
                }

                write.Flush();
                WriteToFile(write.BaseStream as MemoryStream, _includePath + "\\PreDeclarations.h");
            }

            //Create MakePublicDeclarations.h

            using (StreamWriter write = new StreamWriter(new MemoryStream()))
            {
                List<DefType> typesForMakePublic = new List<DefType>();

                foreach (DefType t in PragmaMakePublicTypes)
                {
                    if (t is DefClass && !t.IsNested && !t.IsIgnored)
                    {
                        DefType type = t.FindType<DefType>(t.Name);

                        if (type.FullNativeName.StartsWith(NativeNamespace + "::")
                            && type is DefClass && !type.IsTemplate)
                        {
                            if (!typesForMakePublic.Contains(type))
                                typesForMakePublic.Add(type);
                        }
                    }
                }

                write.WriteLine("#pragma once");
                write.WriteLine();

                write.WriteLine("namespace " + NativeNamespace);
                write.WriteLine("{");

                foreach (DefType type in typesForMakePublic)
                {
                    if (type is DefStruct)
                        write.Write("struct ");
                    else
                        write.Write("class ");
                    write.WriteLine(type.Name + ";");
                }

                write.WriteLine("}");
                write.WriteLine();

                foreach (DefType type in typesForMakePublic)
                {
                    write.WriteLine("#pragma make_public( " + type.FullNativeName + " )");
                }

                write.Flush();
                WriteToFile(write.BaseStream as MemoryStream, _includePath + "\\MakePublicDeclarations.h");
            }

            // Create CLRObjects.inc

            using (StreamWriter write = new StreamWriter(new MemoryStream()))
            {
                List<DefClass> clrObjs = new List<DefClass>();

                foreach (string include in IncludeFiles.Keys)
                {
                    foreach (DefType t in IncludeFiles[include])
                        AddCLRObjects(t, clrObjs);
                }

                foreach (DefClass cls in clrObjs)
                {
                    string name = cls.Name;
                    DefClass parent = cls;
                    while (parent.ParentClass != null)
                    {
                        parent = parent.ParentClass;
                        name = parent.Name + "_" + name;
                    }

                    write.WriteLine("CLROBJECT( " + name + " )");
                }

                write.Flush();
                WriteToFile(write.BaseStream as MemoryStream, _includePath + "\\CLRObjects.inc");
            }
        }

        public string GetInitCLRObjectFuncSignature(DefClass cls) {
            if (!cls.HasAttribute<CLRObjectAttribute>(true))
                throw new Exception("class is not subclass of CLRObject");

            string name = cls.Name;
            DefClass parent = cls;
            while (parent.ParentClass != null)
            {
                parent = parent.ParentClass;
                name = parent.Name + "_" + name;
            }

            return "void _Init_CLRObject_" + name + "(CLRObject* pClrObj)";
        }

        void AddCLRObjects(DefType t, List<DefClass> clrObjs) {
            DefClass cls = t as DefClass;
            if (cls == null)
                return;
            if (cls.IsIgnored || cls.ProtectionType != ProtectionType.Public)
                return;

            if (cls.HasAttribute<CLRObjectAttribute>(true))
                clrObjs.Add(cls);

            foreach (DefType nested in cls.NestedTypes)
                AddCLRObjects(nested, clrObjs);
        }

        public void ProduceSubclassCodeFiles(System.Windows.Forms.ProgressBar bar)
        {
            bar.Minimum = 0;
            bar.Maximum = Overridables.Count;
            bar.Step = 1;
            bar.Value = 0;

            foreach (DefClass type in Overridables)
            {
                string wrapFile = "Subclass" + type.Name;
                string incFile = _includePath + "\\" + wrapFile + ".h";
                string cppFile = _sourcePath + "\\" + wrapFile + ".cpp";

                using (StreamWriter write = new StreamWriter(new MemoryStream()))
                {
                    write.Write(Copyright);
                    write.Write(CreateIncludeCodeForOverridable(type));
                    write.Flush();
                    WriteToFile(write.BaseStream as MemoryStream, incFile);
                }

                using (StreamWriter write = new StreamWriter(new MemoryStream()))
                {
                    write.Write(Copyright);
                    write.Write(CreateCppCodeForOverridable(type));
                    write.Flush();
                    WriteToFile(write.BaseStream as MemoryStream, cppFile);
                }

                bar.Value++;
                bar.Refresh();
            }
        }

        protected void WriteToFile(MemoryStream memStream, string file)
        {
            if (File.Exists(file))
            {
                string memcontent = Encoding.UTF8.GetString(memStream.ToArray());
                string filecontent;
                using (StreamReader inp = new StreamReader(file, Encoding.UTF8))
                {
                    filecontent = inp.ReadToEnd();
                }

                if (memcontent == filecontent)
                    return;
            }

            using (StreamWriter writer = File.CreateText(file))
            {
                memStream.WriteTo(writer.BaseStream);
            }
        }

        protected string GetManagedIncludeFileName(string name)
        {
            name = name.Replace('/', '_').Replace('\\', '_');
            if (name.StartsWith(NativeNamespace))
                name = ManagedNamespace + name.Substring(NativeNamespace.Length);
            else
                name = ManagedNamespace + "-" + name;

            return name;
        }

        public string CreateIncludeCodeForIncludeFile(string include)
        {
            UsedTypes.Clear();

            PreClassProducers.Clear();
            PostClassProducers.Clear();

            IndentStringBuilder sbTypes = new IndentStringBuilder();
            foreach (DefType t in IncludeFiles[include])
            {
                IncAddType(t, sbTypes);
            }

            foreach (ClassProducer producer in PostClassProducers)
            {
                producer.Add();
            }

            foreach (ClassProducer producer in PreClassProducers)
            {
                producer.AddFirst();
            }

            IndentStringBuilder sb = new IndentStringBuilder();
            sb.AppendLine("#pragma once\n");

            IncAddIncludeFiles(include, UsedTypes, sb);

            sb.AppendFormat("namespace {0}\n{{\n", ManagedNamespace);

            sb.IncreaseIndent();
            sb.AppendLine(sbTypes.ToString());
            sb.DecreaseIndent();

            sb.AppendLine("}");

            return sb.ToString().Replace("\r","");
        }

        public string CreateCppCodeForIncludeFile(string include, out bool hasContent)
        {
            UsedTypes.Clear();

            PreClassProducers.Clear();
            PostClassProducers.Clear();

            IndentStringBuilder contentsb = new IndentStringBuilder();
            foreach (DefType t in IncludeFiles[include])
            {
                CppAddType(t, contentsb);
            }

            foreach (ClassProducer producer in PostClassProducers)
            {
                producer.Add();
            }

            foreach (ClassProducer producer in PreClassProducers)
            {
                producer.AddFirst();
            }

            IndentStringBuilder sb = new IndentStringBuilder();
            hasContent = false;

            CppAddIncludeFiles(include, UsedTypes, sb);

            sb.AppendFormat("namespace {0}\n{{\n", ManagedNamespace);

            sb.IncreaseIndent();

            string txt = contentsb.ToString();
            if (txt != "")
            {
                hasContent = true;
                sb.AppendLine(txt);
            }

            sb.DecreaseIndent();

            sb.AppendLine("}");

            return sb.ToString().Replace("\r", "");
        }

        public string CreateIncludeCodeForOverridable(DefClass type)
        {
            UsedTypes.Clear();

            PreClassProducers.Clear();
            PostClassProducers.Clear();

            IndentStringBuilder sbTypes = new IndentStringBuilder();

            new IncSubclassingClassProducer(this, type, sbTypes, null).Add();
            if (type.HasAttribute<InterfacesForOverridableAttribute>())
            {
                List<DefClass[]> interfaces = type.GetAttribute<InterfacesForOverridableAttribute>().Interfaces;
                foreach (DefClass[] ifaces in interfaces)
                {
                    new IncSubclassingClassProducer(this, type, sbTypes, ifaces).Add();
                }
            }

            foreach (ClassProducer producer in PostClassProducers)
            {
                if (!(producer is NativeProtectedTypesProxy)
                    && !(producer is NativeProtectedStaticsProxy))
                    producer.Add();
            }

            foreach (ClassProducer producer in PreClassProducers)
            {
                if (!(producer is NativeProtectedTypesProxy)
                    && !(producer is NativeProtectedStaticsProxy))
                    producer.AddFirst();
            }

            IndentStringBuilder sb = new IndentStringBuilder();
            sb.AppendLine("#pragma once\n");

            sb.AppendFormat("namespace {0}\n{{\n", ManagedNamespace);

            sb.IncreaseIndent();
            sb.AppendLine(sbTypes.ToString());
            sb.DecreaseIndent();

            sb.AppendLine("}");

            return sb.ToString().Replace("\r", "");
        }

        public string CreateCppCodeForOverridable(DefClass type)
        {
            IndentStringBuilder sb = new IndentStringBuilder();

            sb.AppendLine("#include \"MogreStableHeaders.h\"");
            sb.AppendLine("#include \"Subclass" + type.Name + ".h\"\n");
            
            sb.AppendFormat("namespace {0}\n{{\n", ManagedNamespace);

            sb.IncreaseIndent();

            PreClassProducers.Clear();
            PostClassProducers.Clear();

            new CppSubclassingClassProducer(this, type, sb, null).Add();
            if (type.HasAttribute<InterfacesForOverridableAttribute>())
            {
                List<DefClass[]> interfaces = type.GetAttribute<InterfacesForOverridableAttribute>().Interfaces;
                foreach (DefClass[] ifaces in interfaces)
                {
                    new CppSubclassingClassProducer(this, type, sb, ifaces).Add();
                }
            }

            foreach (ClassProducer producer in PostClassProducers)
            {
                if (!(producer is NativeProtectedTypesProxy)
                    && !(producer is NativeProtectedStaticsProxy))
                    producer.Add();
            }

            foreach (ClassProducer producer in PreClassProducers)
            {
                if ( !(producer is NativeProtectedTypesProxy)
                    && !(producer is NativeProtectedStaticsProxy))
                    producer.AddFirst();
            }

            sb.DecreaseIndent();

            sb.AppendLine("}");

            return sb.ToString().Replace("\r", "");
        }

        public void IncAddType(DefType t, IndentStringBuilder sb)
        {
            if (t.HasAttribute<CustomIncClassDefinitionAttribute>())
            {
                string txt = t.GetAttribute<CustomIncClassDefinitionAttribute>().Text;
                sb.AppendLine(txt);
                return;
            }

            if (t is DefClass)
            {
                if (!t.HasAttribute<WrapTypeAttribute>())
                {
                    //Ignore
                }
                else
                {
                    switch (t.GetAttribute<WrapTypeAttribute>().WrapType)
                    {
                        case WrapTypes.NonOverridable:
                            new IncNonOverridableClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.Overridable:
                            new IncOverridableClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.NativeDirector:
                            new IncNativeDirectorClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.Interface:
                            new IncInterfaceClassProducer(this, t as DefClass, sb).Add();
                            new IncOverridableClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.Singleton:
                            new IncSingletonClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.ReadOnlyStruct:
                            new IncReadOnlyStructClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.ValueType:
                            new IncValueClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.NativePtrValueType:
                            new IncNativePtrValueClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.CLRHandle:
                            new IncCLRHandleClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.PlainWrapper:
                            new IncPlainWrapperClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.SharedPtr:
                            IncAddSharedPtrType(t, sb);
                            break;
                    }
                }
            }
            else if (t is DefEnum)
            {
                IncAddEnum(t as DefEnum, sb);
            }
            else if (t is DefTypeDef)
            {
                DefTypeDef explicitType;

                if (t.IsUnnamedSTLContainer)
                    explicitType = t as DefTypeDef;
                else
                    explicitType = (t.IsNested) ? t.ParentClass.FindType<DefTypeDef>(t.Name) : t.NameSpace.FindType<DefTypeDef>(t.Name);

                if (t.HasWrapType(WrapTypes.SharedPtr))
                {
                    IncAddSharedPtrType(t, sb);
                }
                else if (explicitType.IsSTLContainer)
                {
                    IncAddSTLContainer(explicitType, sb);
                }
                else if (explicitType is DefIterator)
                {
                    IncAddIterator(explicitType as DefIterator, sb);
                }
                else if (explicitType.BaseType is DefInternal
                    || (explicitType.Name != "String" && explicitType.Name != "UTFString" && explicitType.IsTypedefOfInternalType))
                {
                    IncAddInternalTypeDef(explicitType, sb);
                }
                else if (explicitType.BaseType.HasAttribute<ValueTypeAttribute>())
                {
                    IncAddValueTypeTypeDef(explicitType, sb);
                }
            }
        }

        public void CppAddType(DefType t, IndentStringBuilder sb)
        {
            if (t.HasAttribute<CustomCppClassDefinitionAttribute>())
            {
                string txt = t.GetAttribute<CustomCppClassDefinitionAttribute>().Text;
                sb.AppendLine(txt);
                return;
            }

            if (t is DefClass)
            {
                if (!t.HasAttribute<WrapTypeAttribute>())
                {
                    //Ignore
                }
                else
                {
                    switch (t.GetAttribute<WrapTypeAttribute>().WrapType)
                    {
                        case WrapTypes.NonOverridable:
                            new CppNonOverridableClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.Overridable:
                            new CppOverridableClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.Interface:
                            new CppOverridableClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.NativeDirector:
                            new CppNativeDirectorClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.NativePtrValueType:
                            new CppNativePtrValueClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.Singleton:
                            new CppSingletonClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.CLRHandle:
                            new CppCLRHandleClassProducer(this, t as DefClass, sb).Add();
                            break;
                        case WrapTypes.PlainWrapper:
                            new CppPlainWrapperClassProducer(this, t as DefClass, sb).Add();
                            break;
                    }
                }
            }
            else if (t is DefTypeDef)
            {
                DefTypeDef explicitType;

                if (t.IsUnnamedSTLContainer)
                    explicitType = t as DefTypeDef;
                else
                    explicitType = (t.IsNested) ? t.ParentClass.FindType<DefTypeDef>(t.Name) : t.NameSpace.FindType<DefTypeDef>(t.Name);

                if (explicitType.IsSTLContainer)
                {
                    CppAddSTLContainer(explicitType, sb);
                }
                else if (explicitType is DefIterator)
                {
                    CppAddIterator(explicitType as DefIterator, sb);
                }
            }
        }

        //public void IncAddEnum(DefEnum enm, IndentStringBuilder sb)
        //{
        //    IncAddEnum(enm, sb, false);
        //}
        public void IncAddEnum(DefEnum enm, IndentStringBuilder sb) //, bool inProtectedTypesProxy)
        {
            if (enm.Name[0] == '@')
                return;

            if (enm.HasAttribute<FlagsEnumAttribute>())
            {
                sb.AppendLine("[Flags]");
            }

            sb.AppendIndent("");
            if (!enm.IsNested)
                sb.Append("public ");
            else
                sb.Append(Producer.GetProtectionString(enm.ProtectionType) + ": ");

            //if (inProtectedTypesProxy)
                //sb.Append("enum " + enm.Name + "\n");
            //else
                sb.Append("enum class " + enm.CLRName + "\n");

            sb.AppendLine("{");
            sb.IncreaseIndent();
            for (int i=0; i < enm.CLREnumValues.Length; i++)
            {
                string value = enm.NativeEnumValues[i];
                sb.AppendIndent("");
                //if (inProtectedTypesProxy)
                //{
                //    value = enm.ParentFullNativeName + "::" + enm.CLREnumValues[i];
                //    sb.Append("PUBLIC_");
                //}

                sb.Append(enm.CLREnumValues[i] + " = " + value);
                if (i < enm.CLREnumValues.Length - 1)
                    sb.Append(",");
                sb.Append("\n");
            }
            sb.DecreaseIndent();
            sb.AppendLine("};\n");
        }

        private void IncAddSharedPtrType(DefType type, IndentStringBuilder sb)
        {
            if (!type.Name.EndsWith("Ptr"))
                throw new Exception("SharedPtr class that doesn't have a name ending to 'Ptr'");

            string basename = null;
            if (type is DefClass)
                basename = (type as DefClass).Inherits[0];
            else
                basename = (type as DefTypeDef).BaseTypeName;

            int s = basename.IndexOf("<");
            int e = basename.LastIndexOf(">");
            string baseClass = basename.Substring(s + 1, e - s - 1).Trim();
            //string nativeClass = _nativePrefix + "::" + baseClass;
            DefType baseType = type.FindType<DefType>(baseClass);
            string nativeClass = baseType.FullNativeName;

            string className = type.FullCLRName;
            if (className.Contains("::"))
                className = className.Substring(className.IndexOf("::") + 2);

            if (!type.IsNested)
            {
                PreDeclarations.Add("ref class " + type.Name + ";");
                sb.AppendIndent("public ");
            }
            else
            {
                sb.AppendIndent(Producer.GetProtectionString(type.ProtectionType) + ": ");
            }

            sb.Append("ref class " + type.Name + " : public " + baseClass + "\n");
            sb.AppendLine("{");
            sb.AppendLine("public protected:");
            sb.IncreaseIndent();
            sb.AppendLine("\t" + type.FullNativeName + "* _sharedPtr;");
            sb.AppendLine();
            sb.AppendLine(type.Name + "(" + type.FullNativeName + "& sharedPtr) : " + baseClass + "( sharedPtr.getPointer() )");
            sb.AppendLine("{");
            sb.AppendLine("\t_sharedPtr = new " + type.FullNativeName + "(sharedPtr);");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("!" + type.Name + "()");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine("if (_sharedPtr != 0)");
            sb.AppendLine("{");
            sb.AppendLine("\tdelete _sharedPtr;");
            sb.AppendLine("\t_sharedPtr = 0;");
            sb.AppendLine("}");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("~" + type.Name + "()");
            sb.AppendLine("{");
            sb.AppendLine("\tthis->!" + type.Name + "();");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.DecreaseIndent();
            sb.AppendLine("public:");
            sb.IncreaseIndent();

            sb.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_SHAREDPTR( " + className + " )");
            sb.AppendLine();

            if (type is DefClass)
            {
                DefClass realType = type.FindType<DefClass>(baseClass, false);
                if (realType != null && realType.BaseClass != null && realType.BaseClass.Name == "Resource")
                {
                    // For Resource subclasses (Material etc.) allow implicit conversion of ResourcePtr (i.e ResourcePtr -> MaterialPtr)

                    AddTypeDependancy(realType.BaseClass);

                    sb.AppendLine("static " + type.Name + "^ FromResourcePtr( ResourcePtr^ ptr )");
                    sb.AppendLine("{");
                    sb.AppendLine("\treturn (" + type.Name + "^) ptr;");
                    sb.AppendLine("}");
                    sb.AppendLine();

                    sb.AppendLine("static operator " + type.Name + "^ ( ResourcePtr^ ptr )");
                    sb.AppendLine("{");
                    sb.IncreaseIndent();
                    sb.AppendLine("if (CLR_NULL == ptr) return nullptr;");
                    sb.AppendLine("void* castptr = dynamic_cast<" + nativeClass + "*>(ptr->_native);");
                    sb.AppendLine("if (castptr == 0) throw gcnew InvalidCastException(\"The underlying type of the ResourcePtr object is not of type " + baseClass + ".\");");
                    sb.AppendLine("return gcnew " + type.Name + "( (" + type.FullNativeName + ") *(ptr->_sharedPtr) );");
                    sb.DecreaseIndent();
                    sb.AppendLine("}");
                    sb.AppendLine();
                }
            }

            //sb.AppendLine(type.Name + "() : " + baseClass + "( (" + nativeClass + "*) 0 )");
            //sb.AppendLine("{");
            //sb.AppendLine("\t_sharedPtr = new " + type.FullNativeName + "();");
            //sb.AppendLine("}");
            //sb.AppendLine();
            if (baseType is DefClass && (baseType as DefClass).IsInterface)
            {
                string proxyName = NativeProxyClassProducer.GetProxyName(baseType as DefClass);
                sb.AppendLine(type.Name + "(" + baseType.CLRName + "^ obj) : " + baseClass + "( static_cast<" + proxyName + "*>( (" + nativeClass + "*)obj ) )");
                sb.AppendLine("{");
                sb.AppendLine("\t_sharedPtr = new " + type.FullNativeName + "( static_cast<" + proxyName + "*>(obj->_native) );");
                sb.AppendLine("}");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine(type.Name + "(" + baseClass + "^ obj) : " + baseClass + "( obj->_native )");
                sb.AppendLine("{");
                sb.AppendLine("\t_sharedPtr = new " + type.FullNativeName + "( static_cast<" + nativeClass + "*>(obj->_native) );");
                sb.AppendLine("}");
                sb.AppendLine();
            }
            //sb.AppendLine("void Bind(" + baseClass + "^ obj)");
            //sb.AppendLine("{");
            //sb.AppendLine("\t(*_sharedPtr).bind( static_cast<" + nativeClass + "*>(obj->_native) );");
            //sb.AppendLine("}");
            //sb.AppendLine();

            sb.AppendLine("virtual bool Equals(Object^ obj) override");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine(type.Name + "^ clr = dynamic_cast<" + type.Name + "^>(obj);");
            sb.AppendLine("if (clr == CLR_NULL)");
            sb.AppendLine("{");
            sb.AppendLine("\treturn false;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("return (_native == clr->_native);");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendLine("bool Equals(" + type.Name + "^ obj)");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine("if (obj == CLR_NULL)");
            sb.AppendLine("{");
            sb.AppendLine("\treturn false;");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("return (_native == obj->_native);");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("static bool operator == (" + type.Name + "^ val1, " + type.Name + "^ val2)");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine("if ((Object^)val1 == (Object^)val2) return true;");
            sb.AppendLine("if ((Object^)val1 == nullptr || (Object^)val2 == nullptr) return false;");
            sb.AppendLine("return (val1->_native == val2->_native);");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("static bool operator != (" + type.Name + "^ val1, " + type.Name + "^ val2)");
            sb.AppendLine("{");
            sb.AppendLine("\treturn !(val1 == val2);");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("virtual int GetHashCode() override");
            sb.AppendLine("{");
            sb.AppendLine("\treturn reinterpret_cast<int>( _native );");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("property IntPtr NativePtr");
            sb.AppendLine("{");
            sb.AppendLine("\tIntPtr get() { return (IntPtr)_sharedPtr; }");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("property bool Unique");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine("bool get()");
            sb.AppendLine("{");
            sb.AppendLine("\treturn (*_sharedPtr).unique();");
            sb.AppendLine("}");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("property int UseCount");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine("int get()");
            sb.AppendLine("{");
            sb.AppendLine("\treturn (*_sharedPtr).useCount();");
            sb.AppendLine("}");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendLine();
            //sb.AppendLine("void SetNull()");
            //sb.AppendLine("{");
            //sb.AppendLine("\t(*_sharedPtr).setNull();");
            //sb.AppendLine("\t_native = 0;");
            //sb.AppendLine("}");
            //sb.AppendLine();
            //sb.AppendLine("property bool IsNull");
            //sb.AppendLine("{");
            //sb.IncreaseIndent();
            //sb.AppendLine("bool get()");
            //sb.AppendLine("{");
            //sb.AppendLine("\treturn (*_sharedPtr).isNull();");
            //sb.AppendLine("}");
            //sb.DecreaseIndent();
            //sb.AppendLine("}");
            //sb.AppendLine();
            sb.AppendLine("property " + baseClass + "^ Target");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine(baseClass + "^ get()");
            sb.AppendLine("{");
            sb.AppendLine("\treturn static_cast<" + nativeClass + "*>(_native);");
            sb.AppendLine("}");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.DecreaseIndent();
            sb.AppendLine("};\n\n");
        }

        public void IncAddSTLContainer(DefTypeDef t, IndentStringBuilder sb)
        {
            if (t is DefStdPair)
            {
                //sb.AppendIndent("");
                //if (t.IsNested)
                //    sb.Append(Producer.GetProtectionString(t.ProtectionType) + ": ");
                //sb.Append("typedef " + t.FullSTLContainerTypeName + " " + t.CLRName + ";\n\n");
                return;
            }

            if (!t.IsNested)
            {
                this.AddPreDeclaration("ref class " + t.CLRName + ";");
                this.AddPreDeclaration("ref class Const_" + t.CLRName + ";");
            }

            if (t is DefTemplateOneType)
            {
                if (t.HasAttribute<STLListNoRemoveAndUniqueAttribute>())
                {
                    sb.AppendLine("#undef INC_STLLIST_DEFINE_REMOVE_AND_UNIQUE");
                    sb.AppendLine("#define INC_STLLIST_DEFINE_REMOVE_AND_UNIQUE(M)");
                    sb.AppendLine();
                }

                sb.AppendLine("#define STLDECL_MANAGEDTYPE " + t.TypeMembers[0].CLRTypeName);
                sb.AppendLine("#define STLDECL_NATIVETYPE " + t.TypeMembers[0].NativeTypeName);
                CheckTypeForDependancy(t.TypeMembers[0].Type);
            }
            else if (t is DefTemplateTwoTypes)
            {
                sb.AppendLine("#define STLDECL_MANAGEDKEY " + t.TypeMembers[0].CLRTypeName);
                sb.AppendLine("#define STLDECL_MANAGEDVALUE " + t.TypeMembers[1].CLRTypeName);
                sb.AppendLine("#define STLDECL_NATIVEKEY " + t.TypeMembers[0].NativeTypeName);
                sb.AppendLine("#define STLDECL_NATIVEVALUE " + t.TypeMembers[1].NativeTypeName);
                CheckTypeForDependancy(t.TypeMembers[0].Type);
                CheckTypeForDependancy(t.TypeMembers[1].Type);
            }

            sb.AppendIndent("");
            string publicprot, privateprot;
            if (!t.IsNested)
            {
                publicprot = "public";
                privateprot = "private";
            }
            else
            {
                publicprot = Producer.GetProtectionString(t.ProtectionType) + ": ";
                privateprot = "private:";
                sb.Append(publicprot);
            }

            sb.Append("INC_DECLARE_STL" + t.STLContainer.ToUpper());

            if (t.IsReadOnly)
                sb.Append("_READONLY");

            sb.Append("( " + t.CLRName);

            if (t is DefTemplateOneType)
                sb.Append(", STLDECL_MANAGEDTYPE, STLDECL_NATIVETYPE, " + publicprot + ", " + privateprot + " )\n");
            else if (t is DefTemplateTwoTypes)
                sb.Append(", STLDECL_MANAGEDKEY, STLDECL_MANAGEDVALUE, STLDECL_NATIVEKEY, STLDECL_NATIVEVALUE, " + publicprot + ", " + privateprot + " )\n");
            else
                throw new Exception("Unexpected");

            if (t is DefTemplateOneType)
            {
                sb.AppendLine("#undef STLDECL_MANAGEDTYPE");
                sb.AppendLine("#undef STLDECL_NATIVETYPE");

                if (t.HasAttribute<STLListNoRemoveAndUniqueAttribute>())
                {
                    sb.AppendLine();
                    sb.AppendLine("#undef INC_STLLIST_DEFINE_REMOVE_AND_UNIQUE");
                    sb.AppendLine("#define INC_STLLIST_DEFINE_REMOVE_AND_UNIQUE(M)    INC_STLLIST_REMOVE_AND_UNIQUE_DEFINITIONS(M)");
                }
            }
            else if (t is DefTemplateTwoTypes)
            {
                sb.AppendLine("#undef STLDECL_MANAGEDKEY");
                sb.AppendLine("#undef STLDECL_MANAGEDVALUE");
                sb.AppendLine("#undef STLDECL_NATIVEKEY");
                sb.AppendLine("#undef STLDECL_NATIVEVALUE");
            }

            sb.AppendLine();
        }

        public void CppAddSTLContainer(DefTypeDef t, IndentStringBuilder sb)
        {
            if (t is DefStdPair)
            {
                return;
            }

            if (t is DefTemplateOneType)
            {
                if (t.HasAttribute<STLListNoRemoveAndUniqueAttribute>())
                {
                    sb.AppendLine("#undef CPP_STLLIST_DEFINE_REMOVE_AND_UNIQUE");
                    sb.AppendLine("#define CPP_STLLIST_DEFINE_REMOVE_AND_UNIQUE(PREFIX,CLASS_NAME,M,N)");
                    sb.AppendLine();
                }

                sb.AppendLine("#define STLDECL_MANAGEDTYPE " + t.TypeMembers[0].CLRTypeName);
                sb.AppendLine("#define STLDECL_NATIVETYPE " + t.TypeMembers[0].NativeTypeName);
                CppCheckTypeForDependancy(t.TypeMembers[0].Type);
            }
            else if (t is DefTemplateTwoTypes)
            {
                sb.AppendLine("#define STLDECL_MANAGEDKEY " + t.TypeMembers[0].CLRTypeName);
                sb.AppendLine("#define STLDECL_MANAGEDVALUE " + t.TypeMembers[1].CLRTypeName);
                sb.AppendLine("#define STLDECL_NATIVEKEY " + t.TypeMembers[0].NativeTypeName);
                sb.AppendLine("#define STLDECL_NATIVEVALUE " + t.TypeMembers[1].NativeTypeName);
                CppCheckTypeForDependancy(t.TypeMembers[0].Type);
                CppCheckTypeForDependancy(t.TypeMembers[1].Type);
            }

            sb.AppendIndent("CPP_DECLARE_STL" + t.STLContainer.ToUpper());

            if (t.IsReadOnly)
                sb.Append("_READONLY");

            string prefix;
            if (!t.IsNested)
            {
                prefix = t.FullNativeName;
                prefix = prefix.Substring(0, prefix.LastIndexOf("::"));
            }
            else
            {
                prefix = t.ParentClass.FullNativeName;
            }

            if (prefix.Contains("::"))
                prefix = prefix.Substring(prefix.IndexOf("::") + 2) + "::";
            else
                prefix = "";

            sb.Append("( " + prefix + ", " + t.CLRName);

            if (t is DefTemplateOneType)
                sb.Append(", STLDECL_MANAGEDTYPE, STLDECL_NATIVETYPE )\n");
            else if (t is DefTemplateTwoTypes)
                sb.Append(", STLDECL_MANAGEDKEY, STLDECL_MANAGEDVALUE, STLDECL_NATIVEKEY, STLDECL_NATIVEVALUE )\n");
            else
                throw new Exception("Unexpected");

            if (t is DefTemplateOneType)
            {
                sb.AppendLine("#undef STLDECL_MANAGEDTYPE");
                sb.AppendLine("#undef STLDECL_NATIVETYPE");

                if (t.HasAttribute<STLListNoRemoveAndUniqueAttribute>())
                {
                    sb.AppendLine();
                    sb.AppendLine("#undef CPP_STLLIST_DEFINE_REMOVE_AND_UNIQUE");
                    sb.AppendLine("#define CPP_STLLIST_DEFINE_REMOVE_AND_UNIQUE(PREFIX,CLASS_NAME,M,N)    CPP_STLLIST_REMOVE_AND_UNIQUE_DEFINITIONS(PREFIX,CLASS_NAME,M,N)");
                }
            }
            else if (t is DefTemplateTwoTypes)
            {
                sb.AppendLine("#undef STLDECL_MANAGEDKEY");
                sb.AppendLine("#undef STLDECL_MANAGEDVALUE");
                sb.AppendLine("#undef STLDECL_NATIVEKEY");
                sb.AppendLine("#undef STLDECL_NATIVEVALUE");
            }

            sb.AppendLine();
        }

        public void IncAddIterator(DefIterator t, IndentStringBuilder sb)
        {
            if (!t.IsNested)
            {
                this.AddPreDeclaration("ref class " + t.CLRName + ";");
            }

            CheckTypeForDependancy(t.IterationElementTypeMember.Type);

            if (t.IsMapIterator)
                CheckTypeForDependancy(t.IterationKeyTypeMember.Type);

            sb.AppendIndent(Producer.GetProtectionString(t.ProtectionType));
            if (t.IsNested) sb.Append(":");

            if (t.IsMapIterator)
                sb.Append(" INC_DECLARE_MAP_ITERATOR");
            else
                sb.Append(" INC_DECLARE_ITERATOR");

            if (t.TypeMembers[0].Type.ProtectionType == ProtectionType.Protected
                && !t.TypeMembers[0].Type.ParentClass.AllowVirtuals)
            {
                // the container type will not be declared,
                // declare an iterator without a constructor that takes a container class
                sb.Append("_NOCONSTRUCTOR");
            }

            if (t.IsMapIterator)
                sb.Append("( " + t.CLRName + ", " + t.FullNativeName + ", " + t.TypeMembers[0].Type.FullCLRName + ", " + t.IterationElementTypeMember.CLRTypeName + ", " + t.IterationElementTypeMember.NativeTypeName + ", " + t.IterationKeyTypeMember.CLRTypeName + ", " + t.IterationKeyTypeMember.NativeTypeName + " )\n");
            else
                sb.Append("( " + t.CLRName + ", " + t.FullNativeName + ", " + t.TypeMembers[0].Type.FullCLRName + ", " + t.IterationElementTypeMember.CLRTypeName + ", " + t.IterationElementTypeMember.NativeTypeName + " )\n");

            sb.AppendLine();
        }

        public void CppAddIterator(DefIterator t, IndentStringBuilder sb)
        {
            string prefix;
            if (!t.IsNested)
            {
                prefix = t.FullNativeName;
                prefix = prefix.Substring(0, prefix.LastIndexOf("::"));
            }
            else
            {
                prefix = t.ParentClass.FullNativeName;
            }

            if (prefix.Contains("::"))
                prefix = prefix.Substring(prefix.IndexOf("::") + 2) + "::";
            else
                prefix = "";

            if (t.IsMapIterator)
                sb.Append("CPP_DECLARE_MAP_ITERATOR");
            else
                sb.Append("CPP_DECLARE_ITERATOR");

            bool noConstructor = t.TypeMembers[0].Type.ProtectionType == ProtectionType.Protected
                && !t.TypeMembers[0].Type.ParentClass.AllowVirtuals;

            if (noConstructor)
            {
                // the container type will not be declared,
                // declare an iterator without a constructor that takes a container class
                sb.Append("_NOCONSTRUCTOR");
            }

            if (t.IsMapIterator)
                sb.Append("( " + prefix + ", " + t.CLRName + ", " + t.FullNativeName + ", " + t.TypeMembers[0].Type.FullCLRName + ", " + t.IterationElementTypeMember.CLRTypeName + ", " + t.IterationElementTypeMember.NativeTypeName + ", " + t.IterationKeyTypeMember.CLRTypeName + ", " + t.IterationKeyTypeMember.NativeTypeName);
            else
                sb.Append("( " + prefix + ", " + t.CLRName + ", " + t.FullNativeName + ", " + t.TypeMembers[0].Type.FullCLRName + ", " + t.IterationElementTypeMember.CLRTypeName + ", " + t.IterationElementTypeMember.NativeTypeName);

            if (!noConstructor)
            {
                if (t.IsConstIterator)
                    sb.Append(", const");
                else
                    sb.Append(", ");
            }

            sb.Append(" )\n");

            AddTypeDependancy(t.TypeMembers[0].Type);

            sb.AppendLine();
        }

        public void IncAddInternalTypeDef(DefTypeDef t, IndentStringBuilder sb)
        {
            sb.AppendIndent("");
            if (t.IsNested)
                sb.Append(Producer.GetProtectionString(t.ProtectionType) + ": ");
            sb.Append("typedef " + t.FullNativeName + " " + t.CLRName + ";\n\n");
        }

        public void IncAddValueTypeTypeDef(DefTypeDef t, IndentStringBuilder sb)
        {
            sb.AppendIndent("");
            if (t.IsNested)
                sb.Append(Producer.GetProtectionString(t.ProtectionType) + ": ");
            sb.Append("typedef " + t.BaseType.FullCLRName + " " + t.CLRName + ";\n\n");
        }

        private void IncAddIncludeFiles(string include, List<DefType> usedTypes, IndentStringBuilder sb)
        {
            sb.AppendFormat("#include \"{0}\"\n", include);
            List<string> added = new List<string>();

            foreach (DefType type in usedTypes)
            {
                if (String.IsNullOrEmpty(type.IncludeFile) || type.IncludeFile == include)
                    continue;

                if (added.Contains(type.IncludeFile))
                    continue;

                sb.AppendLine("#include \"" + GetManagedIncludeFileName(type.IncludeFile) + "\"");
                added.Add(type.IncludeFile);
            }

            sb.AppendLine();
        }

        private void CppAddIncludeFiles(string include, List<DefType> usedTypes, IndentStringBuilder sb)
        {
            sb.AppendLine("#include \"MogreStableHeaders.h\"\n");
            sb.AppendFormat("#include \"{0}\"\n", GetManagedIncludeFileName(include));
            List<string> added = new List<string>();

            foreach (DefType type in usedTypes)
            {
                if (String.IsNullOrEmpty(type.IncludeFile) || type.IncludeFile == include)
                    continue;

                if (added.Contains(type.IncludeFile))
                    continue;

                sb.AppendLine("#include \"" + GetManagedIncludeFileName(type.IncludeFile) + "\"");
                added.Add(type.IncludeFile);
            }

            sb.AppendLine();
        }

        public virtual void AddTypeDependancy(DefType type)
        {
            if (!UsedTypes.Contains(type))
                this.UsedTypes.Add(type);
        }

        public virtual void AddPreDeclaration(string decl)
        {
            if (!PreDeclarations.Contains(decl))
                PreDeclarations.Add(decl);
        }

        public virtual void AddPragmaMakePublicForType(DefType type)
        {
            if (!PragmaMakePublicTypes.Contains(type))
                PragmaMakePublicTypes.Add(type);
        }

        public virtual void CheckTypeForDependancy(DefType type)
        {
            if (type is DefEnum
                || (!(type is IDefString) && type is DefTypeDef && (type as DefTypeDef).BaseType is DefInternal)
                || type.HasWrapType(WrapTypes.NativePtrValueType)
                || type.HasWrapType(WrapTypes.ValueType))
                AddTypeDependancy(type);
            else if (type.ParentClass != null)
                AddTypeDependancy(type.ParentClass);
            else if (type is DefTypeDef)
                CheckTypeForDependancy((type as DefTypeDef).BaseType);

            if (!type.IsNested && type is DefClass)
                AddPragmaMakePublicForType(type);
        }

        public virtual void CppCheckTypeForDependancy(DefType type)
        {
            if (!(type is DefEnum)
                && !type.IsNested
                && !type.IsPureManagedClass
                && !(type is DefInternal)
                && !type.IsValueType)
            {
                AddTypeDependancy(type);
            }
        }

        public static string Copyright = @"/*  This file is produced by the C++/CLI AutoWrapper utility.
        Copyright (c) 2006 by Argiris Kirtzidis  */

".Replace("\r", "");
    }
}
