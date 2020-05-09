using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Spark.Engine.Search.Model;
using Spark.Search;

namespace Spark.Engine.Search
{
    public static class CriteriaExtensions
    {
        public static ModelInfo.SearchParamDefinition FindSearchParamDefinition(this Criterium param, string resourceType)
        {
            return param.SearchParameters?.FirstOrDefault(sp => sp.Resource == resourceType || sp.Resource == "Resource");
        }

        public static List<string> GetTargetedReferenceTypes(this Criterium chainCriterium, string resourceType)
        {

            if (chainCriterium.Operator != Operator.CHAIN)
                throw new ArgumentException("Targeted reference types are only relevent for chained criteria.");

            var critSp = chainCriterium.FindSearchParamDefinition(resourceType);
            var modifier = chainCriterium.Modifier;
            var nextInChain = (Criterium)chainCriterium.Operand;
            var nextParameter = nextInChain.ParamName;
            // The modifier contains the type of resource that the referenced resource must be. It is optional.
            // If not present, search all possible types of resources allowed at this reference.
            // If it is present, it should be of one of the possible types.

            var searchResourceTypes = GetTargetedReferenceTypes(critSp, modifier);

            // Afterwards, filter on the types that actually have the requested searchparameter.
            return searchResourceTypes.Where(rt => IndexFieldNames.All.Contains(nextParameter) || UniversalField.All.Contains(nextParameter) || ModelInfo.SearchParameters.Exists(sp => rt.Equals(sp.Resource) && nextParameter.Equals(sp.Name))).ToList();
        }

        public static List<string> GetTargetedReferenceTypes(ModelInfo.SearchParamDefinition parameter, String modifier)
        {
            var allowedResourceTypes = parameter.Target.Select(t => EnumUtility.GetLiteral(t)).ToList();// ModelInfo.SupportedResources; //TODO: restrict to parameter.ReferencedResources. This means not making this static, because you want to use IFhirModel.
            List<string> searchResourceTypes = new List<string>();
            if (String.IsNullOrEmpty(modifier))
                searchResourceTypes.AddRange(allowedResourceTypes);
            else if (allowedResourceTypes.Contains(modifier))
            {
                searchResourceTypes.Add(modifier);
            }
            else
            {
                throw new NotSupportedException(String.Format("Referenced type cannot be of type %s.", modifier));
            }

            return searchResourceTypes;
        }
    }
}
