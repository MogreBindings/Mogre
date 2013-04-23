using System;
using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefSharedPtr : DefTemplateOneType
    {
        public override string ProducePreCallParamConversionCode(ParamDefinition param, out string newname)
        {
            newname = "(" + param.MemberTypeNativeName + ")" + param.Name;
            return String.Empty;
        }

        public override string GetCLRParamTypeName(ParamDefinition param)
        {
            return GetCLRTypeName(param);
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Value:
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
                case PassedByType.Value:
                    return expr;
                default:
                    throw new Exception("Unexpected");
            }
        }

        public new static TypedefDefinition CreateExplicitType(TypedefDefinition typedef)
        {
            return new DefSharedPtr(typedef.NameSpace, typedef.DefiningXmlElement);
        }

        public DefSharedPtr(NamespaceDefinition nsDef, XmlElement elem)
            : base(nsDef, elem)
        {
        }
    }
}