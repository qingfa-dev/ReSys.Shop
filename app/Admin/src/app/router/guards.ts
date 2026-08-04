import type { Router } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/authStore'

let isInitialized = false

export function setupGuards(router: Router): void {
  router.beforeEach(async (to) => {
    const store = useAuthStore()

    if (!isInitialized) {
      await store.init()
      isInitialized = true
    }

    // Redirect: Send authenticated users away from login and auth-only pages
    if (to.meta.guestOnly && store.isAuthenticated) {
      return { name: 'dashboard' }
    }

    // Redirect: Require authentication before entering protected routes
    if (to.meta.requiresAuth && !store.isAuthenticated) {
      return { name: 'login', query: { redirect: to.fullPath } }
    }
  })

  router.afterEach((to) => {
    if (to.meta.title) {
      document.title = `${to.meta.title} | ReSys.Shop`
    }
  })
}
