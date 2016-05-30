using System.Linq;
using AFT.RegoV2.MemberApi.Interface;
using FluentValidation;

namespace AFT.RegoV2.MemberApi.Filters
{
    public static class ValidationErrorExtension
    {
        public static MemberApiException ToMemberApiException(this ServiceStack.Validation.ValidationError errorException)
        {
            return new MemberApiException
            {
                ErrorMessage = errorException.ErrorMessage,
                ErrorCode = errorException.ErrorCode,
                StackTrace = errorException.StackTrace,
                Violations = (from v in errorException.Violations
                              select new ValidationErrorField
                              {
                                  ErrorCode = v.ErrorCode,
                                  ErrorMessage = v.ErrorMessage,
                                  FieldName = v.FieldName
                              }).ToList()
            };
        }

        public static MemberApiException ToMemberApiException(this ValidationException errorException)
        {
            return new MemberApiException
            {
                ErrorMessage = errorException.Message,
                ErrorCode = errorException.Message,
                StackTrace = errorException.StackTrace,
                Violations = (from v in errorException.Errors
                              select new ValidationErrorField
                              {
                                  ErrorCode = v.ErrorCode,
                                  ErrorMessage = v.ErrorMessage,
                                  FieldName = v.PropertyName
                              }).ToList()
            };
        }
    }
}