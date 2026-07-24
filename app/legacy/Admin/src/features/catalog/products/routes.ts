import type { RouteRecordRaw } from 'vue-router'

export const productRoutes: RouteRecordRaw[] = [
  {
    path: 'products',
    children: [
      {
        path: '',
        name: 'catalog.products.list',
        component: () => import('./pages/ProductListPage.vue'),
      },
      {
        path: 'create',
        name: 'catalog.products.create',
        component: () => import('./pages/ProductFormPage.vue'),
      },
      {
        path: ':id/edit',
        name: 'catalog.products.edit',
        component: () => import('./pages/ProductFormPage.vue'),
      },
    ],
  },
]
