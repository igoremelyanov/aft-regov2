using System;

namespace AFT.RegoV2.Core.Common.Attributes
{
    public abstract class ArgumentValidationAttribute : Attribute
    {
        public abstract void Validate(object value, string argumentName);
    }
}
