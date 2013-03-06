using System;

namespace AutoWrap.Meta
{
    internal class DefString : DefType, IDefString
    {
        public override string FullNativeName
        {
            get { return "std::string"; }
        }

        public override bool IsValueType
        {
            get { return true; }
        }

        public override void ProduceDefaultParamValueConversionCode(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            preConversion = postConversion = "";
            dependancyType = null;
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Value:
                    conversion = param.DefaultValue.Trim();
                    if (!conversion.StartsWith("\"") && conversion.Contains("::"))
                    {
                        //It's a static string of a class

                        if (conversion == "StringUtil::BLANK")
                        {
                            //Manually translate "StringUtil::BLANK" so that there's no need to wrap the StringUtil class
                            conversion = "String::Empty";
                            return;
                        }

                        string name = conversion.Substring(0, conversion.LastIndexOf("::"));
                        dependancyType = FindType<DefType>(name);
                    }
                    break;
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetCLRParamTypeName(DefParam param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "String^";
                case PassedByType.Pointer:
                    return "array<String^>^";
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string ProducePreCallParamConversionCode(DefParam param, out string newname)
        {
            string name = param.Name;
            switch (param.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    newname = "o_" + name;
                    return "DECLARE_NATIVE_STRING( o_" + name + ", " + name + " )\n";
                case PassedByType.Pointer:
                    string expr = FullNativeName + "* arr_" + name + " = new " + FullNativeName + "[" + name + "->Length];\n";
                    expr += "for (int i=0; i < " + name + "->Length; i++)\n";
                    expr += "{\n";
                    expr += "\tSET_NATIVE_STRING( arr_" + name + "[i], " + name + "[i] )\n";
                    expr += "}\n";
                    newname = "arr_" + name;
                    return expr;
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string ProducePostCallParamConversionCleanupCode(DefParam param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "";
                case PassedByType.Pointer:
                    return "delete[] arr_" + param.Name + ";\n";
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "String^";
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string ProduceNativeCallConversionCode(string expr, ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "TO_CLR_STRING( " + expr + " )";
                default:
                    throw new Exception("Unexpected");
            }
        }
    }
}