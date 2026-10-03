import { computed } from 'vue'
import { useAuthStore } from '../store/auth.store'
import type { RegisterRequest, ResetPasswordRequest, ChangePasswordRequest } from '../types'

export function useAuth() {
  const store = useAuthStore()

  return {
    isLoading: computed(() => store.isLoading),
    isAuthenticated: computed(() => store.isAuthenticated),
    serverErrors: computed(() => store.serverErrors),
    fieldErrors: computed(() => store.fieldErrors),
    currentUser: computed(() => store.currentUser),

    login: (credential: string, password: string) => store.login(credential, password),
    register: (fields: RegisterRequest) => store.register(fields),
    forgotPassword: (email: string) => store.forgotPassword(email),
    resetPassword: (params: ResetPasswordRequest) => store.resetPassword(params),
    changePassword: (params: ChangePasswordRequest) => store.changePassword(params),
    logout: () => store.logout(),

    initialize: () => store.initialize(),
  }
}
