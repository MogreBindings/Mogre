﻿using System;
using System.Xml;

namespace AutoWrap.Meta
{
    public class DefEnum : DefType
    {
        public override bool IsValueType
        {
            get { return true; }
        }

        public override string FullNativeName
        {
            get
            {
                if (ProtectionType == ProtectionType.Protected)
                    return NativeProtectedTypesProxy.GetProtectedTypesProxyName(ParentClass) + "::" + Name;
                
                return base.FullNativeName;
            }
        }

        public override void GetDefaultParamValueConversion(DefParam param, out string preConversion, out string conversion, out string postConversion, out DefType dependancyType)
        {
            preConversion = postConversion = "";
            dependancyType = null;
            switch (param.PassedByType)
            {
                case PassedByType.Pointer:
                    preConversion = FullCLRName + " out_" + param.Name + ";";
                    conversion = "out_" + param.Name;
                    return;
                default:
                    conversion = FullCLRName + "::" + param.DefaultValue;
                    return;
            }
        }

        public override string GetPreCallParamConversion(DefParam param, out string newname)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Reference:
                    newname = "*(" + FullNativeName + "*)p_" + param.Name;
                    return "pin_ptr<" + FullCLRName + "> p_" + param.Name + " = &" + param.Name + ";\n";
                case PassedByType.Pointer:
                    newname = "(" + FullNativeName + "*)p_" + param.Name;
                    return "pin_ptr<" + FullCLRName + "> p_" + param.Name + " = &" + param.Name + ";\n";
                case PassedByType.Value:
                    newname = "(" + FullNativeName + ")" + param.Name;
                    return string.Empty;
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
                    return "[Out] " + FullCLRName + "%";
                default:
                    return GetCLRTypeName(param);
            }
        }

        public override string GetNativeCallConversion(string expr, ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                    return "(" + FullCLRName + ")" + expr;
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override string GetCLRTypeName(ITypeMember m)
        {
            switch (m.PassedByType)
            {
                case PassedByType.Value:
                    return FullCLRName;
                default:
                    throw new Exception("Unexpected");
            }
        }

        public override void GetNativeParamConversion(DefParam param, out string preConversion, out string conversion, out string postConversion)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Pointer:
                    preConversion = FullCLRName + " out_" + param.Name + ";";
                    conversion = "out_" + param.Name;
                    postConversion = "if (" + param.Name + ") *" + param.Name + " = (" + FullNativeName + ")out_" + param.Name + ";";
                    return;
            }

            base.GetNativeParamConversion(param, out preConversion, out conversion, out postConversion);
        }

        private string[] _nativeEnumValues;

        public string[] NativeEnumValues
        {
            get
            {
                if (_nativeEnumValues == null)
                    FillEnumValues();

                return _nativeEnumValues;
            }
        }

        private string[] _CLREnumValues;

        public string[] CLREnumValues
        {
            get
            {
                if (_CLREnumValues == null)
                    FillEnumValues();

                return _CLREnumValues;
            }
        }

        private void FillEnumValues()
        {
            _nativeEnumValues = new string[_elem.ChildNodes.Count];
            _CLREnumValues = new string[_elem.ChildNodes.Count];

            for (int i = 0; i < _elem.ChildNodes.Count; i++)
            {
                string parentName = this.FullNativeName;
                parentName = parentName.Substring(0, parentName.LastIndexOf("::"));
                string nativeName = (_elem.ChildNodes[i] as XmlElement).GetAttribute("name");
                //if (ProtectionType == ProtectionType.Protected)
                //    nativeName = "PUBLIC_" + nativeName;

                _nativeEnumValues[i] = parentName + "::" + nativeName;
                _CLREnumValues[i] = (_elem.ChildNodes[i] as XmlElement).GetAttribute("name");
            }
        }

        public DefEnum(XmlElement elem)
            : base(elem)
        {
            if (elem.Name != "enumeration")
                throw new Exception("Not enumeration element");

            #region Code to convert enumeration strings to CamelCase form (commented out)

            // Convert the enumeration strings from UPPER_CASE form to CamelCase

            //string prefix = null;
            //int index = this.CLREnums[0].IndexOf("_");
            //if (index != -1)
            //{
            //    prefix = this.CLREnums[0].Substring(0, index + 1);

            //    foreach (string enumstr in CLREnums)
            //    {
            //        if (!enumstr.StartsWith(prefix)
            //            || !Char.IsLetter(enumstr[prefix.Length]))
            //        {
            //            prefix = null;
            //            break;
            //        }
            //    }
            //}

            //if (prefix != null)
            //{
            //    //Remove the prefix string
            //    for (int i=0; i < CLREnums.Length; i++)
            //        CLREnums[i] = CLREnums[i].Substring(prefix.Length);
            //}

            //for (int i=0; i < CLREnums.Length; i++)
            //{
            //    if (CLREnums[i] == CLREnums[i].ToUpper())
            //    {
            //        string[] words = CLREnums[i].Split('_');
            //        for (int k = 0; k < words.Length; k++)
            //        {
            //            string w = words[k].ToLower();
            //            if (w == "")
            //                continue;

            //            words[k] = Char.ToUpper(w[0]) + w.Substring(1);
            //        }

            //        string res = String.Join("", words);
            //        if (Char.IsLetter(res[0]))
            //            CLREnums[i] = res;
            //    }
            //}

            #endregion
        }
    }
}