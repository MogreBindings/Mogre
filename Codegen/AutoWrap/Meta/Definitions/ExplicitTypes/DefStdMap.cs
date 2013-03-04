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
            get { return "STLMap<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ", " + TypeMembers[1].NativeTypeName + ">"; }
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
                if (TypeMembers[0].Type is IDefString || TypeMembers[1].Type is IDefString)
                {
                    if (TypeMembers[0].Type is IDefString && TypeMembers[1].Type is IDefString)
                        return "Collections::Specialized::NameValueCollection^";
                    
                    throw new Exception("Unexpected");
                }

                return "Collections::Generic::SortedList<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ">^";
            }
        }

        public virtual string PreCallConversionFunction
        {
            get
            {
                if (TypeMembers[0].Type is IDefString || TypeMembers[1].Type is IDefString)
                {
                    if (TypeMembers[0].Type is IDefString && TypeMembers[1].Type is IDefString)
                        return "FillMapFromNameValueCollection";
                    
                    throw new Exception("Unexpected");
                }
                
                return "FillMapFromSortedList<" + FullNativeName + ", " + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ">";
            }
        }

        public virtual string NativeCallConversionFunction
        {
            get
            {
                if (TypeMembers[0].Type is IDefString || TypeMembers[1].Type is IDefString)
                {
                    if (TypeMembers[0].Type is IDefString && TypeMembers[1].Type is IDefString)
                        return "GetNameValueCollectionFromMap";
                    
                    throw new Exception("Unexpected");
                }
                
                return "GetSortedListFromMap<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ", " + FullNativeName + ">";
            }
        }

        public override string GetCLRParamTypeName(DefParam param)
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

        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            if (!IsUnnamedSTLContainer)
                return base.GetPreCallParamConversion(param, out newname);

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

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefStdMap(typedef.Element);
        }

        public DefStdMap(XmlElement elem)
            : base(elem)
        {
        }
    }
}