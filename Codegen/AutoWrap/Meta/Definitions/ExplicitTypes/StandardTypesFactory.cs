using System;

namespace AutoWrap.Meta
{
    public class StandardTypesFactory
    {
        public virtual AbstractTypeDefinition FindStandardType(TypedefDefinition typedef)
        {
            AbstractTypeDefinition expl = null;

            if (typedef.BaseTypeName.Contains("<") || typedef.BaseTypeName.Contains("std::") || Mogre17.IsCollection(typedef.BaseTypeName))
            {
                if (typedef.BaseTypeName == "std::vector" || typedef.BaseTypeName == "std::list")
                    expl = DefTemplateOneType.CreateExplicitType(typedef);
                else
                {
                    switch (typedef.TypeNames.Length)
                    {
                        case 1:
                            expl = DefTemplateOneType.CreateExplicitType(typedef);
                            break;
                        case 2:
                            expl = DefTemplateTwoTypes.CreateExplicitType(typedef);
                            break;
                        default:
                            throw new Exception("Unexpected");
                    }
                }
            }

            if (expl == null)
                throw new ArgumentException("Unsupported or unknown standard type: " + typedef.BaseTypeName);

            return expl;
        }
    }
}