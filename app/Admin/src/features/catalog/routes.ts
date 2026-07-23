import type { RouteRecordRaw } from 'vue-router'

const ROUTE = {
  DASHBOARD: 'catalog.dashboard',
  PRODUCTS: { LIST: 'catalog.products.list', CREATE: 'catalog.products.create', VIEW: 'catalog.products.view', EDIT: 'catalog.products.edit' },
  TAXONOMIES: { LIST: 'catalog.taxonomies.list', CREATE: 'catalog.taxonomies.create', VIEW: 'catalog.taxonomies.view', EDIT: 'catalog.taxonomies.edit' },
  OPTION_TYPES: { LIST: 'catalog.option-types.list', CREATE: 'catalog.option-types.create', VIEW: 'catalog.option-types.view', EDIT: 'catalog.option-types.edit' },
} as const

export { ROUTE }

export const catalogRoutes: RouteRecordRaw = {
  path: 'catalog',
  children: [
    { path: '', redirect: { name: ROUTE.DASHBOARD } },
    {
      path: 'dashboard',
      name: ROUTE.DASHBOARD,
      component: () => import('./pages/DashboardPage.vue'),
      meta: { icon: 'pi pi-fw pi-th-large' },
    },
    {
      path: 'products',
      name: ROUTE.PRODUCTS.LIST,
      component: () => import('./pages/ProductListPage.vue'),
      meta: { icon: 'pi pi-fw pi-shopping-bag' },
    },
    {
      path: 'products/new',
      name: ROUTE.PRODUCTS.CREATE,
      component: () => import('./pages/ProductDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-shopping-bag' },
    },
    {
      path: 'products/:id',
      name: ROUTE.PRODUCTS.VIEW,
      component: () => import('./pages/ProductDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-shopping-bag' },
    },
    {
      path: 'products/:id/edit',
      name: ROUTE.PRODUCTS.EDIT,
      component: () => import('./pages/ProductDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-shopping-bag' },
    },
    {
      path: 'taxonomies',
      name: ROUTE.TAXONOMIES.LIST,
      component: () => import('./pages/TaxonomyListPage.vue'),
      meta: { icon: 'pi pi-fw pi-sitemap' },
    },
    {
      path: 'taxonomies/new',
      name: ROUTE.TAXONOMIES.CREATE,
      component: () => import('./pages/TaxonomyDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-sitemap' },
    },
    {
      path: 'taxonomies/:id',
      name: ROUTE.TAXONOMIES.VIEW,
      component: () => import('./pages/TaxonomyDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-sitemap' },
    },
    {
      path: 'taxonomies/:id/edit',
      name: ROUTE.TAXONOMIES.EDIT,
      component: () => import('./pages/TaxonomyDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-sitemap' },
    },
    {
      path: 'option-types',
      name: ROUTE.OPTION_TYPES.LIST,
      component: () => import('./pages/OptionTypeListPage.vue'),
      meta: { icon: 'pi pi-fw pi-list' },
    },
    {
      path: 'option-types/new',
      name: ROUTE.OPTION_TYPES.CREATE,
      component: () => import('./pages/OptionTypeDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-list' },
    },
    {
      path: 'option-types/:id',
      name: ROUTE.OPTION_TYPES.VIEW,
      component: () => import('./pages/OptionTypeDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-list' },
    },
    {
      path: 'option-types/:id/edit',
      name: ROUTE.OPTION_TYPES.EDIT,
      component: () => import('./pages/OptionTypeDetailPage.vue'),
      meta: { icon: 'pi pi-fw pi-list' },
    },
  ],
}
