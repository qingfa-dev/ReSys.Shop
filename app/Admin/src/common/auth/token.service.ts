const ACCESS_KEY = 'accessToken'
const REFRESH_KEY = 'refreshToken'

export const tokenService = {
  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_KEY)
  },

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY)
  },

  setTokens(access: string, refresh: string): void {
    localStorage.setItem(ACCESS_KEY, access)
    localStorage.setItem(REFRESH_KEY, refresh)
  },

  clearTokens(): void {
    localStorage.removeItem(ACCESS_KEY)
    localStorage.removeItem(REFRESH_KEY)
  },

  hasTokens(): boolean {
    return localStorage.getItem(ACCESS_KEY) !== null && localStorage.getItem(REFRESH_KEY) !== null
  },
}
