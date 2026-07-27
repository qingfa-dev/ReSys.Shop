import type { RouteRecordRaw } from 'vue-router'

const ROUTE = {
  COUNTRIES: { LIST: 'location.countries.list', CREATE: 'location.countries.create', VIEW: 'location.countries.view', EDIT: 'location.countries.edit' },
  STATES: { LIST: 'location.states.list', CREATE: 'location.states.create', VIEW: 'location.states.view', EDIT: 'location.states.edit' },
} as const

export { ROUTE }

export const locationRoutes: RouteRecordRaw = {
  path: 'locations',
  children: [
    { path: '', redirect: { name: ROUTE.COUNTRIES.LIST } },
    {
      path: 'countries',
      name: ROUTE.COUNTRIES.LIST,
      component: () => import('@/features/location/pages/CountryListPage.vue'),
    },
    {
      path: 'countries/new',
      name: ROUTE.COUNTRIES.CREATE,
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'countries/:id',
      name: ROUTE.COUNTRIES.VIEW,
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'countries/:id/edit',
      name: ROUTE.COUNTRIES.EDIT,
      component: () => import('@/features/location/pages/CountryDetailPage.vue'),
    },
    {
      path: 'states',
      name: ROUTE.STATES.LIST,
      component: () => import('@/features/location/pages/StateListPage.vue'),
    },
    {
      path: 'states/new',
      name: ROUTE.STATES.CREATE,
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
    {
      path: 'states/:id',
      name: ROUTE.STATES.VIEW,
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
    {
      path: 'states/:id/edit',
      name: ROUTE.STATES.EDIT,
      component: () => import('@/features/location/pages/StateDetailPage.vue'),
    },
  ],
}
