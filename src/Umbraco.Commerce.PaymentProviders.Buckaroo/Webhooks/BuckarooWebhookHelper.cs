using System;
using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Webhooks
{
    internal static class BuckarooWebhookHelper
    {
        public static BuckarooWebhookTransaction ParseDataFromBytes(byte[] data)
        {
            string jsonContent = System.Text.Encoding.UTF8.GetString(data);
            BuckarooWebhookTransaction buckarooEvent = JsonConvert.DeserializeObject<BuckarooWebhookResponse?>(jsonContent)?.Transaction ?? throw new NotImplementedException("Unable to parse buckaroo push message to object");
            return buckarooEvent;
        }
    }
}
