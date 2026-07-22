import { reactive } from 'vue'

interface CurrentUser {
  id: string
  email: string
  name: string
  role: string
  permissions: string[]
}

interface SessionState {
  user: CurrentUser | null
  isAuthenticated: boolean
  isLoading: boolean
}

export const sessionState = reactive<SessionState>({
  user: null,
  isAuthenticated: false,
  isLoading: true,
})

export function setSessionUser(user: CurrentUser): void {
  sessionState.user = user
  sessionState.isAuthenticated = true
  sessionState.isLoading = false
}

export function clearSession(): void {
  sessionState.user = null
  sessionState.isAuthenticated = false
  sessionState.isLoading = false
}
