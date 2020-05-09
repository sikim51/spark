﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Spark.Engine.Search.Model;
using Spark.Search;
using SearchParamType = Hl7.Fhir.Model.SearchParamType;

namespace Spark.Engine.Search
{


    public abstract class FhirSearcher
    {

        public FhirSearcher(ILocalhost localhost, IFhirModel fhirModel)
        {
            Localhost = localhost;
            FhirModel = fhirModel;
        }

        protected ILocalhost Localhost { get; }
        protected IFhirModel FhirModel { get; }


        public SearchResults Search(string resourceType, SearchParams searchCommand)
        {
            SearchResults results = new SearchResults();

            var criteria = parseCriteria(searchCommand, results);

            if (!results.HasErrors)
            {
                results.UsedCriteria = criteria.Select(c => c.Clone()).ToList();

                criteria = EnrichCriteriaWithSearchParameters(FhirModel.GetResourceTypeForResourceName(resourceType),
                    results);

                var normalizedCriteria = NormalizeNonChainedReferenceCriteria(criteria, resourceType);
                var normalizeSortCriteria = NormalizeSortItems(resourceType, searchCommand);

                List<string> selfLinks = OnCollectSelfLinks(resourceType, normalizedCriteria, results, 0, normalizeSortCriteria).ToList();

                results.AddRange(selfLinks);
                results.MatchCount = selfLinks.Count;
            }

            return results;
        }

        protected abstract IEnumerable<string> OnCollectSelfLinks(string resourceType, IEnumerable<Criterium> criteria, SearchResults results, int level, IList<Tuple<string, SortOrder>> sortItems);

        protected virtual List<Criterium> parseCriteria(SearchParams searchCommand, SearchResults results)
        {
            var result = new List<Criterium>();
            foreach (var c in searchCommand.Parameters)
            {
                try
                {
                    result.Add(Criterium.Parse(c.Item1, c.Item2));
                }
                catch (Exception ex)
                {
                    results.AddIssue(String.Format("Could not parse parameter [{0}] for reason [{1}].", c.ToString(), ex.Message));
                }
            }
            return result;
        }

        protected virtual List<Criterium> EnrichCriteriaWithSearchParameters(ResourceType resourceType, SearchResults results)
        {
            var result = new List<Criterium>();
            var notUsed = new List<Criterium>();
            foreach (var crit in results.UsedCriteria)
            {
                if (TryEnrichCriteriumWithSearchParameters(crit, resourceType))
                {
                    result.Add(crit);
                }
                else
                {
                    notUsed.Add(crit);
                    results.AddIssue(String.Format("Parameter with name {0} is not supported for resource type {1}.", crit.ParamName, resourceType), OperationOutcome.IssueSeverity.Warning);
                }
            }

            results.UsedCriteria = results.UsedCriteria.Except(notUsed).ToList();

            return result;
        }

        protected virtual bool TryEnrichCriteriumWithSearchParameters(Criterium criterium, ResourceType resourceType)
        {
            var sp = FhirModel.FindSearchParameter(resourceType, criterium.ParamName);
            if (sp == null)
            {
                return false;
            }

            var result = true;

            var spDef = sp.GetOriginalDefinition();

            if (spDef != null)
            {
                criterium.SearchParameters.Add(spDef);
            }

            if (criterium.Operator == Operator.CHAIN)
            {
                var subCrit = (Criterium)(criterium.Operand);
                bool subCritResult = false;
                foreach (var targetType in criterium.SearchParameters.SelectMany(spd => spd.Target))
                {
                    //We're ok if at least one of the target types has this searchparameter.
                    subCritResult |= TryEnrichCriteriumWithSearchParameters(subCrit, targetType);
                }
                result &= subCritResult;
            }
            return result;
        }

