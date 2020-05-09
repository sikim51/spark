using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Hl7.Fhir.Model;
using Hl7.Fhir.Utility;
using Spark.Engine.Core;
using Spark.Engine.Search.Common;
using Spark.Engine.Search.Model;

namespace Spark.Engine.Search.Indexer
{
    public abstract class FhirIndexDocumentBuilder<T> where T : class, new()
    {
        public string RootId;
        protected T Document = new T();
       
        protected FhirIndexDocumentBuilder(IKey key)
        {
            this.RootId = key.TypeName + "/" + key.ResourceId;
        }

        public abstract T ToDocument();

        public string Cast(FhirString s)
        {
            if (s != null)
                return s.Value;
            else
                return null;
        }

        public string Cast(Resource resource)
        {
            return ModelInfo.GetFhirTypeNameForType(resource.GetType());
        }

        public string Cast(FhirDateTime dt)
        {
            if (dt != null)
                return dt.Value;
            else
                return null;
        }

        public string Cast(FhirUri uri)
        {
            if (uri != null)
                return uri.ToString();
            else
                return null;
        }

        public abstract void Write(String parameterName, FhirDateTime fhirDateTime);

        public void Write(Definition definition, FhirDateTime fhirDateTime)
        {
            Write(definition.ParamName, fhirDateTime);
        }

        public void Write(Definition definition, Code code)
        {
            if (code != null)
            {
                Write(definition, code.Value);
            }
        }

        public string Cast(ResourceReference reference)
        {
            if (reference == null) return null;
            if (reference.Url == null) return null;
            string uri = reference.Url.ToString();

            string[] s = uri.ToString().Split('#');
            if (s.Count() == 2)
            {
                string system = s[0];
                string code = s[1];
                if (string.IsNullOrEmpty(system))
                {
                    return this.RootId + "#" + code;
                }
            }
            return uri.ToString();
        }

        public abstract void Write(Definition definition, string value);

        public abstract void Write(string paramName, string value);

        public void Write(Definition definition, IEnumerable<string> items)
        {
            if (items != null)
            {
                foreach (string item in items)
                {
                    Write(definition, item);
                }
            }
        }

        public virtual void WriteMetaData(IKey key, int level, Resource resource)
        {
            if (level == 0)
            {
                Write(IndexFieldNames.ID, this.RootId);

                string selflink = key.ToUriString();
                Write(IndexFieldNames.SELFLINK, selflink);

                Write(IndexFieldNames.JUSTID, key.ResourceId);

                var fdt = resource.Meta.LastUpdated.HasValue ? new FhirDateTime(resource.Meta.LastUpdated.Value) : FhirDateTime.Now();
                Write(IndexFieldNames.LASTUPDATED, fdt);
            }
            else
            {
                string id = resource.Id;
                Write(IndexFieldNames.ID, $"{this.RootId}#{id}");
            }

            string category = resource.TypeName;
            Write(IndexFieldNames.RESOURCE, category);
            Write(IndexFieldNames.LEVEL, level);
        }
       
        public void Write(Definition definition, List<FhirString> list)
        {
            foreach (FhirString fs in list)
            {
                Write(definition, Cast(fs));
            }
        }

        public void Write(Definition definition, Enum item)
        {
            var coding = new Coding();
            coding.Code = item.GetLiteral();
            Write(definition, coding);
        }

        public virtual void Write(Definition definition, Quantity quantity)
        {
            switch (definition.ParamType)
            {
                case Hl7.Fhir.Model.SearchParamType.Quantity:
                    {
                        Write(definition.ParamName, quantity);
                        break;
                    }
                case Hl7.Fhir.Model.SearchParamType.Date:
                    {
                        break;
                    }
                default: return;
            }
        }
        public abstract void Write(string paramName, Quantity quantity);

        public abstract void Write(string paramName, int value);

        public abstract void Write(Definition definition, Coding coding);

        public abstract void Write(Definition definition, Identifier identifier);

        public void Write(Definition definition, ContactPoint contact)
        {
            Write(definition, Cast(contact.ValueElement));
        }

        public void Write(Definition definition, Address address)
        {
            Write(definition, address.City);
            Write(definition, address.Country);
            Write(definition, address.Line); // ienumerable
            Write(definition, address.State);
            Write(definition, address.Text);
            Write(definition, address.Use.ToString());
            Write(definition, address.PostalCode);
        }

        public void Write(Definition definition, HumanName name)
        {
            Write(definition, name.Given);
            Write(definition, name.Prefix);
            Write(definition, name.Family);
            Write(definition, name.Suffix);
            //Write(definition, name.Use.ToString());
        }

        public void Write(Definition definition, CodeableConcept concept)
        {
            Write($"{definition.ParamName}_text", concept.Text);
            if (concept.Coding != null)
            {
                foreach (Coding coding in concept.Coding)
                {
                    Write(definition, coding);
                }
            }
        }

        public abstract void Write(String parameterName, Period period);

        public void Write(Definition definition, Period period)
        {
            Write(definition.ParamName, period);
        }

        protected void LogNotImplemented(object item)
        {
            if (!(item is string || item is Uri || item.GetType().IsEnum))
            {
                Debug.WriteLine("Not implemented type: " + item.GetType().ToString());
            }
        }

        public void Write<C>(Definition definition, Code<C> code) where C : struct
        {
            InvokeWrite(definition, code.Value);
        }

        public void InvokeWrite(Definition definition, object item)
        {
            if (item != null)
            {
                Type type = item.GetType();
                MethodInfo m = this.GetType().GetMethod("Write", new Type[] { typeof(Definition), type });
                if (m != null)
                {
                    var result = m.Invoke(this, new object[] { definition, item });
                }
                else
                {
                    string result = null;
                    m = typeof(FhirIndexDocumentBuilder<T>).GetMethod("Cast", new Type[] { type });
                    if (m != null)
                    {
                        var cast = m.Invoke(this, new object[] { item });
                        result = (string)cast;
                    }
                    else
                    {
                        result = item.ToString();
                        LogNotImplemented(item);
                    }
                    Write(definition, result);
                }
            }
            else
            {
                Write(definition, (string)null);
            }
        }
    }
}