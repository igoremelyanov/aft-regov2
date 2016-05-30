using System;
using System.Globalization;
namespace AFT.RegoV2.Core.Payment.Interface.Exceptions
{
    public class PaymentSettingsViolatedException : Exception
    {
        private readonly string _message;
        private readonly decimal _actualValue;

        public PaymentSettingsViolatedException(string message, decimal value)
        {
            _message = message;
            _actualValue = value;
        }

        public override string Message
        {
            get
            {
                return string.Format(
                    "{{\"text\": \"{0}\", \"variables\": {{\"value\": \"{1}\"}}}}",
                    _message,
                    _actualValue.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }
    }
}