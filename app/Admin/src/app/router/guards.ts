import type { Router } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/authStore'

let isInitialized = false

export function setupGuards(router: Router): void {
  router.beforeEach(async (_to, _from) => {
    const store = useAuthStore()

    if (!isInitialized) {
      await store.init()
      isInitialized = true
    }

    // TODO: re-enable auth guard after route scaffold review
    // if (to.meta.requiresAuth && !store.isAuthenticated) {
    //   return { name: 'login', query: { redirect: to.fullPath } }
    // }
  })

  router.afterEach((to) => {
    if (to.meta.title) {
      document.title = `${to.meta.title} | ReSys.Shop`
    }
  })
}
