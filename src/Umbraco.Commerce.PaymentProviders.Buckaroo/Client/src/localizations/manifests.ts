import type { ManifestLocalization } from '@umbraco-cms/backoffice/extension-registry';

const localizationManifests: Array<ManifestLocalization> = [
    {
        type: 'localization',
        alias: 'UmbracoCommerce.PaymentProviders.Buckaroo.Localization.En_US',
        weight: -100,
        name: 'English (US)',
        meta: {
            culture: 'en-us',
        },
        js: () => import('./en-us.ts'),
    },
];
export const manifests = [...localizationManifests];