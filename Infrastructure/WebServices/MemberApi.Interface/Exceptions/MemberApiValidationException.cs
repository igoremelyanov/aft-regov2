using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using AFT.RegoV2.Shared;
using Newtonsoft.Json;

namespace AFT.RegoV2.MemberApi.Interface.Exceptions
{
    public class MemberApiValidationException : MemberApiException
    {
        public IList<RegoValidationError> ValidationErrors { get; private set; }

        public MemberApiValidationException(string message) : base(message)
        {
        }

        public MemberApiValidationException(IEnumerable<RegoValidationError> validationErrors)
            : base(validationErrors.Any() ? validationErrors.First().ErrorMessage : "Unknown error")
        {
            ValidationErrors = validationErrors.ToList();
            StatusCode = HttpStatusCode.BadRequest;
        }
    }

    public static class HttpErrorEx
    {
        private const string ValidationErrorsFieldName = "ValidationErrors";

        public static bool HasValidationErrors(this HttpError error)
        {
            return error.ContainsKey(ValidationErrorsFieldName);
        }

        public static IList<RegoValidationError> GetValidationErrors(this HttpError error)
        {
            var tmpError = error[ValidationErrorsFieldName];
            if (tmpError == null)
                return new List<RegoValidationError>() { new RegoValidationError() { ErrorMessage = error["Message"].ToString(), FieldName = "" } };
            return JsonConvert.DeserializeObject<List<RegoValidationError>>(tmpError.ToString());
        }

        public static void AddValidationErrors(this HttpError error, IEnumerable<RegoValidationError> errors)
        {
            if (error.HasValidationErrors())
            {
                var validationErrors = error.GetValidationErrors().ToList();
                validationErrors.AddRange(errors);
                error[ValidationErrorsFieldName] = validationErrors;
            }
            else
            {
                error[ValidationErrorsFieldName] = errors;
            }
        }
    }
}
