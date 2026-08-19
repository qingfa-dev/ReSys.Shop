import type { Router } from 'vue-router'
import { useAuthStore } from '@/features/identity/stores/authStore'

let isInitialized = false

export function setupGuards(router: Router): void {
  router.beforeEach(async (to) => {
    const store = useAuthStore()

    if (!isInitialized) {
      await store.init()
      isInitialized = true
    }

    if (to.meta.guestOnly && store.isAuthenticated) {
      return { path: '/' }
    }

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
