import { languages } from '../../i18n/translator'
import { useTranslation } from '../../i18n/useTranslation'
import type { SessionViewModel } from '../../viewmodels/useSessionViewModel'
import { useThemeViewModel } from '../../viewmodels/useThemeViewModel'
import type { Segment } from '../atoms/SegmentedControl'
import { AppHeader } from '../organisms/AppHeader'

const languageOptions = languages.map((value) => ({ value, label: value.toUpperCase() }))

/** The header every page shares: language, theme and session; the predictor adds the version. */
export function SiteHeader<T extends string>({ session, versions }: { session: SessionViewModel; versions?: { options: Segment<T>[]; value: T; onChange: (version: T) => void } }) {
  const { language, setLanguage } = useTranslation()
  const theme = useThemeViewModel()

  return (
    <AppHeader
      versions={versions}
      languages={languageOptions}
      language={language}
      onLanguageChange={setLanguage}
      dark={theme.dark}
      onToggleTheme={theme.toggleTheme}
      session={session}
    />
  )
}
