import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User, LoginRequest, RegisterRequest, UpdateProfileRequest } from '../types'
import { userService } from '../services/user/user.service'
import { authService } from '../services/auth/auth.service'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const initialized = ref(false)

  const isAuthenticated = computed(() => !!user.value)
  const isAdmin = computed(() => user.value?.role === 'admin')
  const fullName = computed(() => user.value ? `${user.value.firstName} ${user.value.lastName}` : '')

  async function initialize() {
    if (initialized.value) return
    const token = localStorage.getItem('accessToken')
    if (token) {
      try {
        const storedUserId = localStorage.getItem('userId') || 'user-1'
        const result = await userService.getProfile(storedUserId)
        if (result.isSuccess && result.data) {
          user.value = result.data
        }
      } catch {
        localStorage.removeItem('accessToken')
      }
    }
    initialized.value = true
  }

  async function login(credentials: LoginRequest) {
    loading.value = true
    error.value = null
    try {
      const result = await authService.login(credentials)
      if (result.isSuccess && result.data) {
        user.value = result.data.user
        localStorage.setItem('accessToken', result.data.tokens.accessToken)
        localStorage.setItem('userId', result.data.user.id)
      } else {
        throw new Error(result.message || 'Login failed')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Login failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function register(info: RegisterRequest) {
    loading.value = true
    error.value = null
    try {
      const result = await authService.register(info)
      if (result.isSuccess && result.data) {
        user.value = result.data.user
        localStorage.setItem('accessToken', result.data.tokens.accessToken)
      } else {
        throw new Error(result.message || 'Registration failed')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Registration failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function logout() {
    try {
      await authService.logout()
    } finally {
      user.value = null
      localStorage.removeItem('accessToken')
    }
  }

  async function fetchProfile() {
    if (!localStorage.getItem('accessToken')) return
    const storedUserId = localStorage.getItem('userId') || 'user-1'
    try {
      const result = await userService.getProfile(storedUserId)
      if (result.isSuccess && result.data) {
        user.value = result.data
      } else {
        user.value = null
      }
    } catch {
      user.value = null
    }
  }

  async function updateProfile(userId: string, updates: UpdateProfileRequest) {
    loading.value = true
    error.value = null
    try {
      const result = await userService.updateProfile(userId, updates)
      if (result.isSuccess && result.data) {
        user.value = result.data
      } else {
        throw new Error(result.message || 'Update failed')
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Update failed'
      throw e
    } finally {
      loading.value = false
    }
  }

  return {
    user,
    loading,
    error,
    initialized,
    isAuthenticated,
    isAdmin,
    fullName,
    initialize,
    login,
    register,
    logout,
    fetchProfile,
    updateProfile,
  }
})
