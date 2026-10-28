import js from '@eslint/js'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import globals from 'globals'
import tseslint from 'typescript-eslint'

export default tseslint.config(
  { ignores: ['dist'] },
  {
    files: ['**/*.{ts,tsx}'],
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    languageOptions: { globals: globals.browser },
    plugins: { 'react-hooks': reactHooks, 'react-refresh': reactRefresh },
    rules: {
      ...reactHooks.configs.recommended.rules,
      'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],
    },
  },
  {
    // MVVM: views never call the API or run view-model logic; they may only import view-model types.
    // Pages are the one place that wires view models to views.
    files: ['src/components/**/*.{ts,tsx}'],
    ignores: ['src/components/pages/**', 'src/components/**/*.test.tsx'],
    rules: {
      '@typescript-eslint/no-restricted-imports': ['error', {
        patterns: [
          { group: ['**/models/api'], message: 'Views do not call the API; use a view model.' },
          { group: ['**/viewmodels/**'], allowTypeImports: true, message: 'Views only import view-model types.' },
        ],
      }],
    },
  },
)
