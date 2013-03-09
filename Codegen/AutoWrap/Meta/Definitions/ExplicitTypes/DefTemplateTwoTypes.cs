using System;
using System.Xml;

namespace AutoWrap.Meta
{
    class DefTemplateTwoTypes : TypedefDefinition
    {
        public override bool IsValueType
        {
            get { return false; }
        }

        public override string CLRName
        {
            get
            {
                if (IsUnnamedSTLContainer)
                    return "STL" + STLContainer + "_" + TypeMembers[0].MemberType.Name + "_" + TypeMembers[1].MemberType.Name;

                return base.CLRName;
            }
        }

        public override string FullCLRName
        {
            get
            {
                if (IsUnnamedSTLContainer)
                    return CLRName;

                return base.FullCLRName;
            }
        }

        public override void ProduceDefaultParamValueConversionCode(ParamDefinition param, out string preConversion, out string conversion, out string postConversion, out TypeDefinition dependancyType)
        {
            preConversion = postConversion = "";
            dependancyType = null;
            switch (param.PassedByType)
            {
                case PassedByType.Pointer:
                    if (param.DefaultValue == "NULL" || param.DefaultValue == "0")
                    {
                        conversion = "nullptr";
                        return;
                    }

                    throw new Exception("Unexpected");
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetCLRParamTypeName(ParamDefinition param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Pointer:
                    if (param.IsConst)
                        return FullCLRName.Replace(CLRName, "Const_" + CLRName) + "^";

                    return FullCLRName + "^";
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string ProducePreCallParamConversionCode(ParamDefinition param, out string newname)
        {
            newname = param.Name;
            return "";
        }

        public override string ProducePostCallParamConversionCleanupCode(ParamDefinition param)
        {
            return "";
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Pointer:
                    if (m.IsConst)
                        return FullCLRName.Replace(CLRName, "Const_" + CLRName) + "^";

                    return FullCLRName + "^";
                case PassedByType.Value:
                    if (m.IsConst || IsReadOnly)
                        return FullCLRName.Replace(CLRName, "Const_" + CLRName) + "^";

                    return FullCLRName + "^";
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string ProduceNativeCallConversionCode(string expr, ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Pointer:
                    return expr;
                case PassedByType.Value:
                    if (m.IsConst || IsReadOnly)
                        return FullCLRName + "::ByValue( " + expr + " )->ReadOnlyInstance";

                    return FullCLRName + "::ByValue( " + expr + " )";
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string FullNativeName
        {
            get
            {
                if (Name.StartsWith("std::"))
                    return Name + "<" + TypeMembers[0].MemberTypeNativeName + "," + TypeMembers[1].MemberTypeNativeName + ">";

                if (ProtectionLevel == ProtectionLevel.Protected)
                    return NativeProtectedTypesProxy.GetProtectedTypesProxyName(SurroundingClass) + "::" + Name;

                return base.FullNativeName;
            }
        }

        public new static TypedefDefinition CreateExplicitType(TypedefDefinition typedef)
        {
            string baseTypeName = Mogre17.GetBaseType(typedef);

            switch (baseTypeName)
            {
                case "::std::hash_map":
                    return DefStdHashMap.CreateExplicitType(typedef);
                case "std::map":
                    return DefStdMap.CreateExplicitType(typedef);
                case "std::multimap":
                    return DefStdMultiMap.CreateExplicitType(typedef);
                case "std::pair":
                    return DefStdPair.CreateExplicitType(typedef);
                default:
                    throw new Exception("Unexpected");
            }
        }

        public DefTemplateTwoTypes(XmlElement elem)
            : base(elem)
        {
        }
    }
}