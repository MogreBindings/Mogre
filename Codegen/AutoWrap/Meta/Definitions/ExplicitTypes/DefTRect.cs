﻿using System;
using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefTRect : DefTemplateOneType
    {
        public override bool IsValueType
        {
            get { return true; }
        }

        public override string ProducePreCallParamConversionCode(ParamDefinition param, out string newname)
        {
            if (param.PassedByType == PassedByType.Pointer)
                newname = "(" + param.Type.FullNativeName + "*) " + param.Name;
            else
                newname = param.Name;
            return string.Empty;
        }

        public override string ProducePostCallParamConversionCleanupCode(ParamDefinition param)
        {
            return string.Empty;
        }

        public override string GetCLRParamTypeName(ParamDefinition param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Value:
                    if (param.PassedByType == PassedByType.Reference && !param.IsConst)
                        throw new Exception("Unexpected");
                    return FullCLRName;
                case PassedByType.Pointer:
                    return FullCLRName + "*";
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Value:
                    return FullCLRName;
                case PassedByType.Pointer:
                    return FullCLRName + "*";
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
                case PassedByType.Pointer:
                    return "(" + m.MemberTypeCLRName + ") " + expr;
                default:
                    throw new Exception("Unexpected");
            }
        }

        public new static TypedefDefinition CreateExplicitType(TypedefDefinition typedef)
        {
            return new DefTRect(typedef.NameSpace, typedef.DefiningXmlElement);
        }

        public DefTRect(NamespaceDefinition nsDef, XmlElement elem)
            : base(nsDef, elem)
        {
        }
    }
}