using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Serialization;

namespace AFT.RegoV2.AdminApi.Attributes
{
    public class DisableCamelCaseSerializationAttribute : Attribute, IControllerConfiguration
    {
        public void Initialize(HttpControllerSettings httpControllerSettings, HttpControllerDescriptor httpControllerDescriptor)
        {
            var jsonFormatter = httpControllerSettings.Formatters.OfType<JsonMediaTypeFormatter>().First();

            jsonFormatter.SerializerSettings.ContractResolver = new DefaultContractResolver();

            httpControllerSettings.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));
        }
    }
}