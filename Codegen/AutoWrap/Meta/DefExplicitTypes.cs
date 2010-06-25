#region GPL license
/*
 * This source file is part of the AutoWrap code generator of the
 * MOGRE project (http://mogre.sourceforge.net).
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
using System.Xml;

namespace AutoWrap.Meta
{
    public class DefTemplateOneType : DefTypeDef
    {
        public override bool IsValueType
        {
            get
            {
                return false;
            }
        }

        public override string CLRName
        {
            get
            {
                if (IsUnnamedSTLContainer)
                    return "STL" + STLContainer + "_" + TypeMembers[0].Type.Name;
                else
                    return base.CLRName;
            }
        }

        public override string FullCLRName
        {
            get
            {
                if (IsUnnamedSTLContainer)
                    return CLRName;
                else
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
                    else
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
                    else
                        return FullCLRName + "^";
                case PassedByType.Value:
                case PassedByType.PointerPointer:
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
                    else
                        return FullCLRName + "^";
                case PassedByType.Value:
                    if (m.IsConst || this.IsReadOnly)
                        return FullCLRName.Replace(CLRName, "Const_" + CLRName) + "^";
                    else
                        return FullCLRName + "^";
                case PassedByType.PointerPointer:
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
                    if (m.IsConst || this.IsReadOnly)
                        return FullCLRName + "::ByValue( " + expr + " )->ReadOnlyInstance";
                    else
                        return FullCLRName + "::ByValue( " + expr + " )";
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string FullNativeName
        {
            get
            {
                if (Name.StartsWith("std::"))
                    return Name + "<" + TypeMembers[0].NativeTypeName + ">";
                else if (ProtectionType == ProtectionType.Protected)
                {
                    return NativeProtectedTypesProxy.GetProtectedTypesProxyName(ParentClass) + "::" + Name;
                }
                else
                    return base.FullNativeName;
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            if (typedef.IsSharedPtr)
                return DefSharedPtr.CreateExplicitType(typedef);
            else if (Producer.IsIteratorWrapper(typedef))
                return DefIterator.CreateExplicitType(typedef);
            else if (typedef.BaseTypeName.StartsWith("TRect"))
                return DefTRect.CreateExplicitType(typedef);
            else
                return DefStdList.CreateExplicitType(typedef);
        }

        public DefTemplateOneType(XmlElement elem)
            : base(elem)
        {
        }
    }

    class DefTemplateTwoTypes : DefTypeDef
    {
        public override bool IsValueType
        {
            get
            {
                return false;
            }
        }

        public override string CLRName
        {
            get
            {
                if (IsUnnamedSTLContainer)
                    return "STL" + STLContainer + "_" + TypeMembers[0].Type.Name + "_" + TypeMembers[1].Type.Name;
                else
                    return base.CLRName;
            }
        }

        public override string FullCLRName
        {
            get
            {
                if (IsUnnamedSTLContainer)
                    return CLRName;
                else
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
                    else
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
                    else
                        return FullCLRName + "^";
                case PassedByType.Value:
                case PassedByType.PointerPointer:
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
                    else
                        return FullCLRName + "^";
                case PassedByType.Value:
                    if (m.IsConst || this.IsReadOnly)
                        return FullCLRName.Replace(CLRName, "Const_" + CLRName) + "^";
                    else
                        return FullCLRName + "^";
                case PassedByType.PointerPointer:
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
                    if (m.IsConst || this.IsReadOnly)
                        return FullCLRName + "::ByValue( " + expr + " )->ReadOnlyInstance";
                    else
                        return FullCLRName + "::ByValue( " + expr + " )";
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string FullNativeName
        {
            get
            {
                if (Name.StartsWith("std::"))
                    return Name + "<" + TypeMembers[0].NativeTypeName + "," + TypeMembers[1].NativeTypeName + ">";
                else if (ProtectionType == ProtectionType.Protected)
                {
                    return NativeProtectedTypesProxy.GetProtectedTypesProxyName(ParentClass) + "::" + Name;
                }
                else
                    return base.FullNativeName;
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
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

    class DefStdList : DefTemplateOneType
    {
        public override bool IsUnnamedSTLContainer
        {
            get
            {
                return Name.StartsWith("std::");
            }
        }

        public override string STLContainer
        {
            get { return "List"; }
        }

        public override string FullSTLContainerTypeName
        {
            get
            {
                return "STLList<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ">";
            }
        }

        public virtual string NativeCallConversionFunction
        {
            get { return "GetArrayFromList"; }
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
        //            {
        //                preConversion = "array<" + TypeMembers[0].CLRTypeName + ">^ out_" + param.Name + ";";
        //                conversion = "out_" + param.Name;
        //            }
        //            break;
        //        default:
        //            base.GetDefaultParamValueConversion(param, out preConversion, out conversion, out postConversion);
        //            break;
        //    }
        //}

        //public override string GetCLRParamTypeName(DefParam param)
        //{
        //    switch (param.PassedByType)
        //    {
        //        case PassedByType.Reference:
        //        case PassedByType.Pointer:
        //            if (param.IsConst)
        //                return "Collections::Generic::List<" + TypeMembers[0].CLRTypeName + ">^";
        //            else
        //                return "[Out] array<" + TypeMembers[0].CLRTypeName + ">^%";
        //        case PassedByType.Value:
        //            return "Collections::Generic::List<" + TypeMembers[0].CLRTypeName + ">^";
        //        case PassedByType.PointerPointer:
        //        default:
        //            throw new Exception("Unexpected");
        //    }
        //}

        //public override string GetPreCallParamConversion(DefParam param, out string newname)
        //{
        //    string expr;
        //    switch (param.PassedByType)
        //    {
        //        case PassedByType.Reference:
        //            if (param.IsConst)
        //            {
        //                expr = FullNativeName + " o_" + param.Name + ";\n";
        //                expr += "FillListFromGenericList<" + FullNativeName + ", " + TypeMembers[0].CLRTypeName + ">(o_" + param.Name + ", " + param.Name + ");\n";
        //                newname = "o_" + param.Name;
        //                return expr;
        //            }
        //            else
        //            {
        //                newname = "o_" + param.Name;
        //                return FullNativeName + " o_" + param.Name + ";\n";
        //            }
        //        case PassedByType.Pointer:
        //            if (param.IsConst)
        //            {
        //                expr = FullNativeName + "* p_" + param.Name + " = 0;\n";
        //                expr += FullNativeName + " o_" + param.Name + ";\n";
        //                expr += "if (" + param.Name + " != CLR_NULL)\n{\n";
        //                expr += "\tFillListFromGenericList<" + FullNativeName + ", " + TypeMembers[0].CLRTypeName + ">(o_" + param.Name + ", " + param.Name + ");\n";
        //                expr += "\tp_" + param.Name + " = &o_" + param.Name + ";\n";
        //                expr += "}\n";
        //                newname = "p_" + param.Name;
        //                return expr;
        //            }
        //            else
        //            {
        //                newname = "&o_" + param.Name;
        //                return FullNativeName + " o_" + param.Name + ";\n";
        //            }
        //        case PassedByType.Value:
        //            expr = FullNativeName + " o_" + param.Name + ";\n";
        //            expr += "FillListFromGenericList<" + FullNativeName + ", " + TypeMembers[0].CLRTypeName + ">(o_" + param.Name + ", " + param.Name + ");\n";
        //            newname = "o_" + param.Name;
        //            return expr;
        //        case PassedByType.PointerPointer:
        //        default:
        //            throw new Exception("Unexpected");
        //    }
        //}

        //public override string GetPostCallParamConversionCleanup(DefParam param)
        //{
        //    switch (param.PassedByType)
        //    {
        //        case PassedByType.Reference:
        //        case PassedByType.Pointer:
        //            if (param.IsConst)
        //            {
        //                return base.GetPostCallParamConversionCleanup(param);
        //            }
        //            else
        //            {
        //                return param.Name + " = " + NativeCallConversionFunction + "<" + TypeMembers[0].CLRTypeName + ", " + FullNativeName + ">(o_" + param.Name + ");\n";
        //            }
        //        default:
        //            return base.GetPostCallParamConversionCleanup(param);
        //    }
        //}

        //public override string GetCLRTypeName(ITypeMember m)
        //{
        //    if (IsUnnamedSTLContainer)
        //    {
        //        switch (m.PassedByType)
        //        {
        //            case PassedByType.Value:
        //            case PassedByType.Pointer:
        //            case PassedByType.Reference:
        //                return "array<" + TypeMembers[0].CLRTypeName + ">^";
        //            case PassedByType.PointerPointer:
        //            default:
        //                throw new Exception("Unexpected");
        //        }
        //    }
        //    else
        //        return base.GetCLRTypeName(m);
        //}

        //public override string GetNativeCallConversion(string expr, ITypeMember m)
        //{
        //    if (IsUnnamedSTLContainer)
        //    {
        //        switch (m.PassedByType)
        //        {
        //            case PassedByType.Value:
        //            case PassedByType.Reference:
        //                return NativeCallConversionFunction + "<" + TypeMembers[0].CLRTypeName + ", " + FullNativeName + ">( " + expr + " )";
        //            case PassedByType.Pointer:
        //                return NativeCallConversionFunction + "<" + TypeMembers[0].CLRTypeName + ", " + FullNativeName + ">( *(" + expr + ") )";
        //            case PassedByType.PointerPointer:
        //            default:
        //                throw new Exception("Unexpected");
        //        }
        //    }
        //    else
        //        return base.GetNativeCallConversion(expr, m);
        //}

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
			string baseTypeName = Mogre17.GetBaseType(typedef);

			switch (baseTypeName)
            {
                case "std::vector":
                    return DefStdVector.CreateExplicitType(typedef);
                case "std::set":
                    return DefStdSet.CreateExplicitType(typedef);
                case "std::deque":
                    return DefStdDeque.CreateExplicitType(typedef);
                case "std::list":
                    return new DefStdList(typedef.Element);
                case "HashedVector":
                    return DefHashedVector.CreateExplicitType(typedef);
                case "std::map":
                    return DefStdMap.CreateExplicitType(typedef);
                default:
                    throw new Exception("Unexpected");
            }
        }

        public DefStdList(XmlElement elem) : base(elem)
        {
        }
    }

    class DefHashedVector : DefStdVector
    {
        public override string STLContainer
        {
            get { return "HashedVector"; }
        }

        public override string FullSTLContainerTypeName
        {
            get
            {
                return "STLHASHEDVECTOR<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ">";
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefHashedVector(typedef.Element);
        }

        public DefHashedVector(XmlElement elem)
            : base(elem)
        {
        }

        
    }

    class DefStdVector : DefStdList
    {
        public override string STLContainer
        {
            get { return "Vector"; }
        }

        public override string FullSTLContainerTypeName
        {
            get
            {
                return "STLVector<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ">";
            }
        }

        public override string NativeCallConversionFunction
        {
            get { return "GetArrayFromVector"; }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefStdVector(typedef.Element);
        }

        public DefStdVector(XmlElement elem)
            : base(elem)
        {
        }
    }

    class DefStdDeque : DefStdVector
    {
        public override string STLContainer
        {
            get { return "Deque"; }
        }

        public override string FullSTLContainerTypeName
        {
            get
            {
                return "STLDeque<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ">";
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefStdDeque(typedef.Element);
        }

        public DefStdDeque(XmlElement elem)
            : base(elem)
        {
        }
    }

    class DefStdSet : DefStdList
    {
        public override string STLContainer
        {
            get { return "Set"; }
        }

        public override string FullSTLContainerTypeName
        {
            get
            {
                return "STLSet<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ">";
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefStdSet(typedef.Element);
        }

        public DefStdSet(XmlElement elem)
            : base(elem)
        {
        }
    }

    class DefStdPair : DefTemplateTwoTypes
    {
        public override string STLContainer
        {
            get { return "Pair"; }
        }

        public override string FullSTLContainerTypeName
        {
            get
            {
                return ConversionTypeName;
            }
        }

        public virtual string ConversionTypeName
        {
            get
            {
                return "Pair<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ">";
            }
        }

        public override string GetCLRParamTypeName(DefParam param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Value:
                    return ConversionTypeName;
                case PassedByType.Pointer:
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Value:
                    newname = "ToNative<" + ConversionTypeName + ", " + FullNativeName + ">( " + param.Name + ")";
                    return "";
                case PassedByType.Pointer:
                case PassedByType.PointerPointer:
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
                    return ConversionTypeName;
                case PassedByType.PointerPointer:
                case PassedByType.Pointer:
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
                    return "ToManaged<" + ConversionTypeName + ", " + FullNativeName + ">( " + expr + " )";
                case PassedByType.PointerPointer:
                case PassedByType.Pointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefStdPair(typedef.Element);
        }

        public DefStdPair(XmlElement elem)
            : base(elem)
        {
        }
    }

    class DefStdMap : DefTemplateTwoTypes
    {
        public override bool IsUnnamedSTLContainer
        {
            get
            {
                return Name.StartsWith("std::");
            }
        }

        public override string STLContainer
        {
            get { return "Map"; }
        }

        public override string FullSTLContainerTypeName
        {
            get
            {
                return "STLMap<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ", " + TypeMembers[1].NativeTypeName + ">";
            }
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
                    else
                        throw new Exception("Unexpected");
                }
                else
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
                    else
                        throw new Exception("Unexpected");
                }
                else
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
                    else
                        throw new Exception("Unexpected");
                }
                else
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
                    else
                        throw new Exception("Unexpected");
                case PassedByType.PointerPointer:
                case PassedByType.Value:
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
                    else
                        throw new Exception("Unexpected");
                case PassedByType.PointerPointer:
                case PassedByType.Value:
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

    class DefStdMultiMap : DefStdMap
    {
        public override string STLContainer
        {
            get { return "MultiMap"; }
        }

        public override string FullSTLContainerTypeName
        {
            get
            {
                return "STLMultiMap<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ", " + TypeMembers[1].NativeTypeName + ">";
            }
        }

        //public override string GetCLRTypeName(ITypeMember m)
        //{
        //    switch (m.PassedByType)
        //    {
        //        case PassedByType.Reference:
        //            return "Collections::Generic::SortedList<" + TypeMembers[0].CLRTypeName + ", Collections::Generic::List<" + TypeMembers[1].CLRTypeName + ">^>^";
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
        //            return "GetSortedListFromMultiMap<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ", " + FullNativeName + ">( " + expr + ")";
        //        case PassedByType.PointerPointer:
        //        case PassedByType.Value:
        //        case PassedByType.Pointer:
        //        default:
        //            throw new Exception("Unexpected");
        //    }
        //}

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefStdMultiMap(typedef.Element);
        }

        public DefStdMultiMap(XmlElement elem)
            : base(elem)
        {
        }
    }

    class DefStdHashMap : DefStdMap
    {
        public override string STLContainer
        {
            get { return "HashMap"; }
        }

        public override string FullSTLContainerTypeName
        {
            get
            {
                return "STLHashMap<" + TypeMembers[0].CLRTypeName + ", " + TypeMembers[1].CLRTypeName + ", " + TypeMembers[0].NativeTypeName + ", " + TypeMembers[1].NativeTypeName + ">";
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefStdHashMap(typedef.Element);
        }

        public DefStdHashMap(XmlElement elem)
            : base(elem)
        {
        }
    }

    class DefSharedPtr : DefTemplateOneType
    {
        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            newname = "(" + param.NativeTypeName + ")" + param.Name;
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
                case PassedByType.PointerPointer:
                case PassedByType.Pointer:
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
                case PassedByType.PointerPointer:
                case PassedByType.Pointer:
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

    public class DefIterator : DefTemplateOneType
    {
        public bool IsConstIterator
        {
            get
            {
                return Name.StartsWith("Const") && Char.IsUpper(Name["Const".Length]);
            }
        }

        public bool IsMapIterator
        {
            get
            {
                return TypeMembers[0].Type is DefTemplateTwoTypes;
            }
        }

        private ITypeMember _iterationElementType;
        public virtual ITypeMember IterationElementTypeMember
        {
            get
            {
                if (_iterationElementType == null)
                {
                    if (TypeMembers[0].Type is DefTemplateOneType)
                    {
                        _iterationElementType = (TypeMembers[0].Type as DefTemplateOneType).TypeMembers[0];
                    }
                    else if (TypeMembers[0].Type is DefTemplateTwoTypes)
                    {
                        _iterationElementType = (TypeMembers[0].Type as DefTemplateTwoTypes).TypeMembers[1];
                    }
                    else
                        throw new Exception("Unexpected");
                }

                return _iterationElementType;
            }
        }

        private ITypeMember _iterationKeyType;
        public virtual ITypeMember IterationKeyTypeMember
        {
            get
            {
                if (_iterationKeyType == null)
                {
                    if (TypeMembers[0].Type is DefTemplateTwoTypes)
                    {
                        _iterationKeyType = (TypeMembers[0].Type as DefTemplateTwoTypes).TypeMembers[0];
                    }
                }

                return _iterationKeyType;
            }
        }

        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            newname = param.Name;
            return String.Empty;
        }
        public override string GetPostCallParamConversionCleanup(DefParam param)
        {
            return String.Empty;
        }

        public override string GetCLRParamTypeName(DefParam param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Value:
                    return FullCLRName + "^";
                case PassedByType.Reference:
                case PassedByType.PointerPointer:
                case PassedByType.Pointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                    return FullCLRName + "^";
                case PassedByType.Reference:
                case PassedByType.PointerPointer:
                case PassedByType.Pointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetNativeCallConversion(string expr, ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                    return expr;
                case PassedByType.Reference:
                case PassedByType.PointerPointer:
                case PassedByType.Pointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefIterator(typedef.Element);
        }

        public DefIterator(XmlElement elem)
            : base(elem)
        {
        }
    }

    class DefTRect : DefTemplateOneType
    {
        public override bool IsValueType
        {
            get
            {
                return true;
            }
        }

        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            if (param.PassedByType == PassedByType.Pointer)
                newname = "(" + param.Type.FullNativeName + "*) " + param.Name;
            else
                newname = param.Name;
            return string.Empty;
        }
        public override string GetPostCallParamConversionCleanup(DefParam param)
        {
            return string.Empty;
        }

        public override string GetCLRParamTypeName(DefParam param)
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
                case PassedByType.PointerPointer:
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
                case PassedByType.PointerPointer:
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
                case PassedByType.Pointer:
                    return "(" + m.CLRTypeName + ") " + expr;
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public new static DefTypeDef CreateExplicitType(DefTypeDef typedef)
        {
            return new DefTRect(typedef.Element);
        }

        public DefTRect(XmlElement elem)
            : base(elem)
        {
        }
    }

    interface IDefString
    {
    }

    class DefStringTypeDef : DefTypeDef, IDefString
    {
        public override bool IsValueType
        {
            get
            {
                return true;
            }
        }

        public override void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            preConversion = postConversion = "";
            dependancyType = null;
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Value:
                    conversion = param.DefaultValue.Trim();
                    if (!conversion.StartsWith("\"")
                        && conversion.Contains("::"))
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
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetPreCallParamConversion(DefParam param, out string newname)
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
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetPostCallParamConversionCleanup(DefParam param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "";
                case PassedByType.Pointer:
                    return "delete[] arr_" + param.Name + ";\n";
                case PassedByType.PointerPointer:
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
                case PassedByType.Pointer:
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetNativeCallConversion(string expr, ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "TO_CLR_STRING( " + expr + " )";
                case PassedByType.Pointer:
                    return "gcnew array<String^> { (" + expr + " ? TO_CLR_STRING( *" + expr + " ) : String::Empty) }";
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public DefStringTypeDef(XmlElement elem)
            : base(elem)
        {
        }
    }

    class DefString : DefType, IDefString
    {
        public override string FullNativeName
        {
            get
            {
                return "std::string";
            }
        }

        public override bool IsValueType
        {
            get
            {
                return true;
            }
        }

        public override void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            preConversion = postConversion = "";
            dependancyType = null;
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                case PassedByType.Value:
                    conversion = param.DefaultValue.Trim();
                    if (!conversion.StartsWith("\"")
                        && conversion.Contains("::"))
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
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetPreCallParamConversion(DefParam param, out string newname)
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
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetPostCallParamConversionCleanup(DefParam param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "";
                case PassedByType.Pointer:
                    return "delete[] arr_" + param.Name + ";\n";
                case PassedByType.PointerPointer:
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
                case PassedByType.Pointer:
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetNativeCallConversion(string expr, ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "TO_CLR_STRING( " + expr + " )";
                case PassedByType.Pointer:
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public DefString()
        {
        }
    }

    class DefUtfString : DefType, IDefString {
        public override string FullNativeName {
            get {
                return "DisplayString";
            }
        }

        public override bool IsValueType {
            get {
                return true;
            }
        }

        public override void GetDefaultParamValueConversion (DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType) {
            preConversion = postConversion = "";
            dependancyType = null;
            switch (param.PassedByType) {
                case PassedByType.Reference:
                case PassedByType.Value:
                    conversion = param.DefaultValue.Trim ();
                    if (!conversion.StartsWith ("\"")
                        && conversion.Contains ("::")) {
                        //It's a static string of a class

                        if (conversion == "StringUtil::BLANK") {
                            //Manually translate "StringUtil::BLANK" so that there's no need to wrap the StringUtil class
                            conversion = "String::Empty";
                            return;
                        }

                        string name = conversion.Substring (0, conversion.LastIndexOf ("::"));
                        dependancyType = FindType<DefType> (name);
                    }
                    break;
                default:
                    throw new Exception ("Unexpected");

            }
        }

        public override string GetCLRParamTypeName (DefParam param) {
            switch (param.PassedByType) {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "String^";
                case PassedByType.Pointer:
                    return "array<String^>^";
                case PassedByType.PointerPointer:
                default:
                    throw new Exception ("Unexpected");
            }
        }

        public override string GetPreCallParamConversion (DefParam param, out string newname) {
            string name = param.Name;
            switch (param.PassedByType) {
                case PassedByType.Value:
                case PassedByType.Reference:
                    newname = "o_" + name;
                    return "DECLARE_NATIVE_UTFSTRING( o_" + name + ", " + name + " )\n";
                case PassedByType.Pointer:
                    string expr = FullNativeName + "* arr_" + name + " = new " + FullNativeName + "[" + name + "->Length];\n";
                    expr += "for (int i=0; i < " + name + "->Length; i++)\n";
                    expr += "{\n";
                    expr += "\tSET_NATIVE_UTFSTRING( arr_" + name + "[i], " + name + "[i] )\n";
                    expr += "}\n";
                    newname = "arr_" + name;
                    return expr;
                case PassedByType.PointerPointer:
                default:
                    throw new Exception ("Unexpected");
            }
        }

        public override string GetPostCallParamConversionCleanup (DefParam param) {
            switch (param.PassedByType) {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "";
                case PassedByType.Pointer:
                    return "delete[] arr_" + param.Name + ";\n";
                case PassedByType.PointerPointer:
                default:
                    throw new Exception ("Unexpected");
            }
        }

        public override string GetCLRTypeName (ITypeMember m) {
            switch (m.PassedByType) {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "String^";
                case PassedByType.Pointer:
                case PassedByType.PointerPointer:
                default:
                    throw new Exception ("Unexpected");
            }
        }

        public override string GetNativeCallConversion (string expr, ITypeMember m) {
            switch (m.PassedByType) {
                case PassedByType.Value:
                case PassedByType.Reference:
                    return "UTF_TO_CLR_STRING( " + expr + " )";
                case PassedByType.Pointer:
                case PassedByType.PointerPointer:
                default:
                    throw new Exception ("Unexpected");
            }
        }

        public DefUtfString () {
        }
    }

    class DefInternal : DefType
    {
        public override void GetNativeParamConversion(DefParam param, out string preConversion, out string conversion, out string postConversion)
        {
            preConversion = postConversion = null;
            conversion = param.Name;

            switch (param.PassedByType)
            {
                case PassedByType.Value:
                case PassedByType.Reference:
                    break;
                case PassedByType.Pointer:
                    if (!IsVoid)
                    {
                        if (!param.IsConst)
                        {
                            conversion = "*" + param.Name;
                        }
                        else
                        {
                            //Treat it as array
                            throw new Exception("Unexpected");
                        }
                    }
                    break;
                case PassedByType.PointerPointer:
                    throw new Exception("Unexpected");
            }
        }

        public override void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            preConversion = postConversion = "";
            dependancyType = null;
            if (IsVoid)
            {
                conversion = param.DefaultValue;
                return;
            }

            switch (param.PassedByType)
            {
                case PassedByType.Pointer:
                    if (!param.IsConst)
                    {
                        preConversion = FullCLRName + " out_" + param.Name + ";";
                        conversion = "out_" + param.Name;
                        return;
                    }
                    else
                    {
                        throw new Exception("Unexpected");
                    }
                default:
                    conversion = param.DefaultValue;
                    break;
            }
        }

        public override string GetCLRParamTypeName(DefParam param)
        {
            if (IsVoid)
            {
                string name = "void";
                if (param.IsConst) name = "const " + name;

                switch (param.PassedByType)
                {
                    case PassedByType.Value:
                        break;
                    case PassedByType.Pointer:
                        name += "*";
                        break;
                    case PassedByType.PointerPointer:
                        name += "*%";
                        break;
                    case PassedByType.Reference:
                        throw new Exception("Unexpected");
                }
                return name;
            }

            switch (param.PassedByType)
            {
                case PassedByType.Value:
                    return Name;
                case PassedByType.Reference:
                case PassedByType.Pointer:
                    if (!param.IsConst && !param.HasAttribute<RawPointerParamAttribute>())
                    {
                        return "[Out] " + Name + "%";
                    }
                    else
                    {
                        //Treat it as array
                        //return "array<" + FullCLRName + ">^";
                        string name = Name + "*";
                        if (param.IsConst) name = "const " + name;
                        return name;
                    }
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            if (IsVoid)
            {
                newname = param.Name;
                return "";
            }

            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                    newname = "*p_" + param.Name;
                    return "pin_ptr<" + FullCLRName + "> p_" + param.Name + " = &" + param.Name + ";\n";
                case PassedByType.Pointer:
                    if (!param.IsConst && !param.HasAttribute<RawPointerParamAttribute>())
                    {
                        newname = "p_" + param.Name;
                        return "pin_ptr<" + FullCLRName + "> p_" + param.Name + " = &" + param.Name + ";\n";
                    }
                    else
                    {
                        //Treat it as array
                        //newname = "arr_" + param.Name;
                        //string expr = FullNativeName + "* arr_" + param.Name + " = new " + FullNativeName + "[" + param.Name + "->Length];\n";
                        //expr += "pin_ptr<" + FullCLRName + "> src_" + param.Name + " = &" + param.Name + "[0];\n";
                        //expr += "memcpy(arr_" + param.Name + ", src_" + param.Name + ", " + param.Name + "->Length * sizeof(" + FullNativeName + "));\n";
                        //return expr;
                        newname = param.Name;
                        return "";
                    }
                default:
                    return base.GetPreCallParamConversion(param, out newname);
            }
        }

        public override string GetPostCallParamConversionCleanup(DefParam param)
        {
            if (IsVoid)
                return "";

            switch (param.PassedByType)
            {
                case PassedByType.Pointer:
                    if (param.IsConst)
                    {
                        //Treat it as array
                        //return "delete[] arr_" + param.Name + ";";
                        return "";
                    }
                    else
                        return base.GetPostCallParamConversionCleanup(param);
                default:
                    return base.GetPostCallParamConversionCleanup(param);
            }
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                    return Name;
                case PassedByType.Pointer:
                    if (IsVoid)
                    {
                        return "void*";
                    }
                    else if (m.HasAttribute<ArrayTypeAttribute>())
                    {
                        return "array<" + FullCLRName + ">^";
                    }
                    else
                    {
                        string name = Name + "*";
                        if (m.IsConst) name = "const " + name;
                        return name;
                    }
                case PassedByType.Reference:
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetNativeCallConversion(string expr, ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Reference:
                    //Could be called from NativeClass producer
                case PassedByType.Value:
                    return expr;
                case PassedByType.Pointer:
                    if (IsVoid)
                    {
                        return expr;
                    }
                    else if (m.HasAttribute<ArrayTypeAttribute>())
                    {
                        int len = m.GetAttribute<ArrayTypeAttribute>().Length;
                        return "GetValueArrayFromNativeArray<" + FullCLRName + ", " + FullNativeName + ">( " + expr + " , " + len + " )";
                    }
                    else
                        return expr;
                case PassedByType.PointerPointer:
                default:
                    throw new Exception("Unexpected");
            }
        }

        public virtual bool IsVoid
        {
            get { return (Name == "void"); }
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
        }

        public override string FullCLRName
        {
            get
            {
                return Name;
            }
        }

        public override string FullNativeName
        {
            get
            {
                return Name;
            }
        }

        public override bool IsValueType
        {
            get
            {
                return true;
            }
        }

        public DefInternal(string name)
        {
            if (name.StartsWith("const "))
                name = name.Substring("const ".Length);

            this._name = name;
        }
    }
}
