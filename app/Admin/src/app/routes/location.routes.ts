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
      path: 'countries/new',
      name: 'location.countries.create',
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'countries/:id',
      name: 'location.countries.view',
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'countries/:id/edit',
      name: 'location.countries.edit',
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'states',
      name: 'location.states.list',
      component: () => import('@/features/location/pages/StateListPage.vue'),
    },
    {
      path: 'states/new',
      name: 'location.states.create',
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
    {
      path: 'states/:id',
      name: 'location.states.view',
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
    {
      path: 'states/:id/edit',
      name: 'location.states.edit',
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
  ],
}
