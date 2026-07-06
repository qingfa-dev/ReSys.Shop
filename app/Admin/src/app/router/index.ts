import { createRouter, createWebHistory } from 'vue-router'
import { routes } from './routes'
import { useAuthGuard } from '@/features/auth/composables/useAuthGuard'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

const guard = useAuthGuard()
router.beforeEach(guard)

export default router
