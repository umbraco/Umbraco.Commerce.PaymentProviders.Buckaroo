using System;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions
{
    public class BuckarooInvalidSettingsException : Exception
    {
        private const string DefaultMessage = "Invalid payment provider settings. Please make sure that website key and secret key are set correctly in Umbraco backoffice.";


        public BuckarooInvalidSettingsException() : base(DefaultMessage)
        {
        }

        public BuckarooInvalidSettingsException(string message = DefaultMessage) : base(message)
        {
        }

        public BuckarooInvalidSettingsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
