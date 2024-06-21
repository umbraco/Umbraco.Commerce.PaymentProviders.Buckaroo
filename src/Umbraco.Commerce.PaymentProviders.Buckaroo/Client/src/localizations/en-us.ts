import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export default {
    paymentProviders: {
        'buckaroo-onetime-paymentLabel': 'Buckaroo One Time Payment',
        'buckaroo-onetime-paymentDescription': 'Buckaroo payment provider for one time checkout',
        'buckaroo-onetime-paymentSettingsContinueUrlLabel': 'Continue URL',
        'buckaroo-onetime-paymentSettingsContinueUrlDescription': 'The URL to continue to after this provider has done processing. eg: /continue/',
        'buckaroo-onetime-paymentSettingsCancelUrlLabel': 'Cancel URL',
        'buckaroo-onetime-paymentSettingsCancelUrlDescription': 'The URL to return to if the payment attempt is canceled. eg: /cart/',
        'buckaroo-onetime-paymentSettingsErrorUrlLabel': 'Error URL',
        'buckaroo-onetime-paymentSettingsErrorUrlDescription': 'The URL to return to if the payment attempt errors. eg: /error/',
        'buckaroo-onetime-paymentSettingsWebsiteKeyLabel': 'Website key',
        'buckaroo-onetime-paymentSettingsWebsiteKeyDescription': 'The website key, which can be found here: https://plaza.buckaroo.nl/Configuration/WebSite/Index/',
        'buckaroo-onetime-paymentSettingsApiKeyLabel': 'Secret key',
        'buckaroo-onetime-paymentSettingsApiKeyDescription': 'The secret key, which can be found here: https://plaza.buckaroo.nl/Configuration/Merchant/SecretKey',
        'buckaroo-onetime-paymentSettingsWebhookHostnameOverwriteLabel': 'Webhook hostname overwrite',
        'buckaroo-onetime-paymentSettingsWebhookHostnameOverwriteDescription': 'If you rewrite incoming host headers to a different value, set this to the hostname where the buyer does the checkout action. Enter hostname only eg: \'umbraco.com\'',
        'buckaroo-onetime-paymentSettingsIsTestModeLabel': 'Enable test mode',
        'buckaroo-onetime-paymentSettingsIsTestModeDescription': 'Set whether to process payments in test mode',
    },
} as UmbLocalizationDictionary;
