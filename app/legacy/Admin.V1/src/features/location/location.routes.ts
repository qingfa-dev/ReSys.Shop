import type { RouteRecordRaw } from 'vue-router'

export const locationRoutes: RouteRecordRaw = {
  path: 'locations',
  meta: { breadcrumb: 'Locations' },
  children: [
    {
      path: 'countries',
      name: 'location.countries.list',
      component: () => import('../location/countries/pages/CountryListPage.vue'),
      meta: { breadcrumb: 'Countries' },
    },
    {
      path: 'states',
      name: 'location.states.list',
      component: () => import('../location/states/pages/StateListPage.vue'),
      meta: { breadcrumb: 'States' },
    },
  ],
}
