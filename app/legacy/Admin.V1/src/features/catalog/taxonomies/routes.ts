import type { RouteRecordRaw } from 'vue-router'

export const taxonomyRoutes: RouteRecordRaw[] = [
  {
    path: 'taxonomies',
    component: () => import('./pages/TaxonomyManagerPage.vue'),
    children: [
      {
        path: '',
        name: 'catalog.taxonomies.list',
        component: () => import('@/shared/components/feedback/ManagerWelcome.vue'),
        props: {
          title: 'Hierarchy Manager',
          description: 'Select a taxonomy from the left to edit its configuration, or create a new one to start a new product hierarchy.',
          icon: 'pi-sitemap',
        },
      },
      {
        path: 'create',
        name: 'catalog.taxonomies.create',
        component: () => import('./pages/TaxonomyFormPage.vue'),
      },
      {
        path: ':id/edit',
        name: 'catalog.taxonomies.edit',
        component: () => import('./pages/TaxonomyFormPage.vue'),
      },
    ],
  },
]
