using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    // Stripped down Buckaroo webhook event which should
    // hopefully work regardless of webhook API version.
    // We are essentially grabbing the most basic info
    // and then we use the API to fetch the entity in 
    // question so that it is fetched using the payment
    // providers API version.
    public class BuckarooWebhookEvent
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public BuckarooWebhookEventData Data { get; set; }
    }

    public class BuckarooWebhookEventData
    {
        [JsonProperty("object")]
        public BuckarooWebhookEventObject Object { get; set; }
    }

    public class BuckarooWebhookEventObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Type { get; set; }

        [JsonIgnore]
        public object Instance { get; set; }
    }
}
