using System;
using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefSharedPtr : DefTemplateOneType
    {
        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            newname = "(" + param.MemberTypeNativeName + ")" + param.Name;
            return String.Empty;
        }

        public override string GetCLRParamTypeName(DefParam param)
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

        public override string GetNativeCallConversion(string expr, ITypeMember m)
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

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefSharedPtr(typedef.Element);
        }

        public DefSharedPtr(XmlElement elem)
            : base(elem)
        {
        }
    }
}