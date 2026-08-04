import type { RouteRecordRaw } from 'vue-router'

const CountriesList = () => import('../views/CountriesList.vue')
const CountryDetail = () => import('../views/CountryDetail.vue')
const StatesList = () => import('../views/StatesList.vue')
const StateDetail = () => import('../views/StateDetail.vue')

export const locationRoutes: RouteRecordRaw[] = [
  {
    path: 'location',
    redirect: { name: 'location-countries' },
  },
  {
    path: 'location/countries',
    name: 'location-countries',
    component: CountriesList,
    meta: { title: 'Countries' },
  },
  {
    path: 'location/countries/:id',
    name: 'location-country-detail',
    component: CountryDetail,
    meta: { title: 'Country Detail' },
  },
  {
    path: 'location/states',
    name: 'location-states',
    component: StatesList,
    meta: { title: 'States' },
  },
  {
    path: 'location/states/:id',
    name: 'location-state-detail',
    component: StateDetail,
    meta: { title: 'State Detail' },
  },
]

export const locationMenuItems = [
  {
    label: 'Location',
    icon: 'pi pi-fw pi-map',
    items: [
      { label: 'Countries', icon: 'pi pi-fw pi-globe', route: '/location/countries' },
      { label: 'States', icon: 'pi pi-fw pi-flag', route: '/location/states' },
    ],
  },
]
