import { useState } from 'react'

import { ApiError, login, register, setToken } from '../models/api'
import { useTranslation } from '../i18n/useTranslation'

export type AuthMode = 'login' | 'register'

type FieldErrors = Record<string, string[]>

/**
 * Login and register forms. Registering logs the player in right away. `onDone` runs once the token is
 * stored (the page goes back to the predictor).
 */
export function useAuthFormViewModel(mode: AuthMode, onDone: () => void) {
  const { t } = useTranslation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})

  const submit = async () => {
    const missing: FieldErrors = {}
    if (!email.trim()) missing.email = ['errors.emailRequired']
    if (!password) missing.password = ['errors.passwordRequired']
    setFieldErrors(missing)
    setError(null)
    if (Object.keys(missing).length > 0) {
      return
    }

    setLoading(true)
    try {
      if (mode === 'register') {
        await register(email.trim(), password)
      }
      setToken((await login(email.trim(), password)).accessToken)
      onDone()
    } catch (reason) {
      if (reason instanceof ApiError) {
        setError(reason.message)
        setFieldErrors(reason.fieldErrors)
      } else {
        setError('errors.unexpected')
      }
    } finally {
      setLoading(false)
    }
  }

  // Keys follow a language switch; backend messages pass through as-is.
  const translate = (messages: string[] = []) => messages.map((message) => t(message))

  return {
    mode,
    email,
    password,
    loading,
    error: error && t(error),
    fieldErrors: { email: translate(fieldErrors.email), password: translate(fieldErrors.password) },
    setEmail,
    setPassword,
    submit,
  }
}

export type AuthFormViewModel = ReturnType<typeof useAuthFormViewModel>
