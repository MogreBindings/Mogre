using System;
using System.Xml;

namespace AutoWrap.Meta
{
    public class DefIterator : DefTemplateOneType
    {
        public bool IsConstIterator
        {
            get { return Name.StartsWith("Const") && Char.IsUpper(Name["Const".Length]); }
        }

        public bool IsMapIterator
        {
            get { return TypeMembers[0].MemberType is DefTemplateTwoTypes; }
        }

        private ITypeMember _iterationElementType;
        public virtual ITypeMember IterationElementTypeMember
        {
            get
            {
                if (_iterationElementType == null)
                {
                    if (TypeMembers[0].MemberType is DefTemplateOneType)
                        _iterationElementType = (TypeMembers[0].MemberType as DefTemplateOneType).TypeMembers[0];
                    else if (TypeMembers[0].MemberType is DefTemplateTwoTypes)
                        _iterationElementType = (TypeMembers[0].MemberType as DefTemplateTwoTypes).TypeMembers[1];
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
                    if (TypeMembers[0].MemberType is DefTemplateTwoTypes)
                        _iterationKeyType = (TypeMembers[0].MemberType as DefTemplateTwoTypes).TypeMembers[0];
                }

                return _iterationKeyType;
            }
        }

        public override string ProducePreCallParamConversionCode(DefParam param, out string newname)
        {
            newname = param.Name;
            return String.Empty;
        }

        public override string ProducePostCallParamConversionCleanupCode(DefParam param)
        {
            return String.Empty;
        }

        public override string GetCLRParamTypeName(DefParam param)
        {
            switch (param.PassedByType)
            {
                case PassedByType.Value:
                    return FullCLRName + "^";
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
}