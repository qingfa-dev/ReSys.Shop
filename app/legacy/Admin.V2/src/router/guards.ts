import type { Router } from 'vue-router'
import { TokenService } from '@/features/auth/services/token.service'

const PUBLIC_ROUTES = ['auth.login', 'auth.register', 'auth.forgotPassword', 'auth.resetPassword']

export function registerAuthGuard(router: Router) {
  router.beforeEach((to, _from) => {
    const isAuthenticated = TokenService.hasValidAccessToken()

    if (!isAuthenticated && !PUBLIC_ROUTES.includes(to.name as string)) {
      return { name: 'auth.login', query: { redirect: to.fullPath } }
    }

    if (isAuthenticated && to.name === 'auth.login') {
      return { name: 'reports.dashboard' }
    }
  })
}
