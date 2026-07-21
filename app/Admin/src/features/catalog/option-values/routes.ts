import type { RouteRecordRaw } from 'vue-router'

export const optionValueRoutes: RouteRecordRaw[] = [
  {
    path: 'option-values',
    name: 'catalog.option-values.list',
    component: () => import('./pages/OptionValueListPage.vue'),
  },
]