        protected virtual Tuple<string, SortOrder> NormalizeSortItem(string resourceType, Tuple<string, SortOrder> sortItem)
        {
            ModelInfo.SearchParamDefinition definition =
                FhirModel.FindSearchParameter(resourceType, sortItem.Item1)?.GetOriginalDefinition();

            if (definition?.Type == SearchParamType.Token)
            {
                return new Tuple<string, SortOrder>(sortItem.Item1 + ".code", sortItem.Item2);
            }
            if (definition?.Type == SearchParamType.Date)
            {
                return new Tuple<string, SortOrder>(sortItem.Item1 + ".start", sortItem.Item2);
            }
            if (definition?.Type == SearchParamType.Quantity)
            {
                return new Tuple<string, SortOrder>(sortItem.Item1 + ".value", sortItem.Item2);
            }
            return sortItem;
        }

        protected virtual IList<Tuple<string, SortOrder>> NormalizeSortItems(string resourceType, SearchParams searchCommand)
        {
            var sortItems = searchCommand.Sort.Select(s => NormalizeSortItem(resourceType, s)).ToList();
            return sortItems;
        }

        /// <summary>
        /// Change something like Condition/subject:Patient=Patient/10014 
        /// to Condition/subject:Patient.internal_id=Patient/10014, so it is correctly handled as a chained parameter, 
        /// including the filtering on the type in the modifier (if any).
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        private List<Criterium> NormalizeNonChainedReferenceCriteria(List<Criterium> criteria, string resourceType)
        {
            var result = new List<Criterium>();

            foreach (var crit in criteria)
            {
                var critSp = crit.FindSearchParamDefinition(resourceType);
                //                var critSp_ = _fhirModel.FindSearchParameter(resourceType, crit.ParamName); HIER VERDER: kunnen meerdere searchParameters zijn, hoewel dat alleen bij subcriteria van chains het geval is...
                if (critSp != null && critSp.Type == SearchParamType.Reference && crit.Operator != Operator.CHAIN && crit.Modifier != ModifierNames.MISSING && crit.Operand != null)
                {
                    var subCrit = new Criterium();
                    subCrit.Operator = crit.Operator;
                    string modifier = crit.Modifier;

                    //operand can be one of three things:
                    //1. just the id: 10014 (in the index as internal_justid), with no modifier
                    //2. just the id, but with a modifier that contains the type: Patient:10014
                    //3. full id: [http://localhost:xyz/fhir/]Patient/10014 (in the index as internal_id):
                    //  - might start with a host: http://localhost:xyz/fhir/Patient/100014
                    //  - the type in the modifier (if present) is no longer relevant
                    //And above that, you might have multiple identifiers with an IN operator. So we have to cater for that as well.
                    //Because we cannot express an OR construct in Criterium, we have choose one situation for all identifiers. We inspect the first, to determine which situation is appropriate.

                    //step 1: get the operand value, or - in the case of a Choice - the first operand value.
                    string operand = null;
                    if (crit.Operand is ChoiceValue)
                    {
                        ChoiceValue choiceOperand = (crit.Operand as ChoiceValue);
                        if (!choiceOperand.Choices.Any())
                        {
                            continue; //Choice operator without choices: ignore it.
                        }
                        else
                        {
                            operand = (choiceOperand.Choices.First() as UntypedValue).Value;
                        }
                    }
                    else
                    {
                        operand = (crit.Operand as UntypedValue).Value;
                    }

                    //step 2: determine which situation is accurate
                    int situation = 3;
                    if (!operand.Contains("/")) //Situation 1 or 2
                    {
                        if (String.IsNullOrWhiteSpace(modifier)) // no modifier, so no info about the referenced type at all
                        {
                            situation = 1;
                        }
                        else //modifier contains the referenced type
                        {
                            situation = 2;
                        }
                    }

                    //step 3: create a subcriterium appropriate for every situation. 
                    switch (situation)
                    {
                        case 1:
                            subCrit.ParamName = IndexFieldNames.JUSTID;
                            subCrit.Operand = crit.Operand;
                            break;
                        case 2:
                            subCrit.ParamName = IndexFieldNames.ID;
                            if (crit.Operand is ChoiceValue)
                            {
                                subCrit.Operand = new ChoiceValue(
                                    (crit.Operand as ChoiceValue).Choices.Select(choice =>
                                        new UntypedValue(modifier + "/" + (choice as UntypedValue).Value))
                                        .ToList());
                            }
                            else
                            {
                                subCrit.Operand = new UntypedValue(modifier + "/" + operand);
                            }
                            break;
                        default: //remove the base of the url if there is one and it matches this server
                            subCrit.ParamName = IndexFieldNames.ID;
                            if (crit.Operand is ChoiceValue)
                            {
                                subCrit.Operand = new ChoiceValue(
                                    (crit.Operand as ChoiceValue).Choices.Select(choice =>
                                    {
                                        Uri uriOperand;
                                        Uri.TryCreate((choice as UntypedValue).Value, UriKind.RelativeOrAbsolute, out uriOperand);
                                        var refUri = Localhost.RemoveBase(uriOperand); //Drop the first part if it points to our own server.
                                        return new UntypedValue(refUri.ToString().TrimStart(new char[] { '/' }));
                                    }));
                            }
                            else
                            {
                                Uri uriOperand;
                                Uri.TryCreate(operand, UriKind.RelativeOrAbsolute, out uriOperand);
                                var refUri = Localhost.RemoveBase(uriOperand); //Drop the first part if it points to our own server.
                                subCrit.Operand = new UntypedValue(refUri.ToString().TrimStart(new char[] { '/' }));
                            }
                            break;
                    }

                    var superCrit = new Criterium();
                    superCrit.ParamName = crit.ParamName;
                    superCrit.Modifier = crit.Modifier;
                    superCrit.Operator = Operator.CHAIN;
                    superCrit.Operand = subCrit;
                    superCrit.SearchParameters.AddRange(crit.SearchParameters);

                    result.Add(superCrit);
                }
                else result.Add(crit);
            }

            return result;
        }

