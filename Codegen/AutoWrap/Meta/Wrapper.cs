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
        public List<AbstractTypeDefinition> PragmaMakePublicTypes = new List<AbstractTypeDefinition>();
        public List<AbstractTypeDefinition> UsedTypes = new List<AbstractTypeDefinition>();
        public SortedList<string, List<AbstractTypeDefinition>> IncludeFiles = new SortedList<string, List<AbstractTypeDefinition>>();
        public List<ClassDefinition> Overridables = new List<ClassDefinition>();
        public List<ClassCodeProducer> PostClassProducers = new List<ClassCodeProducer>();
        public List<ClassCodeProducer> PreClassProducers = new List<ClassCodeProducer>();

        public Wrapper(MetaDefinition meta, string includePath, string sourcePath)
        {
            this._includePath = includePath;
            this._sourcePath = sourcePath;
            this.ManagedNamespace = Globals.ManagedNamespace;
            this.NativeNamespace = Globals.NativeNamespace;

            foreach (NamespaceDefinition space in meta.NameSpaces)
            {
                foreach (AbstractTypeDefinition type in space.Types)
                {

                    if (TypeIsWrappable(type))
                    {
                        List<AbstractTypeDefinition> list;
                        if (!IncludeFiles.TryGetValue(type.IncludeFile, out list))
                        {
                            list = new List<AbstractTypeDefinition>();
                            IncludeFiles.Add(type.IncludeFile, list);
                        }

                        if (type is EnumDefinition || type.IsInternalTypeDef())
                        {
                            list.Insert(0, type);
                        }
                        else if (type.HasWrapType(WrapTypes.NativePtrValueType))
                        {
                            //put it after enums and before other classes
                            int i;
                            for (i = 0; i < list.Count; i++)
                            {
                                if (!(type is EnumDefinition || type.IsInternalTypeDef()))
                                    break;
                            }

                            list.Insert(i, type);
                        }
                        else
                            list.Add(type);

                        if (type.HasWrapType(WrapTypes.Overridable))
                            Overridables.Add((ClassDefinition)type);
                    }
                }

                foreach (AbstractTypeDefinition type in space.Types)
                {
                    if (type is EnumDefinition && IncludeFiles.ContainsKey(type.IncludeFile))
                        if (!IncludeFiles[type.IncludeFile].Contains(type))
                            IncludeFiles[type.IncludeFile].Insert(0, type);
                }
            }
        }

        public bool TypeIsWrappable(AbstractTypeDefinition type)
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
            type = (type.IsNested) ? type.SurroundingClass.FindType<AbstractTypeDefinition>(type.Name) : type.NameSpace.FindType<AbstractTypeDefinition>(type.Name);

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
            else if (type is ClassDefinition)
            {
                ClassDefinition cls = type as ClassDefinition;
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
            else if (type is TypedefDefinition)
            {
                if (type.IsSTLContainer)
                {
                    foreach (ITypeMember m in (type as TypedefDefinition).TypeMembers)
                    {
                        AbstractTypeDefinition mt = m.MemberType;
                        if (!mt.IsValueType && !mt.IsPureManagedClass
                            && !TypeIsWrappable(mt))
                            return false;
                    }

                    return true;
                }
                else if (type is DefIterator)
                {
                    if (TypeIsWrappable((type as DefIterator).TypeMembers[0].MemberType))
                    {
                        if ((type as DefIterator).IsConstIterator)
                        {
                            try
                            {
                                AbstractTypeDefinition notconst = type.FindType<AbstractTypeDefinition>(type.Name.Substring("Const".Length), true);
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
                else if ((type as TypedefDefinition).BaseType is DefInternal
                         || (type as TypedefDefinition).BaseType.HasAttribute<ValueTypeAttribute>())
                    return true;
                else
                    return TypeIsWrappable((type as TypedefDefinition).BaseType);
            }
            else
                return false;
        }

        /// <summary>
        /// Generates the C++/CLI source and header files.
        /// </summary>
        public void GenerateCodeFiles()
        {
            StringBuilder builder = new StringBuilder();
            PreDeclarations.Clear();
            PragmaMakePublicTypes.Clear();

            //
            // Generate all source files from header files.
            //
            foreach (string includeFile in IncludeFiles.Keys)
            {
                // Strip ".h" from the file name
                string baseFileName = GetManagedIncludeFileName(includeFile.Substring(0, includeFile.Length - 2));

                string incFile = _includePath + "\\" + baseFileName + ".h";
                string cppFile = _sourcePath + "\\" + baseFileName + ".cpp";

                // Header file
                builder.Clear();
                builder.Append(HEADER_TEXT);
                builder.Append(GenerateIncludeFileCodeForIncludeFile(includeFile));
				if (includeFile == "OgrePrerequisites.h")
                {
                    builder.Append("#include \"MogrePagingPrerequisites.h\"");
                }
                WriteToFile(incFile, builder.ToString());

                // Source file
                bool hasContent;
                string txt = GenerateCppFileCodeForIncludeFile(includeFile, out hasContent);
                if (hasContent)
                {
                    // There is a .cpp file for the .h file.
                    builder.Clear();
                    builder.Append(HEADER_TEXT);
                    builder.Append(txt);
                    WriteToFile(cppFile, builder.ToString());
                }

                IncludeFileWrapped(this, new IncludeFileWrapEventArgs(includeFile));
            }

            //
            // Create PreDeclarations.h
            //
            builder.Clear();
            foreach (string decl in PreDeclarations)
            {
                builder.AppendLine(decl);
            }

            WriteToFile(_includePath + "\\PreDeclarations.h", builder.ToString());

            //
            // Create MakePublicDeclarations.h
            //
            builder.Clear();
            List<AbstractTypeDefinition> typesForMakePublic = new List<AbstractTypeDefinition>();

            foreach (AbstractTypeDefinition t in PragmaMakePublicTypes)
            {
                if (t is ClassDefinition && !t.IsNested && !t.IsIgnored)
                {
                    AbstractTypeDefinition type = t.FindType<AbstractTypeDefinition>(t.Name);

                    if (type.FullNativeName.StartsWith(NativeNamespace + "::")
                        && type is ClassDefinition && !type.IsTemplate)
                    {
                        if (!typesForMakePublic.Contains(type))
                            typesForMakePublic.Add(type);
                    }
                }
            }

            builder.AppendLine("#pragma once");
            builder.AppendLine();

            builder.AppendLine("namespace " + NativeNamespace);
            builder.AppendLine("{");

            foreach (AbstractTypeDefinition type in typesForMakePublic)
            {
                if (type is StructDefinition)
                    builder.Append("struct ");
                else
                    builder.Append("class ");
                builder.AppendLine(type.Name + ";");
            }

            builder.AppendLine("}");
            builder.AppendLine();

            foreach (AbstractTypeDefinition type in typesForMakePublic)
            {
                builder.AppendLine("#pragma make_public( " + type.FullNativeName + " )");
            }

            WriteToFile(_includePath + "\\MakePublicDeclarations.h", builder.ToString());
            
            //
            // Create CLRObjects.inc
            //
            builder.Clear();
            List<ClassDefinition> clrObjs = new List<ClassDefinition>();

            foreach (string include in IncludeFiles.Keys)
            {
                foreach (AbstractTypeDefinition t in IncludeFiles[include])
                    AddCLRObjects(t, clrObjs);
            }

            foreach (ClassDefinition cls in clrObjs)
            {
                string name = cls.Name;
                ClassDefinition parent = cls;
                while (parent.SurroundingClass != null)
                {
                    parent = parent.SurroundingClass;
                    name = parent.Name + "_" + name;
                }

                builder.AppendLine("CLROBJECT( " + name + " )");
            }

            WriteToFile(_includePath + "\\CLRObjects.inc", builder.ToString());
        }

        public string GetInitCLRObjectFuncSignature(ClassDefinition cls) {
            if (!cls.HasAttribute<CLRObjectAttribute>(true))
                throw new Exception("class is not subclass of CLRObject");

            string name = cls.Name;
            ClassDefinition parent = cls;
            while (parent.SurroundingClass != null)
            {
                parent = parent.SurroundingClass;
                name = parent.Name + "_" + name;
            }

            return "void _Init_CLRObject_" + name + "(CLRObject* pClrObj)";
        }

        void AddCLRObjects(AbstractTypeDefinition t, List<ClassDefinition> clrObjs) {
            ClassDefinition cls = t as ClassDefinition;
            if (cls == null)
                return;
            if (cls.IsIgnored || cls.ProtectionLevel != ProtectionLevel.Public)
                return;

            if (cls.HasAttribute<CLRObjectAttribute>(true))
                clrObjs.Add(cls);

            foreach (AbstractTypeDefinition nested in cls.NestedTypes)
                AddCLRObjects(nested, clrObjs);
        }

        public void ProduceSubclassCodeFiles(System.Windows.Forms.ProgressBar bar)
        {
          StringBuilder builder = new StringBuilder();

            bar.Minimum = 0;
            bar.Maximum = Overridables.Count;
            bar.Step = 1;
            bar.Value = 0;

            foreach (ClassDefinition type in Overridables)
            {
                string wrapFile = "Subclass" + type.Name;
                string incFile = _includePath + "\\" + wrapFile + ".h";
                string cppFile = _sourcePath + "\\" + wrapFile + ".cpp";

                builder.Clear();
                builder.Append(HEADER_TEXT);
                builder.Append(GenerateIncludeFileCodeForOverridable(type));
                WriteToFile(incFile, builder.ToString());

                builder.Clear();
                builder.Append(HEADER_TEXT);
                builder.Append(GenerateCppFileCodeForOverridable(type));
                WriteToFile(cppFile, builder.ToString());

                bar.Value++;
                bar.Refresh();
            }
        }

        /// <summary>
        /// Writes the contents to the specified file. Checks whether the content has actually
        /// changed to prevent unnecessary rebuilds.
        /// </summary>
        protected void WriteToFile(string file, string contents)
        {
            if (File.Exists(file))
            {
                string filecontent;
                using (StreamReader inp = new StreamReader(file, Encoding.UTF8))
                {
                    filecontent = inp.ReadToEnd();
                }

                if (contents == filecontent)
                    return;
            }

            using (StreamWriter writer = File.CreateText(file))
            {
                writer.Write(contents);
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

        /// <summary>
        /// Generates the C++/CLI code for .h file.
        /// </summary>
        /// <param name="includeFile">the name of the .h file from which to generate the code</param>
        public string GenerateIncludeFileCodeForIncludeFile(string includeFile)
        {
            UsedTypes.Clear();

            PreClassProducers.Clear();
            PostClassProducers.Clear();

            SourceCodeStringBuilder sbTypes = new SourceCodeStringBuilder();
            foreach (AbstractTypeDefinition t in IncludeFiles[includeFile])
            {
                IncAddType(t, sbTypes);
            }

            foreach (ClassCodeProducer producer in PostClassProducers)
            {
                producer.Add();
            }

            foreach (ClassCodeProducer producer in PreClassProducers)
            {
                producer.AddFirst();
            }

            SourceCodeStringBuilder sb = new SourceCodeStringBuilder();
            sb.AppendLine("#pragma once\n");

            IncAddIncludeFiles(includeFile, UsedTypes, sb);

            sb.AppendFormat("namespace {0}\n{{\n", ManagedNamespace);

            sb.IncreaseIndent();
            sb.AppendLine(sbTypes.ToString());
            sb.DecreaseIndent();

            sb.AppendLine("}");

            return sb.ToString();
        }

        public string GenerateCppFileCodeForIncludeFile(string include, out bool hasContent)
        {
            UsedTypes.Clear();

            PreClassProducers.Clear();
            PostClassProducers.Clear();

            SourceCodeStringBuilder contentsb = new SourceCodeStringBuilder();
            foreach (AbstractTypeDefinition t in IncludeFiles[include])
            {
                CppAddType(t, contentsb);
            }

            foreach (ClassCodeProducer producer in PostClassProducers)
            {
                producer.Add();
            }

            foreach (ClassCodeProducer producer in PreClassProducers)
            {
                producer.AddFirst();
            }

            SourceCodeStringBuilder sb = new SourceCodeStringBuilder();
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

            return sb.ToString();
        }

        public string GenerateIncludeFileCodeForOverridable(ClassDefinition type)
        {
            UsedTypes.Clear();

            PreClassProducers.Clear();
            PostClassProducers.Clear();

            SourceCodeStringBuilder sbTypes = new SourceCodeStringBuilder();

            new IncSubclassingClassProducer(this, type, sbTypes, null).Add();
            if (type.HasAttribute<InterfacesForOverridableAttribute>())
            {
                List<ClassDefinition[]> interfaces = type.GetAttribute<InterfacesForOverridableAttribute>().Interfaces;
                foreach (ClassDefinition[] ifaces in interfaces)
                {
                    new IncSubclassingClassProducer(this, type, sbTypes, ifaces).Add();
                }
            }

            foreach (ClassCodeProducer producer in PostClassProducers)
            {
                if (!(producer is NativeProtectedTypesProxy)
                    && !(producer is NativeProtectedStaticsProxy))
                    producer.Add();
            }

            foreach (ClassCodeProducer producer in PreClassProducers)
            {
                if (!(producer is NativeProtectedTypesProxy)
                    && !(producer is NativeProtectedStaticsProxy))
                    producer.AddFirst();
            }

            SourceCodeStringBuilder sb = new SourceCodeStringBuilder();
            sb.AppendLine("#pragma once\n");

            sb.AppendFormat("namespace {0}\n{{\n", ManagedNamespace);

            sb.IncreaseIndent();
            sb.AppendLine(sbTypes.ToString());
            sb.DecreaseIndent();

            sb.AppendLine("}");

            return sb.ToString();
        }

        public string GenerateCppFileCodeForOverridable(ClassDefinition type)
        {
            SourceCodeStringBuilder sb = new SourceCodeStringBuilder();

            sb.AppendLine("#include \"MogreStableHeaders.h\"");
            sb.AppendLine("#include \"Subclass" + type.Name + ".h\"\n");
            
            sb.AppendFormat("namespace {0}\n{{\n", ManagedNamespace);

            sb.IncreaseIndent();

            PreClassProducers.Clear();
            PostClassProducers.Clear();

            new CppSubclassingClassProducer(this, type, sb, null).Add();
            if (type.HasAttribute<InterfacesForOverridableAttribute>())
            {
                List<ClassDefinition[]> interfaces = type.GetAttribute<InterfacesForOverridableAttribute>().Interfaces;
                foreach (ClassDefinition[] ifaces in interfaces)
                {
                    new CppSubclassingClassProducer(this, type, sb, ifaces).Add();
                }
            }

            foreach (ClassCodeProducer producer in PostClassProducers)
            {
                if (!(producer is NativeProtectedTypesProxy)
                    && !(producer is NativeProtectedStaticsProxy))
                    producer.Add();
            }

            foreach (ClassCodeProducer producer in PreClassProducers)
            {
                if ( !(producer is NativeProtectedTypesProxy)
                    && !(producer is NativeProtectedStaticsProxy))
                    producer.AddFirst();
            }

            sb.DecreaseIndent();

            sb.AppendLine("}");

            return sb.ToString();
        }

        public void IncAddType(AbstractTypeDefinition t, SourceCodeStringBuilder sb)
        {
            if (t.HasAttribute<CustomIncClassDefinitionAttribute>())
            {
                string txt = t.GetAttribute<CustomIncClassDefinitionAttribute>().Text;
                sb.AppendLine(txt);
                return;
            }

            if (t is ClassDefinition)
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
                            new IncNonOverridableClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.Overridable:
                            new IncOverridableClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.NativeDirector:
                            new IncNativeDirectorClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.Interface:
                            new IncInterfaceClassProducer(this, t as ClassDefinition, sb).Add();
                            new IncOverridableClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.Singleton:
                            new IncSingletonClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.ReadOnlyStruct:
                            new IncReadOnlyStructClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.ValueType:
                            new IncValueClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.NativePtrValueType:
                            new IncNativePtrValueClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.CLRHandle:
                            new IncCLRHandleClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.PlainWrapper:
                            new IncPlainWrapperClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.SharedPtr:
                            IncAddSharedPtrType(t, sb);
                            break;
                    }
                }
            }
            else if (t is EnumDefinition)
            {
                IncAddEnum(t as EnumDefinition, sb);
            }
            else if (t is TypedefDefinition)
            {
                TypedefDefinition explicitType;

                if (t.IsUnnamedSTLContainer)
                    explicitType = t as TypedefDefinition;
                else
                    explicitType = (t.IsNested) ? t.SurroundingClass.FindType<TypedefDefinition>(t.Name) : t.NameSpace.FindType<TypedefDefinition>(t.Name);

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

        public void CppAddType(AbstractTypeDefinition t, SourceCodeStringBuilder sb)
        {
            if (t.HasAttribute<CustomCppClassDefinitionAttribute>())
            {
                string txt = t.GetAttribute<CustomCppClassDefinitionAttribute>().Text;
                sb.AppendLine(txt);
                return;
            }

            if (t is ClassDefinition)
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
                            new CppNonOverridableClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.Overridable:
                            new CppOverridableClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.Interface:
                            new CppOverridableClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.NativeDirector:
                            new CppNativeDirectorClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.NativePtrValueType:
                            new CppNativePtrValueClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.Singleton:
                            new CppSingletonClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.CLRHandle:
                            new CppCLRHandleClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                        case WrapTypes.PlainWrapper:
                            new CppPlainWrapperClassProducer(this, t as ClassDefinition, sb).Add();
                            break;
                    }
                }
            }
            else if (t is TypedefDefinition)
            {
                TypedefDefinition explicitType;

                if (t.IsUnnamedSTLContainer)
                    explicitType = t as TypedefDefinition;
                else
                    explicitType = (t.IsNested) ? t.SurroundingClass.FindType<TypedefDefinition>(t.Name) : t.NameSpace.FindType<TypedefDefinition>(t.Name);

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
        public void IncAddEnum(EnumDefinition enm, SourceCodeStringBuilder sb) //, bool inProtectedTypesProxy)
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
              sb.Append(enm.ProtectionLevel.GetCLRProtectionName() + ": ");

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

        private void IncAddSharedPtrType(AbstractTypeDefinition type, SourceCodeStringBuilder sb)
        {
            if (!type.Name.EndsWith("Ptr"))
                throw new Exception("SharedPtr class that doesn't have a name ending to 'Ptr'");

            string basename = null;
            if (type is ClassDefinition)
                basename = (type as ClassDefinition).Inherits[0];
            else
                basename = (type as TypedefDefinition).BaseTypeName;

            int s = basename.IndexOf("<");
            int e = basename.LastIndexOf(">");
            string baseClass = basename.Substring(s + 1, e - s - 1).Trim();
            //string nativeClass = _nativePrefix + "::" + baseClass;
            AbstractTypeDefinition baseType = type.FindType<AbstractTypeDefinition>(baseClass);
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
                sb.AppendIndent(type.ProtectionLevel.GetCLRProtectionName() + ": ");
            }

            sb.Append("ref class " + type.Name + " : public " + baseClass + "\n");
            sb.AppendLine("{");
            sb.AppendLine("public protected:");
            sb.IncreaseIndent();
            sb.AppendLine("\t" + type.FullNativeName + "* _sharedPtr;");
            sb.AppendEmptyLine();
            sb.AppendLine(type.Name + "(" + type.FullNativeName + "& sharedPtr) : " + baseClass + "( sharedPtr.getPointer() )");
            sb.AppendLine("{");
            sb.AppendLine("\t_sharedPtr = new " + type.FullNativeName + "(sharedPtr);");
            sb.AppendLine("}");
            sb.AppendEmptyLine();
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
            sb.AppendEmptyLine();
            sb.AppendLine("~" + type.Name + "()");
            sb.AppendLine("{");
            sb.AppendLine("\tthis->!" + type.Name + "();");
            sb.AppendLine("}");
            sb.AppendEmptyLine();
            sb.DecreaseIndent();
            sb.AppendLine("public:");
            sb.IncreaseIndent();

            sb.AppendLine("DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_SHAREDPTR( " + className + " )");
            sb.AppendEmptyLine();

            if (type is ClassDefinition)
            {
                ClassDefinition realType = type.FindType<ClassDefinition>(baseClass, false);
                if (realType != null && realType.BaseClass != null && realType.BaseClass.Name == "Resource")
                {
                    // For Resource subclasses (Material etc.) allow implicit conversion of ResourcePtr (i.e ResourcePtr -> MaterialPtr)

                    AddTypeDependancy(realType.BaseClass);

                    sb.AppendLine("static " + type.Name + "^ FromResourcePtr( ResourcePtr^ ptr )");
                    sb.AppendLine("{");
                    sb.AppendLine("\treturn (" + type.Name + "^) ptr;");
                    sb.AppendLine("}");
                    sb.AppendEmptyLine();

                    sb.AppendLine("static operator " + type.Name + "^ ( ResourcePtr^ ptr )");
                    sb.AppendLine("{");
                    sb.IncreaseIndent();
                    sb.AppendLine("if (CLR_NULL == ptr) return nullptr;");
                    sb.AppendLine("void* castptr = dynamic_cast<" + nativeClass + "*>(ptr->_native);");
                    sb.AppendLine("if (castptr == 0) throw gcnew InvalidCastException(\"The underlying type of the ResourcePtr object is not of type " + baseClass + ".\");");
                    sb.AppendLine("return gcnew " + type.Name + "( (" + type.FullNativeName + ") *(ptr->_sharedPtr) );");
                    sb.DecreaseIndent();
                    sb.AppendLine("}");
                    sb.AppendEmptyLine();
                }
            }

            //sb.AppendLine(type.Name + "() : " + baseClass + "( (" + nativeClass + "*) 0 )");
            //sb.AppendLine("{");
            //sb.AppendLine("\t_sharedPtr = new " + type.FullNativeName + "();");
            //sb.AppendLine("}");
            //sb.AppendLine();
            if (baseType is ClassDefinition && (baseType as ClassDefinition).IsInterface)
            {
                string proxyName = NativeProxyClassProducer.GetProxyName(baseType as ClassDefinition);
                sb.AppendLine(type.Name + "(" + baseType.CLRName + "^ obj) : " + baseClass + "( static_cast<" + proxyName + "*>( (" + nativeClass + "*)obj ) )");
                sb.AppendLine("{");
                sb.AppendLine("\t_sharedPtr = new " + type.FullNativeName + "( static_cast<" + proxyName + "*>(obj->_native) );");
                sb.AppendLine("}");
                sb.AppendEmptyLine();
            }
            else
            {
                sb.AppendLine(type.Name + "(" + baseClass + "^ obj) : " + baseClass + "( obj->_native )");
                sb.AppendLine("{");
                sb.AppendLine("\t_sharedPtr = new " + type.FullNativeName + "( static_cast<" + nativeClass + "*>(obj->_native) );");
                sb.AppendLine("}");
                sb.AppendEmptyLine();
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
            sb.AppendEmptyLine();
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
            sb.AppendEmptyLine();
            sb.AppendLine("return (_native == obj->_native);");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendEmptyLine();

            sb.AppendLine("static bool operator == (" + type.Name + "^ val1, " + type.Name + "^ val2)");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine("if ((Object^)val1 == (Object^)val2) return true;");
            sb.AppendLine("if ((Object^)val1 == nullptr || (Object^)val2 == nullptr) return false;");
            sb.AppendLine("return (val1->_native == val2->_native);");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendEmptyLine();
            sb.AppendLine("static bool operator != (" + type.Name + "^ val1, " + type.Name + "^ val2)");
            sb.AppendLine("{");
            sb.AppendLine("\treturn !(val1 == val2);");
            sb.AppendLine("}");
            sb.AppendEmptyLine();

            sb.AppendLine("virtual int GetHashCode() override");
            sb.AppendLine("{");
            sb.AppendLine("\treturn reinterpret_cast<int>( _native );");
            sb.AppendLine("}");
            sb.AppendEmptyLine();

            sb.AppendLine("property IntPtr NativePtr");
            sb.AppendLine("{");
            sb.AppendLine("\tIntPtr get() { return (IntPtr)_sharedPtr; }");
            sb.AppendLine("}");
            sb.AppendEmptyLine();

            sb.AppendLine("property bool Unique");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine("bool get()");
            sb.AppendLine("{");
            sb.AppendLine("\treturn (*_sharedPtr).unique();");
            sb.AppendLine("}");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendEmptyLine();
            sb.AppendLine("property int UseCount");
            sb.AppendLine("{");
            sb.IncreaseIndent();
            sb.AppendLine("int get()");
            sb.AppendLine("{");
            sb.AppendLine("\treturn (*_sharedPtr).useCount();");
            sb.AppendLine("}");
            sb.DecreaseIndent();
            sb.AppendLine("}");
            sb.AppendEmptyLine();
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

        public void IncAddSTLContainer(TypedefDefinition t, SourceCodeStringBuilder sb)
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
                    sb.AppendEmptyLine();
                }

                sb.AppendLine("#define STLDECL_MANAGEDTYPE " + t.TypeMembers[0].MemberTypeCLRName);
                sb.AppendLine("#define STLDECL_NATIVETYPE " + t.TypeMembers[0].MemberTypeNativeName);
                CheckTypeForDependancy(t.TypeMembers[0].MemberType);
            }
            else if (t is DefTemplateTwoTypes)
            {
                sb.AppendLine("#define STLDECL_MANAGEDKEY " + t.TypeMembers[0].MemberTypeCLRName);
                sb.AppendLine("#define STLDECL_MANAGEDVALUE " + t.TypeMembers[1].MemberTypeCLRName);
                sb.AppendLine("#define STLDECL_NATIVEKEY " + t.TypeMembers[0].MemberTypeNativeName);
                sb.AppendLine("#define STLDECL_NATIVEVALUE " + t.TypeMembers[1].MemberTypeNativeName);
                CheckTypeForDependancy(t.TypeMembers[0].MemberType);
                CheckTypeForDependancy(t.TypeMembers[1].MemberType);
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
                publicprot = t.ProtectionLevel.GetCLRProtectionName() + ": ";
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
                    sb.AppendEmptyLine();
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

            sb.AppendEmptyLine();
        }

        public void CppAddSTLContainer(TypedefDefinition t, SourceCodeStringBuilder sb)
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
                    sb.AppendEmptyLine();
                }

                sb.AppendLine("#define STLDECL_MANAGEDTYPE " + t.TypeMembers[0].MemberTypeCLRName);
                sb.AppendLine("#define STLDECL_NATIVETYPE " + t.TypeMembers[0].MemberTypeNativeName);
                CppCheckTypeForDependancy(t.TypeMembers[0].MemberType);
            }
            else if (t is DefTemplateTwoTypes)
            {
                sb.AppendLine("#define STLDECL_MANAGEDKEY " + t.TypeMembers[0].MemberTypeCLRName);
                sb.AppendLine("#define STLDECL_MANAGEDVALUE " + t.TypeMembers[1].MemberTypeCLRName);
                sb.AppendLine("#define STLDECL_NATIVEKEY " + t.TypeMembers[0].MemberTypeNativeName);
                sb.AppendLine("#define STLDECL_NATIVEVALUE " + t.TypeMembers[1].MemberTypeNativeName);
                CppCheckTypeForDependancy(t.TypeMembers[0].MemberType);
                CppCheckTypeForDependancy(t.TypeMembers[1].MemberType);
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
                prefix = t.SurroundingClass.FullNativeName;
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
                    sb.AppendEmptyLine();
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

            sb.AppendEmptyLine();
        }

        public void IncAddIterator(DefIterator t, SourceCodeStringBuilder sb)
        {
            if (!t.IsNested)
            {
                this.AddPreDeclaration("ref class " + t.CLRName + ";");
            }

            CheckTypeForDependancy(t.IterationElementTypeMember.MemberType);

            if (t.IsMapIterator)
                CheckTypeForDependancy(t.IterationKeyTypeMember.MemberType);

            sb.AppendIndent(t.ProtectionLevel.GetCLRProtectionName());
            if (t.IsNested) sb.Append(":");

            if (t.IsMapIterator)
                sb.Append(" INC_DECLARE_MAP_ITERATOR");
            else
                sb.Append(" INC_DECLARE_ITERATOR");

            if (t.TypeMembers[0].MemberType.ProtectionLevel == ProtectionLevel.Protected
                && !t.TypeMembers[0].MemberType.SurroundingClass.AllowVirtuals)
            {
                // the container type will not be declared,
                // declare an iterator without a constructor that takes a container class
                sb.Append("_NOCONSTRUCTOR");
            }

            if (t.IsMapIterator)
                sb.Append("( " + t.CLRName + ", " + t.FullNativeName + ", " + t.TypeMembers[0].MemberType.FullCLRName + ", " + t.IterationElementTypeMember.MemberTypeCLRName + ", " + t.IterationElementTypeMember.MemberTypeNativeName + ", " + t.IterationKeyTypeMember.MemberTypeCLRName + ", " + t.IterationKeyTypeMember.MemberTypeNativeName + " )\n");
            else
                sb.Append("( " + t.CLRName + ", " + t.FullNativeName + ", " + t.TypeMembers[0].MemberType.FullCLRName + ", " + t.IterationElementTypeMember.MemberTypeCLRName + ", " + t.IterationElementTypeMember.MemberTypeNativeName + " )\n");

            sb.AppendEmptyLine();
        }

        public void CppAddIterator(DefIterator t, SourceCodeStringBuilder sb)
        {
            string prefix;
            if (!t.IsNested)
            {
                prefix = t.FullNativeName;
                prefix = prefix.Substring(0, prefix.LastIndexOf("::"));
            }
            else
            {
                prefix = t.SurroundingClass.FullNativeName;
            }

            if (prefix.Contains("::"))
                prefix = prefix.Substring(prefix.IndexOf("::") + 2) + "::";
            else
                prefix = "";

            if (t.IsMapIterator)
                sb.Append("CPP_DECLARE_MAP_ITERATOR");
            else
                sb.Append("CPP_DECLARE_ITERATOR");

            bool noConstructor = t.TypeMembers[0].MemberType.ProtectionLevel == ProtectionLevel.Protected
                && !t.TypeMembers[0].MemberType.SurroundingClass.AllowVirtuals;

            if (noConstructor)
            {
                // the container type will not be declared,
                // declare an iterator without a constructor that takes a container class
                sb.Append("_NOCONSTRUCTOR");
            }

            if (t.IsMapIterator)
                sb.Append("( " + prefix + ", " + t.CLRName + ", " + t.FullNativeName + ", " + t.TypeMembers[0].MemberType.FullCLRName + ", " + t.IterationElementTypeMember.MemberTypeCLRName + ", " + t.IterationElementTypeMember.MemberTypeNativeName + ", " + t.IterationKeyTypeMember.MemberTypeCLRName + ", " + t.IterationKeyTypeMember.MemberTypeNativeName);
            else
                sb.Append("( " + prefix + ", " + t.CLRName + ", " + t.FullNativeName + ", " + t.TypeMembers[0].MemberType.FullCLRName + ", " + t.IterationElementTypeMember.MemberTypeCLRName + ", " + t.IterationElementTypeMember.MemberTypeNativeName);

            if (!noConstructor)
            {
                if (t.IsConstIterator)
                    sb.Append(", const");
                else
                    sb.Append(", ");
            }

            sb.Append(" )\n");

            AddTypeDependancy(t.TypeMembers[0].MemberType);

            sb.AppendEmptyLine();
        }

        public void IncAddInternalTypeDef(TypedefDefinition t, SourceCodeStringBuilder sb)
        {
            sb.AppendIndent("");
            if (t.IsNested)
                sb.Append(t.ProtectionLevel.GetCLRProtectionName() + ": ");
            sb.Append("typedef " + t.FullNativeName + " " + t.CLRName + ";\n\n");
        }

        public void IncAddValueTypeTypeDef(TypedefDefinition t, SourceCodeStringBuilder sb)
        {
            sb.AppendIndent("");
            if (t.IsNested)
                sb.Append(t.ProtectionLevel.GetCLRProtectionName() + ": ");
            sb.Append("typedef " + t.BaseType.FullCLRName + " " + t.CLRName + ";\n\n");
        }

        private void IncAddIncludeFiles(string include, List<AbstractTypeDefinition> usedTypes, SourceCodeStringBuilder sb)
        {
            sb.AppendFormat("#include \"{0}\"\n", include);
            List<string> added = new List<string>();

            foreach (AbstractTypeDefinition type in usedTypes)
            {
                if (String.IsNullOrEmpty(type.IncludeFile) || type.IncludeFile == include)
                    continue;

                if (added.Contains(type.IncludeFile))
                    continue;

                sb.AppendLine("#include \"" + GetManagedIncludeFileName(type.IncludeFile) + "\"");
                added.Add(type.IncludeFile);
            }

            sb.AppendEmptyLine();
        }

        private void CppAddIncludeFiles(string include, List<AbstractTypeDefinition> usedTypes, SourceCodeStringBuilder sb)
        {
            sb.AppendLine("#include \"MogreStableHeaders.h\"\n");
            sb.AppendFormat("#include \"{0}\"\n", GetManagedIncludeFileName(include));
            List<string> added = new List<string>();

            foreach (AbstractTypeDefinition type in usedTypes)
            {
                if (String.IsNullOrEmpty(type.IncludeFile) || type.IncludeFile == include)
                    continue;

                if (added.Contains(type.IncludeFile))
                    continue;

                sb.AppendLine("#include \"" + GetManagedIncludeFileName(type.IncludeFile) + "\"");
                added.Add(type.IncludeFile);
            }

            sb.AppendEmptyLine();
        }

        public virtual void AddTypeDependancy(AbstractTypeDefinition type)
        {
            if (!UsedTypes.Contains(type))
                this.UsedTypes.Add(type);
        }

        public virtual void AddPreDeclaration(string decl)
        {
            if (!PreDeclarations.Contains(decl))
                PreDeclarations.Add(decl);
        }

        public virtual void AddPragmaMakePublicForType(AbstractTypeDefinition type)
        {
            if (!PragmaMakePublicTypes.Contains(type))
                PragmaMakePublicTypes.Add(type);
        }

        public virtual void CheckTypeForDependancy(AbstractTypeDefinition type)
        {
            if (type is EnumDefinition
                || (!(type is IDefString) && type is TypedefDefinition && (type as TypedefDefinition).BaseType is DefInternal)
                || type.HasWrapType(WrapTypes.NativePtrValueType)
                || type.HasWrapType(WrapTypes.ValueType))
                AddTypeDependancy(type);
            else if (type.SurroundingClass != null)
                AddTypeDependancy(type.SurroundingClass);
            else if (type is TypedefDefinition)
                CheckTypeForDependancy((type as TypedefDefinition).BaseType);

            if (!type.IsNested && type is ClassDefinition)
                AddPragmaMakePublicForType(type);
        }

        public virtual void CppCheckTypeForDependancy(AbstractTypeDefinition type)
        {
            if (!(type is EnumDefinition)
                && !type.IsNested
                && !type.IsPureManagedClass
                && !(type is DefInternal)
                && !type.IsValueType)
            {
                AddTypeDependancy(type);
            }
        }


        /// <summary>
        /// Header text for all auto-generated source files.
        /// </summary>
        public static readonly string HEADER_TEXT = 
            (  "/*  This file is produced by the C++/CLI AutoWrapper utility.\n"
             + "          Copyright (c) 2006 by Argiris Kirtzidis  */\n\n")
            .Replace("\n", SourceCodeStringBuilder.NEWLINE_STRING);
    }
}
