import type { RouteRecordRaw } from 'vue-router'

export const catalogRoutes: RouteRecordRaw = {
  path: 'catalog',
  children: [
    { path: '', redirect: { name: 'catalog.dashboard' } },
    {
      path: 'dashboard',
      name: 'catalog.dashboard',
      component: () => import('@/features/catalog/pages/DashboardPage.vue'),
    },
    {
      path: 'products',
      name: 'catalog.products.list',
      component: () => import('@/features/catalog/pages/ProductListPage.vue'),
    },
    {
      path: 'products/create',
      name: 'catalog.products.create',
      component: () => import('@/features/catalog/pages/ProductCreatePage.vue'),
    },
    {
      path: 'taxa',
      name: 'catalog.taxa.list',
      component: () => import('@/features/catalog/pages/TaxonListPage.vue'),
    },
    {
      path: 'taxonomies',
      name: 'catalog.taxonomies.list',
      component: () => import('@/features/catalog/pages/TaxonTreeManagerPage.vue'),
    },
    {
      path: 'option-types',
      name: 'catalog.option-types.list',
      component: () => import('@/features/catalog/pages/OptionTypeListPage.vue'),
    },
    {
      path: 'option-values',
      name: 'catalog.option-values.list',
      component: () => import('@/features/catalog/pages/OptionValueListPage.vue'),
    },
  ],
}
