import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';
import { manifests as localizationManifests } from './localizations/manifests'
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const manifests: Array<ManifestTypes> = [
    ...localizationManifests,
]

export const onInit: UmbEntryPointOnInit = (host, extensionRegistry) => {
    console.log('%c Buckaroo plugin loaded ＼（〇_ｏ）／', 'font-size: 20pt;')
    extensionRegistry.registerMany(manifests);
}