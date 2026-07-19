import type { RouteRecordRaw } from 'vue-router'

export const addressesRoutes: RouteRecordRaw = {
  path: 'addresses',
  name: 'addresses',
  component: () => import('./views/AddressList.View.vue'),
  meta: { breadcrumb: 'Addresses' },
}
