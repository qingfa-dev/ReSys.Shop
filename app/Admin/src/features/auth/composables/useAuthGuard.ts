import type { NavigationGuardWithThis, RouteLocationNormalized } from 'vue-router'
import { RouteName } from '@/shared/config/routes'
import { useAuthState } from './useAuthState'

export function useAuthGuard(_router: unknown): NavigationGuardWithThis<undefined> {
  const { isAuthenticated } = useAuthState()
  return function (to: RouteLocationNormalized, _from, next) {
    const requiresAuth = to.meta.authRequired === true
    if (requiresAuth && !isAuthenticated.value) {
      return next({ name: RouteName.Login, query: { redirect: to.fullPath } })
    }
    return next()
  }
}