        protected virtual Dictionary<Criterium, Criterium> CloseChainedCriteria(string resourceType, IEnumerable<Criterium> criteria, SearchResults results, int level)
        {
            //Mapping of original criterium and closed criterium, the former to be able to exclude it if it errors later on.
            var closedCriteria = new Dictionary<Criterium, Criterium>();
            foreach (var c in criteria)
            {
                if (c.Operator == Operator.CHAIN)
                {
                    try
                    {
                        closedCriteria.Add(c.Clone(), CloseCriterium(c, resourceType, level));
                        //CK: We don't pass the SearchResults on to the (recursive) CloseCriterium. We catch any exceptions only on the highest level.
                    }
                    catch (ArgumentException ex)
                    {
                        if (results == null) throw; //The exception *will* be caught on the highest level.
                        results.AddIssue(String.Format("Parameter [{0}] was ignored for the reason: {1}.", c.ToString(), ex.Message), OperationOutcome.IssueSeverity.Warning);
                        results.UsedCriteria.Remove(c);
                    }
                }
                else
                {
                    //If it is not a chained criterium, we don't need to 'close' it, so it is said to be 'closed' already.
                    closedCriteria.Add(c, c);
                }
            }

            return closedCriteria;
        }

        /// <summary>
        /// CloseCriterium("patient.name=\"Teun\"") -> "patient IN (id1,id2)"
        /// </summary>
        /// <param name="resourceType"></param>
        /// <param name="crit"></param>
        /// <returns></returns>
        protected virtual Criterium CloseCriterium(Criterium crit, string resourceType, int level)
        {

            List<string> targeted = crit.GetTargetedReferenceTypes(resourceType);
            List<string> allKeys = new List<string>();
            var errors = new List<Exception>();
            foreach (var target in targeted)
            {
                try
                {
                    Criterium innerCriterium = (Criterium)crit.Operand;
                    var keys = OnCollectKeys(target, new List<Criterium> { innerCriterium }, ++level);               //Recursive call to CollectKeys!
                    allKeys.AddRange(keys);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
            if (errors.Count == targeted.Count())
            {
                //It is possible that some of the targets don't support the current parameter. But if none do, there is a serious problem.
                throw new ArgumentException(String.Format("None of the possible target resources support querying for parameter {0}", crit.ParamName));
            }
            crit.Operator = Operator.IN;
            crit.Operand = new ChoiceValue(allKeys.Select(k => new UntypedValue(k)));
            return crit;
        }

        protected abstract List<string> OnCollectKeys(string resourceType, IEnumerable<Criterium> criteria, int level = 0);
    }
}
