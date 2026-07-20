import type { RouteRecordRaw } from 'vue-router'

export const addressesRoutes: RouteRecordRaw = {
  path: 'addresses',
  name: 'addresses',
  component: () => import('./pages/AddressListPage.vue'),
  meta: { breadcrumb: 'Addresses' },
}
