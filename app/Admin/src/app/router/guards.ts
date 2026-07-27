import type { Router } from 'vue-router'
import { STORAGE_KEYS } from '@/shared/constants/storage'

function getAccessToken(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
  } catch {
    return null
  }
}

export function setupGuards(router: Router): void {
  router.beforeEach((to, _from, next) => {
    if (to.meta.requiresAuth && !getAccessToken()) {
      return next({ name: 'login', query: { redirect: to.fullPath } })
    }
    next()
  })

  router.afterEach((to) => {
    if (to.meta.title) {
      document.title = `${to.meta.title} | ReSys.Shop`
    }
  })
}
