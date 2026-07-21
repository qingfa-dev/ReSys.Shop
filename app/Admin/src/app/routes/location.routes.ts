import type { RouteRecordRaw } from 'vue-router'

export const locationRoutes: RouteRecordRaw = {
  path: 'locations',
  children: [
    { path: '', redirect: { name: 'location.countries.list' } },
    {
      path: 'countries',
      name: 'location.countries.list',
      component: () => import('@/features/location/pages/CountryListPage.vue'),
    },
    {
      path: 'states',
      name: 'location.states.list',
      component: () => import('@/features/location/pages/StateListPage.vue'),
    },
  ],
}
