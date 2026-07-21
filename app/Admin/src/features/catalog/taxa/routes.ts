import type { RouteRecordRaw } from 'vue-router'

export const taxonRoutes: RouteRecordRaw[] = [
  {
    path: 'categories',
    children: [
      {
        path: '',
        name: 'catalog.taxa.list',
        component: () => import('./pages/TaxonListPage.vue'),
      },
      {
        path: ':taxonomyId/manage',
        component: () => import('./pages/TaxonTreeManagerPage.vue'),
        name: 'catalog.taxa.manager',
        children: [
          {
            path: 'create',
            name: 'catalog.taxa.create',
            component: () => import('./pages/TaxonFormPage.vue'),
          },
          {
            path: ':id/edit',
            name: 'catalog.taxa.edit',
            component: () => import('./pages/TaxonFormPage.vue'),
          },
        ],
      },
    ],
  },
]
