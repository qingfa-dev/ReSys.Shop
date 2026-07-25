import { ref, readonly, computed } from 'vue'
import { defineStore } from 'pinia'
import { useRouter } from 'vue-router'
import { useSessionStore } from '@/stores/useSessionStore'
import { TokenService } from '../services/token.service'
import type { ApiProblemDetail } from '@/shared/models'
import { AuthApi } from '../api/auth.api'
import type { RegisterRequest, ResetPasswordRequest, ChangePasswordRequest } from '../types'
import { AuthResponseMapper } from '../mappers/auth.response.mapper'

function fieldNameFromCode(code: string): string | null {
  const segments = code.split('.')
  if (segments.length < 2) return null
  const field = segments[1]
  if (!field) return null
  return field.charAt(0).toLowerCase() + field.slice(1)
}

function mapErrors(
  errors: ApiProblemDetail[],
  fieldErrors: { value: Record<string, string[]> },
  serverErrors: { value: ApiProblemDetail[] },
) {
  const fields: Record<string, string[]> = {}
  const server: ApiProblemDetail[] = []
  for (const error of errors) {
    server.push(error)
    const mapped = fieldNameFromCode(error.code)
    if (mapped) {
      if (!fields[mapped]) fields[mapped] = []
      fields[mapped].push(error.message)
    }
  }
  fieldErrors.value = fields
  serverErrors.value = server
}

export const useAuthStore = defineStore('auth', () => {
  const session = useSessionStore()
  const router = useRouter()

  const isLoading = ref(false)
  const serverErrors = ref<ApiProblemDetail[]>([])
  const fieldErrors = ref<Record<string, string[]>>({})

  function resetFormState() {
    isLoading.value = false
    serverErrors.value = []
    fieldErrors.value = {}
  }

  async function login(credential: string, password: string) {
    resetFormState()
    isLoading.value = true
    const result = await AuthApi.login(credential, password)
    if (result.isSuccess) {
      TokenService.setTokens(result.value.accessToken, result.value.refreshToken)
      const payload = TokenService.getAccessTokenPayload()
      if (payload) {
        session.setUser(AuthResponseMapper.fromJwt(payload as unknown as Record<string, unknown>))
      }
      const redirect = (router.currentRoute.value.query.redirect as string) ?? '/'
      router.push(redirect)
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  async function register(fields: RegisterRequest) {
    resetFormState()
    isLoading.value = true
    const result = await AuthApi.register(fields)
    if (result.isSuccess) {
      await login(fields.email, fields.password)
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  async function forgotPassword(email: string) {
    resetFormState()
    isLoading.value = true
    const result = await AuthApi.forgotPassword(email)
    if (!result.isSuccess) {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  async function resetPassword(params: ResetPasswordRequest) {
    resetFormState()
    isLoading.value = true
    const result = await AuthApi.resetPassword(params)
    if (result.isSuccess) {
      router.push({ name: 'auth.login' })
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  async function changePassword(params: ChangePasswordRequest) {
    resetFormState()
    isLoading.value = true
    const result = await AuthApi.changePassword(params)
    if (result.isSuccess) {
      router.push({ name: 'reports.dashboard' })
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  async function logout() {
    resetFormState()
    const refreshToken = TokenService.getRefreshToken()
    await AuthApi.logout(refreshToken ?? '')
    TokenService.clearTokens()
    session.clear()
    router.push({ name: 'auth.login' })
  }

  async function initialize() {
    isLoading.value = true
    if (TokenService.hasValidAccessToken()) {
      const result = await AuthApi.getSession()
      if (result.isSuccess && result.value) {
        session.setUser(AuthResponseMapper.fromSession(result.value))
      } else {
        TokenService.clearTokens()
        session.clear()
      }
    } else {
      TokenService.clearTokens()
      session.clear()
    }
    isLoading.value = false
  }

  return {
    isLoading: readonly(isLoading),
    serverErrors: readonly(serverErrors),
    fieldErrors: readonly(fieldErrors),
    login,
    register,
    forgotPassword,
    resetPassword,
    changePassword,
    logout,
    initialize,
    isAuthenticated: session.isAuthenticated,
    currentUser: readonly(session.user),
  }
})


