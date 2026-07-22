import type { Router } from 'vue-router'
import { TokenService } from '@/shared/auth/token.service'

const PUBLIC_ROUTES = ['auth.login', 'auth.register', 'auth.forgotPassword', 'auth.resetPassword']

export function registerAuthGuard(router: Router) {
  router.beforeEach((to, _from, next) => {
    const isAuthenticated = TokenService.hasValidAccessToken()

    if (!isAuthenticated && !PUBLIC_ROUTES.includes(to.name as string)) {
      next({ name: 'auth.login', query: { redirect: to.fullPath } })
      return
    }

    if (isAuthenticated && to.name === 'auth.login') {
      next({ name: 'reports.dashboard' })
      return
    }

    next()
  })
}
