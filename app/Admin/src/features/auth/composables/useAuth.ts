import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '../store/auth.store'
import {
  createLoginSchema,
  createRegisterSchema,
  createForgotPasswordSchema,
  createResetPasswordSchema,
  createChangePasswordSchema,
} from '../models'
import type { RegisterRequest, ResetPasswordRequest, ChangePasswordRequest } from '../types'

export function useAuth() {
  const store = useAuthStore()
  const { t } = useI18n()

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

    loginSchema: createLoginSchema(t),
    registerSchema: createRegisterSchema(t),
    forgotPasswordSchema: createForgotPasswordSchema(t),
    resetPasswordSchema: createResetPasswordSchema(t),
    changePasswordSchema: createChangePasswordSchema(t),

    initialize: () => store.initialize(),
  }
}
