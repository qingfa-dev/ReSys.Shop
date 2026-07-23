import type { RouteRecordRaw } from 'vue-router'
import { ROUTE_CATALOG } from './route-names'

export const catalogRoutes: RouteRecordRaw = {
  path: 'catalog',
  children: [
    { path: '', redirect: { name: ROUTE_CATALOG.DASHBOARD } },
    {
      path: 'dashboard',
      name: ROUTE_CATALOG.DASHBOARD,
      component: () => import('@/features/catalog/pages/DashboardPage.vue'),
      meta: { icon: 'pi pi-fw pi-th-large' },
    },
    {
      path: 'products',
      name: ROUTE_CATALOG.PRODUCTS.LIST,
      component: () => import('@/features/catalog/pages/ProductListPage.vue'),
      meta: { icon: 'pi pi-fw pi-shopping-bag' },
    },
    {
      path: 'products/new',
      name: ROUTE_CATALOG.PRODUCTS.CREATE,
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-shopping-bag' },
    },
    {
      path: 'products/:id',
      name: ROUTE_CATALOG.PRODUCTS.VIEW,
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-shopping-bag' },
    },
    {
      path: 'products/:id/edit',
      name: ROUTE_CATALOG.PRODUCTS.EDIT,
      component: () => import('@/features/catalog/pages/ProductDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-shopping-bag' },
    },
    {
      path: 'taxonomies',
      name: ROUTE_CATALOG.TAXONOMIES.LIST,
      component: () => import('@/features/catalog/pages/TaxonomyListPage.vue'),
      meta: { icon: 'pi pi-fw pi-sitemap' },
    },
    {
      path: 'taxonomies/new',
      name: ROUTE_CATALOG.TAXONOMIES.CREATE,
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-sitemap' },
    },
    {
      path: 'taxonomies/:id',
      name: ROUTE_CATALOG.TAXONOMIES.VIEW,
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-sitemap' },
    },
    {
      path: 'taxonomies/:id/edit',
      name: ROUTE_CATALOG.TAXONOMIES.EDIT,
      component: () => import('@/features/catalog/pages/TaxonomyDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-sitemap' },
    },
    {
      path: 'option-types',
      name: ROUTE_CATALOG.OPTION_TYPES.LIST,
      component: () => import('@/features/catalog/pages/OptionTypeListPage.vue'),
      meta: { icon: 'pi pi-fw pi-list' },
    },
    {
      path: 'option-types/new',
      name: ROUTE_CATALOG.OPTION_TYPES.CREATE,
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-list' },
    },
    {
      path: 'option-types/:id',
      name: ROUTE_CATALOG.OPTION_TYPES.VIEW,
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-list' },
    },
    {
      path: 'option-types/:id/edit',
      name: ROUTE_CATALOG.OPTION_TYPES.EDIT,
      component: () => import('@/features/catalog/pages/OptionTypeDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-list' },
    },
  ],
}
