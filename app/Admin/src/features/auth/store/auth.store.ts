import { ref, readonly, type Ref } from 'vue'
import { defineStore } from 'pinia'
import { useRouter } from 'vue-router'
import { useSessionStore } from '@/stores/useSessionStore'
import { TokenService } from '@/shared/auth/token.service'
import type { ApiProblemDetail } from '@/shared/models'
import {
  loginApi,
  registerApi,
  forgotPasswordApi,
  resetPasswordApi,
  changePasswordApi,
  logoutApi,
  getSessionApi,
} from '../api/auth.api'
import type { RegisterRequest, ResetPasswordRequest, ChangePasswordRequest } from '../types'

function mapErrors(
  errors: ApiProblemDetail[],
  fieldErrors: Ref<Record<string, string[]>>,
  serverErrors: Ref<ApiProblemDetail[]>,
) {
  const fields: Record<string, string[]> = {}
  const server: ApiProblemDetail[] = []

  for (const error of errors) {
    server.push(error)
    const segments = error.code.split('.')
    const mapped = segments.length >= 2
      ? segments[1].charAt(0).toLowerCase() + segments[1].slice(1)
      : null

    if (mapped) {
      if (!fields[mapped]) fields[mapped] = []
      fields[mapped].push(error.message)
    }
  }

  fieldErrors.value = fields
  serverErrors.value = server
}

function fromJwtToUser(payload: Record<string, unknown>) {
  return {
    id: (payload.sub as string) ?? '',
    email: (payload.email as string) ?? '',
    name: (payload.name as string) ?? '',
    role: (payload.role as string) ?? '',
    permissions: (payload.permissions as string[]) ?? [],
  }
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
    const result = await loginApi(credential, password)
    if (result.isSuccess) {
      TokenService.setTokens(result.value.accessToken, result.value.refreshToken)
      const payload = TokenService.getAccessTokenPayload()
      if (payload) {
        session.setUser(fromJwtToUser(payload as unknown as Record<string, unknown>))
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
    const result = await registerApi(fields)
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
    const result = await forgotPasswordApi(email)
    if (!result.isSuccess) {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  async function resetPassword(params: ResetPasswordRequest) {
    resetFormState()
    isLoading.value = true
    const result = await resetPasswordApi(params)
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
    const result = await changePasswordApi(params)
    if (result.isSuccess) {
      router.push({ name: 'reports.dashboard' })
    } else {
      mapErrors(result.errors, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  async function logout() {
    resetFormState()
    await logoutApi()
    TokenService.clearTokens()
    session.clear()
    router.push({ name: 'auth.login' })
  }

  async function initialize() {
    isLoading.value = true
    if (TokenService.hasValidAccessToken()) {
      const result = await getSessionApi()
      if (result.isSuccess && result.value) {
        session.setUser({
          id: result.value.id,
          email: '',
          name: '',
          role: result.value.roles[0] ?? '',
          permissions: Array.isArray(result.value.permissions) ? result.value.permissions : [],
        })
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
    currentUser: session.user,
  }
})

export { mapErrors }
