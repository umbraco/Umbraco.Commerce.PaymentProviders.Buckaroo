// @ts-check

import eslint from '@eslint/js';
import tseslint from 'typescript-eslint';

export default tseslint.config(
    eslint.configs.recommended,
    ...tseslint.configs.recommended,
    {
        files: ['**/*.ts', '**/*.js'],
        rules: {
            'comma-dangle': ['error', 'always-multiline'],
            'quotes': ['error', 'single'],
            'object-property-newline': 'error',
        },
    },
);