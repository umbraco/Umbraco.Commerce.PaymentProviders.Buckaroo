using System;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo
{
    public abstract class BuckarooPaymentProviderBase<TSelf, TSettings> : PaymentProviderBase<TSettings>
        where TSelf : BuckarooPaymentProviderBase<TSelf, TSettings>
        where TSettings : BuckarooSettingsBase, new()
    {
        protected ILogger<TSelf> Logger { get; }

        protected BuckarooPaymentProviderBase(
            UmbracoCommerceContext context,
            ILogger<TSelf> logger)
            : base(context)
        {
            Logger = logger;
        }

        public override string GetCancelUrl(PaymentProviderContext<TSettings> context)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(context.Settings, "settings");
            ArgumentNullException.ThrowIfNull(context.Settings.CancelUrl, "settings.CancelUrl");

            return context.Settings.CancelUrl;
        }

        public override string GetContinueUrl(PaymentProviderContext<TSettings> context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Settings.ContinueUrl; // + (settings.ContinueUrl.Contains("?") ? "&" : "?") + "session_id={CHECKOUT_SESSION_ID}";
        }

        public override string GetErrorUrl(PaymentProviderContext<TSettings> context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Settings.ErrorUrl;
        }
    }
}
