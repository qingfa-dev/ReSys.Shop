import type { Router } from 'vue-router'

let isInitialized = false

export function setupGuards(router: Router): void {
  router.beforeEach(async (_to) => {
    // Auth guard — wired in Phase 2 after authStore exists
    if (!isInitialized) {
      isInitialized = true
    }
  })

  router.afterEach((to) => {
    if (to.meta.title) {
      document.title = `${to.meta.title} | ReSys.Shop`
    }
  })
}
