import type { RouteRecordRaw } from 'vue-router'
import { taxonRoutes } from './taxa/routes'

export const catalogRoutes: RouteRecordRaw = {
  path: 'catalog',
  meta: { breadcrumb: 'navigation.catalog' },
  children: [
    {
      path: '',
      name: 'catalog.dashboard',
      component: () => import('@/features/catalog/dashboard/pages/CatalogDashboardPage.vue'),
    },
    {
      path: 'products',
      meta: { breadcrumb: 'Products' },
      children: [
        {
          path: '',
          name: 'catalog.products.list',
          component: () => import('@/features/catalog/products/pages/ProductListPage.vue'),
        },
        {
          path: 'create',
          name: 'catalog.products.create',
          component: () => import('@/features/catalog/products/pages/ProductFormPage.vue'),
          meta: { breadcrumb: 'Create' },
        },
        {
          path: ':id/edit',
          name: 'catalog.products.edit',
          component: () => import('@/features/catalog/products/pages/ProductFormPage.vue'),
          meta: { breadcrumb: 'Edit' },
        },
      ],
    },
    {
      path: 'taxonomies',
      meta: { breadcrumb: 'Taxonomies' },
      component: () => import('@/features/catalog/taxonomies/pages/TaxonomyManagerPage.vue'),
      children: [
        {
          path: '',
          name: 'catalog.taxonomies.list',
          component: () => import('@/shared/components/feedback/ManagerWelcome.vue'),
          props: {
            title: 'Hierarchy Manager',
            description: 'Select a taxonomy from the left to edit its configuration, or create a new one to start a new product hierarchy.',
            icon: 'pi-sitemap'
          }
        },
        {
          path: 'create',
          name: 'catalog.taxonomies.create',
          component: () => import('@/features/catalog/taxonomies/pages/TaxonomyFormPage.vue'),
          meta: { breadcrumb: 'Create' },
        },
        {
          path: ':id/edit',
          name: 'catalog.taxonomies.edit',
          component: () => import('@/features/catalog/taxonomies/pages/TaxonomyFormPage.vue'),
          meta: { breadcrumb: 'Edit' },
        },
      ],
    },
    ...taxonRoutes,
    {
      path: 'option-types',
      meta: { breadcrumb: 'Option Types' },
      component: () => import('@/features/catalog/option-types/pages/OptionTypeManagerPage.vue'),
      children: [
        {
          path: '',
          name: 'catalog.option-types.list',
          component: () => import('@/shared/components/feedback/ManagerWelcome.vue'),
          props: {
            title: 'Option Type Manager',
            description: 'Select an option type from the left to edit its configuration and values, or create a new one to add more product attributes.',
            icon: 'pi-list'
          }
        },
        {
          path: 'create',
          name: 'catalog.option-types.create',
          component: () => import('@/features/catalog/option-types/pages/OptionTypeFormPage.vue'),
          meta: { breadcrumb: 'Create' },
        },
        {
          path: ':id/edit',
          name: 'catalog.option-types.edit',
          component: () => import('@/features/catalog/option-types/pages/OptionTypeFormPage.vue'),
          meta: { breadcrumb: 'Edit' },
        },
      ],
    },
    {
      path: 'option-values',
      name: 'catalog.option-values.list',
      component: () => import('@/features/catalog/option-values/pages/OptionValueListPage.vue'),
      meta: { breadcrumb: 'Option Values' },
    },

  ],
}
