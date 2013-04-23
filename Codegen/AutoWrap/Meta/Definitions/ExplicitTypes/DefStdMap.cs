using System;
using System.Xml;

namespace AutoWrap.Meta
{
    internal class DefStdMap : DefTemplateTwoTypes
    {
        public override bool IsUnnamedSTLContainer
        {
            get { return Name.StartsWith("std::"); }
        }

        public override string STLContainer
        {
            get { return "Map"; }
        }

        public override string FullSTLContainerTypeName
        {
            get { return "STLMap<" + TypeMembers[0].MemberTypeCLRName + ", " + TypeMembers[1].MemberTypeCLRName + ", " + TypeMembers[0].MemberTypeNativeName + ", " + TypeMembers[1].MemberTypeNativeName + ">"; }
        }

        //public override void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion)
        //{
        //    preConversion = postConversion = "";
        //    switch (param.PassedByType)
        //    {
        //        case PassedByType.Pointer:
        //            if (param.IsConst)
        //                conversion = "nullptr";
        //            else
        //                throw new Exception("Unexpected");
        //            break;
        //        default:
        //            throw new Exception("Unexpected");
        //    }
        //}

        public virtual string ConversionTypeName
        {
            get
            {
                if (TypeMembers[0].MemberType is IDefString || TypeMembers[1].MemberType is IDefString)
                {
                    if (TypeMembers[0].MemberType is IDefString && TypeMembers[1].MemberType is IDefString)
                        return "Collections::Specialized::NameValueCollection^";
                    
                    throw new Exception("Unexpected");
                }

                return "Collections::Generic::SortedList<" + TypeMembers[0].MemberTypeCLRName + ", " + TypeMembers[1].MemberTypeCLRName + ">^";
            }
        }

        public virtual string PreCallConversionFunction
        {
            get
            {
                if (TypeMembers[0].MemberType is IDefString || TypeMembers[1].MemberType is IDefString)
                {
                    if (TypeMembers[0].MemberType is IDefString && TypeMembers[1].MemberType is IDefString)
                        return "FillMapFromNameValueCollection";
                    
                    throw new Exception("Unexpected");
                }

                return "FillMapFromSortedList<" + FullNativeName + ", " + TypeMembers[0].MemberTypeCLRName + ", " + TypeMembers[1].MemberTypeCLRName + ">";
            }
        }

        public virtual string NativeCallConversionFunction
        {
            get
            {
                if (TypeMembers[0].MemberType is IDefString || TypeMembers[1].MemberType is IDefString)
                {
                    if (TypeMembers[0].MemberType is IDefString && TypeMembers[1].MemberType is IDefString)
                        return "GetNameValueCollectionFromMap";
                    
                    throw new Exception("Unexpected");
                }

                return "GetSortedListFromMap<" + TypeMembers[0].MemberTypeCLRName + ", " + TypeMembers[1].MemberTypeCLRName + ", " + FullNativeName + ">";
            }
        }

        public override string GetCLRParamTypeName(ParamDefinition param)
        {
            if (!IsUnnamedSTLContainer)
                return base.GetCLRParamTypeName(param);

            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                    return ConversionTypeName;
                case PassedByType.Pointer:
                    if (param.IsConst)
                        return ConversionTypeName;
                    
                        throw new Exception("Unexpected");
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string ProducePreCallParamConversionCode(ParamDefinition param, out string newname)
        {
            if (!IsUnnamedSTLContainer)
                return base.ProducePreCallParamConversionCode(param, out newname);

            string expr;
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                    expr = FullNativeName + " o_" + param.Name + ";\n";
                    expr += PreCallConversionFunction + "(o_" + param.Name + ", " + param.Name + ");\n";
                    newname = "o_" + param.Name;
                    return expr;
                case PassedByType.Pointer:
                    if (param.IsConst)
                    {
                        expr = FullNativeName + "* p_" + param.Name + " = 0;\n";
                        expr += FullNativeName + " o_" + param.Name + ";\n";
                        expr += "if (" + param.Name + " != CLR_NULL)\n{\n";
                        expr += "\t" + PreCallConversionFunction + "(o_" + param.Name + ", " + param.Name + ");\n";
                        expr += "\tp_" + param.Name + " = &o_" + param.Name + ";\n";
                        expr += "}\n";
                        newname = "p_" + param.Name;
                        return expr;
                    }
                    
                    throw new Exception("Unexpected");
                default:
                    throw new Exception("Unexpected");
            }
        }

        //public override string GetCLRTypeName(ITypeMember m)
        //{
        //    switch (m.PassedByType)
        //    {
        //        case PassedByType.Reference:
        //            return ConversionTypeName;
        //        case PassedByType.PointerPointer:
        //        case PassedByType.Value:
        //        case PassedByType.Pointer:
        //        default:
        //            throw new Exception("Unexpected");
        //    }
        //}

        //public override string GetNativeCallConversion(string expr, ITypeMember m)
        //{
        //    switch (m.PassedByType)
        //    {
        //        case PassedByType.Reference:
        //            return NativeCallConversionFunction + "( " + expr + ")";
        //        case PassedByType.PointerPointer:
        //        case PassedByType.Value:
        //        case PassedByType.Pointer:
        //        default:
        //            throw new Exception("Unexpected");
        //    }
        //}

        public new static TypedefDefinition CreateExplicitType(TypedefDefinition typedef)
        {
            return new DefStdMap(typedef.NameSpace, typedef.DefiningXmlElement);
        }

        public DefStdMap(NamespaceDefinition nsDef, XmlElement elem)
            : base(nsDef, elem)
        {
        }
    }
}