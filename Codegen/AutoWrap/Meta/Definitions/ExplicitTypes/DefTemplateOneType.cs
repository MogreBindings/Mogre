using System;
using System.Xml;

namespace AutoWrap.Meta
{
    public class DefTemplateOneType : DefTypeDef
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
                    return "STL" + STLContainer + "_" + TypeMembers[0].MemberType.Name;
                
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

        public override void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
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

        public override string GetCLRParamTypeName(DefParam param)
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

        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            newname = param.Name;
            return "";
        }

        public override string GetPostCallParamConversionCleanup(DefParam param)
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

        public override string GetNativeCallConversion(string expr, ITypeMember m)
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
                    return Name + "<" + TypeMembers[0].MemberTypeNativeName + ">";

                if (ProtectionLevel == ProtectionLevel.Protected)
                    return NativeProtectedTypesProxy.GetProtectedTypesProxyName(SurroundingClass) + "::" + Name;

                return base.FullNativeName;
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            if (typedef.IsSharedPtr)
                return DefSharedPtr.CreateExplicitType(typedef);
            
            if (IsIteratorWrapper(typedef))
                return DefIterator.CreateExplicitType(typedef);
            
            if (typedef.BaseTypeName.StartsWith("TRect"))
                return DefTRect.CreateExplicitType(typedef);
            
            return DefStdList.CreateExplicitType(typedef);
        }

        public DefTemplateOneType(XmlElement elem)
            : base(elem)
        {
        }
    }
}