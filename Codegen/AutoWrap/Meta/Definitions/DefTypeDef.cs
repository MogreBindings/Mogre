using System;
using System.Xml;

namespace AutoWrap.Meta
{
    /// <summary>
    /// Describes a C++ <c>typedef</c>.
    /// </summary>
    public class DefTypeDef : DefType
    {
        public override void ProduceDefaultParamValueConversionCode(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            preConversion = postConversion = "";
            dependancyType = null;
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
                conversion = param.DefaultValue;
            else
                throw new Exception("Unexpected");
        }

        public override void ProduceNativeParamConversionCode(DefParam param, out string preConversion, out string conversion, out string postConversion)
        {
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
                BaseType.ProduceNativeParamConversionCode(param, out preConversion, out conversion, out postConversion);
            else
                base.ProduceNativeParamConversionCode(param, out preConversion, out conversion, out postConversion);
        }

        public override string GetCLRParamTypeName(DefParam param)
        {
            string s = BaseType.GetCLRParamTypeName(param).Replace(BaseType.FullCLRName, FullCLRName);
            if (s.Contains("Mogre::int32"))
                return s;
            return
                BaseType.GetCLRParamTypeName(param).Replace(BaseType.FullCLRName, FullCLRName).Replace(BaseType.FullNativeName, FullNativeName);
        }

        public override string ProducePreCallParamConversionCode(DefParam param, out string newname)
        {
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
            {
                string s = BaseType.ProducePreCallParamConversionCode(param, out newname).Replace(BaseType.FullCLRName, FullCLRName);
                if (s.Contains("Mogre::int32"))
                    return s;
                return
                    BaseType.ProducePreCallParamConversionCode(param, out newname)
                            .Replace(BaseType.FullCLRName, FullCLRName)
                            .Replace(BaseType.FullNativeName, FullNativeName);
            }

            return base.ProducePreCallParamConversionCode(param, out newname);
        }

        public override string ProducePostCallParamConversionCleanupCode(DefParam param)
        {
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
            {
                string s = BaseType.ProducePostCallParamConversionCleanupCode(param).Replace(BaseType.FullCLRName, FullCLRName);
                if (s.Contains("Mogre::int32"))
                    return s;
                return
                    BaseType.ProducePostCallParamConversionCleanupCode(param).Replace(BaseType.FullCLRName, FullCLRName).Replace(BaseType.FullNativeName, FullNativeName);
            }

            return base.ProducePostCallParamConversionCleanupCode(param);
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            string s = BaseType.GetCLRTypeName(m).Replace(BaseType.FullCLRName, FullCLRName);
            if (s.Contains("Mogre::int32"))
                return s;
            return BaseType.GetCLRTypeName(m).Replace(BaseType.FullCLRName, FullCLRName).Replace(BaseType.FullNativeName, FullNativeName);
        }

        public override string ProduceNativeCallConversionCode(string expr, ITypeMember m)
        {
            if (!(this is DefTemplateOneType || this is DefTemplateTwoTypes) && BaseType is DefInternal)
            {
                string s = BaseType.ProduceNativeCallConversionCode(expr, m).Replace(BaseType.FullCLRName, FullCLRName);
                if (s.Contains("Mogre::int32"))
                    return s;
                return
                    BaseType.ProduceNativeCallConversionCode(expr, m).Replace(BaseType.FullCLRName, FullCLRName).Replace(BaseType.FullNativeName, FullNativeName);
            }

            return base.ProduceNativeCallConversionCode(expr, m);
        }

        public static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            DefTypeDef expl = null;

            if (typedef.BaseTypeName.Contains("<") || typedef.BaseTypeName.Contains("std::") || Mogre17.IsCollection(typedef.BaseTypeName))
            {
                if (typedef.BaseTypeName == "std::vector" || typedef.BaseTypeName == "std::list")
                {
                    expl = DefTemplateOneType.CreateExplicitType(typedef);
                }
                else
                {
                    switch (typedef.TypeNames.Length)
                    {
                        case 1:
                            expl = DefTemplateOneType.CreateExplicitType(typedef);
                            break;
                        case 2:
                            expl = DefTemplateTwoTypes.CreateExplicitType(typedef);
                            break;
                        default:
                            throw new Exception("Unexpected");
                    }
                }
            }
            else if (typedef.Name == "String")
            {
                expl = new DefStringTypeDef(typedef.Element);
            }

            if (expl != null)
            {
                expl.SurroundingClass = typedef.SurroundingClass;
                expl.NameSpace = typedef.NameSpace;
                expl.Attributes = typedef.Attributes;
                return expl;
            }

            return typedef;
        }

        public override bool IsIgnored
        {
            get
            {
                if (base.IsIgnored)
                    return true;

                foreach (ITypeMember m in TypeMembers)
                    if (m.MemberType.IsIgnored || m.MemberType.HasWrapType(WrapTypes.NativeDirector))
                        return true;

                return false;
            }
        }

        public override bool IsValueType
        {
            get { return BaseType.IsValueType; }
        }

        private DefType _baseType;

        public virtual DefType BaseType
        {
            get
            {
                if (_baseType == null)
                {
                    string basename = BaseTypeName;
                    if (basename.Contains("<"))
                        basename = basename.Substring(0, basename.IndexOf("<")).Trim();

                    _baseType = FindType<DefType>(basename, false);
                }

                return _baseType;
            }
        }

        public bool IsTypedefOfInternalType
        {
            get
            {
                return BaseType is DefInternal || (BaseType is DefTypeDef && (BaseType as DefTypeDef).IsTypedefOfInternalType);
            }
        }

        public string BaseTypeName;
        public string[] TypeNames;

        protected PassedByType[] _passed;

        private ITypeMember[] _types;

        public virtual ITypeMember[] TypeMembers
        {
            get
            {
                if (_types == null)
                {
                    _types = new ITypeMember[TypeNames.Length];
                    for (int i = 0; i < TypeNames.Length; i++)
                    {
                        bool isConst = false;
                        string name = TypeNames[i];
                        if (name.StartsWith("const "))
                        {
                            isConst = true;
                            name = name.Substring("const ".Length);
                        }
                        _types[i] = new DefTypeMember(FindType<DefType>(name, false), _passed[i], isConst);
                    }
                }

                return _types;
            }
        }

        public override bool IsSharedPtr
        {
            get { return BaseTypeName.StartsWith("SharedPtr"); }
        }

        public DefTypeDef(XmlElement elem)
            : base(elem)
        {
            if (elem.Name != "typedef")
                throw new Exception("Not typedef element");

            BaseTypeName = elem.GetAttribute("basetype");
            string type = elem.GetAttribute("type");
            if (type == "")
            {
                TypeNames = new string[elem.ChildNodes.Count];
                _passed = new PassedByType[elem.ChildNodes.Count];
                for (int i = 0; i < elem.ChildNodes.Count; i++)
                {
                    TypeNames[i] = elem.ChildNodes[i].InnerText.Trim();
                    string pass = (elem.ChildNodes[i] as XmlElement).GetAttribute("passedBy");
                    _passed[i] = (pass == "") ? PassedByType.Value : (PassedByType) Enum.Parse(typeof (PassedByType), pass, true);
                }
            }
            else
            {
                TypeNames = new string[1];
                TypeNames[0] = type.Trim();
                _passed = new PassedByType[1];
                string pass = elem.GetAttribute("passedBy");
                _passed[0] = (pass == "") ? PassedByType.Value : (PassedByType) Enum.Parse(typeof (PassedByType), pass, true);
            }
        }
    }
}