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
      path: 'products/new',
      name: 'catalog.products.create',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'products/:id',
      name: 'catalog.products.view',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'products/:id/edit',
      name: 'catalog.products.edit',
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
    },
    {
      path: 'taxonomies',
      name: 'catalog.taxonomies.list',
      component: () => import('@/features/catalog/pages/TaxonomyListPage.vue'),
    },
    {
      path: 'taxonomies/new',
      name: 'catalog.taxonomies.create',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'taxonomies/:id',
      name: 'catalog.taxonomies.view',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'taxonomies/:id/edit',
      name: 'catalog.taxonomies.edit',
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
    },
    {
      path: 'option-types',
      name: 'catalog.option-types.list',
      component: () => import('@/features/catalog/pages/OptionTypeListPage.vue'),
    },
    {
      path: 'option-types/new',
      name: 'catalog.option-types.create',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
    {
      path: 'option-types/:id',
      name: 'catalog.option-types.view',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
    {
      path: 'option-types/:id/edit',
      name: 'catalog.option-types.edit',
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
    },
  ],
}
