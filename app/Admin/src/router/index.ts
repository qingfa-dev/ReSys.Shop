import { createRouter, createWebHistory } from 'vue-router'
import MainLayout from '@/app/layout/MainLayout.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: MainLayout,
      children: [],
    },
  ],
})

export default router
