import { useTranslation } from '../../i18n/useTranslation'
import { routeHref } from '../../routing'
import type { AuthFormViewModel } from '../../viewmodels/useAuthFormViewModel'
import { Button } from '../atoms/Button'
import { Hint } from '../atoms/Hint'
import { TextField } from '../molecules/TextField'

export function AuthForm({ form }: { form: AuthFormViewModel }) {
  const { t } = useTranslation()
  const register = form.mode === 'register'

  return (
    <form
      noValidate
      className="rounded-[10px] border border-line bg-panel p-[18px]"
      onSubmit={(event) => {
        event.preventDefault()
        void form.submit()
      }}
    >
      <h2 className="font-mono text-[11px] font-medium uppercase tracking-[0.2em] text-muted">{t(register ? 'auth.registerHeading' : 'auth.loginHeading')}</h2>
      <p className="mt-2 text-sm text-muted">{t('auth.optional')}</p>

      <TextField label={t('auth.email')} type="email" autoComplete="email" value={form.email} onChange={form.setEmail} errors={form.fieldErrors.email} />
      <TextField
        label={t('auth.password')}
        type="password"
        autoComplete={register ? 'new-password' : 'current-password'}
        value={form.password}
        onChange={form.setPassword}
        hint={register ? t('auth.passwordHint') : undefined}
        errors={form.fieldErrors.password}
      />

      <Button type="submit" className="mt-5" disabled={form.loading}>
        {form.loading ? t('auth.submitting') : t(register ? 'auth.submitRegister' : 'auth.submitLogin')}
      </Button>
      {form.error && <Hint error>{form.error}</Hint>}

      <p className="mt-4 text-sm">
        <a className="font-semibold text-accent underline-offset-2 hover:underline" href={routeHref(register ? 'login' : 'register')}>
          {t(register ? 'auth.haveAccount' : 'auth.noAccount')}
        </a>
      </p>
    </form>
  )
}
