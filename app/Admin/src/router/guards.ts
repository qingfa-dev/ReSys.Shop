import type { Router } from 'vue-router'
import { TokenService } from '@/shared/auth/token.service'

export function registerAuthGuard(router: Router) {
  router.beforeEach((to, _from, next) => {
    const isAuthenticated = TokenService.hasValidAccessToken()

    if (!isAuthenticated && to.name !== 'login') {
      // Login route not yet implemented — guard is scaffolding.
      // When login route exists, uncomment:
      // next({ name: 'login', query: { redirect: to.fullPath } })
      // return
    }

    next()
  })
}
