import type { RouteRecordRaw } from 'vue-router'

export const catalogRoutes: RouteRecordRaw = {
  path: 'catalog',
  meta: { breadcrumb: 'navigation.catalog' },
  children: [
    {
      path: '',
      name: 'catalog.dashboard',
      component: () => import('@/features/catalog/dashboard/views/CatalogDashboard.View.vue'),
    },
    {
      path: 'products',
      meta: { breadcrumb: 'Products' },
      children: [
        {
          path: '',
          name: 'catalog.products.list',
          component: () => import('@/features/catalog/products/views/ProductList.View.vue'),
        },
        {
          path: 'create',
          name: 'catalog.products.create',
          component: () => import('@/features/catalog/products/views/ProductForm.View.vue'),
          meta: { breadcrumb: 'Create' },
        },
        {
          path: ':id/edit',
          name: 'catalog.products.edit',
          component: () => import('@/features/catalog/products/views/ProductForm.View.vue'),
          meta: { breadcrumb: 'Edit' },
        },
      ],
    },
    {
      path: 'taxonomies',
      meta: { breadcrumb: 'Taxonomies' },
      component: () => import('@/features/catalog/taxonomies/views/TaxonomyManager.View.vue'),
      children: [
        {
          path: '',
          name: 'catalog.taxonomies.list',
          component: () => import('@/shared/components/ManagerWelcome.Component.vue'),
          props: {
            title: 'Hierarchy Manager',
            description: 'Select a taxonomy from the left to edit its configuration, or create a new one to start a new product hierarchy.',
            icon: 'pi-sitemap'
          }
        },
        {
          path: 'create',
          name: 'catalog.taxonomies.create',
          component: () => import('@/features/catalog/taxonomies/views/TaxonomyForm.View.vue'),
          meta: { breadcrumb: 'Create' },
        },
        {
          path: ':id/edit',
          name: 'catalog.taxonomies.edit',
          component: () => import('@/features/catalog/taxonomies/views/TaxonomyForm.View.vue'),
          meta: { breadcrumb: 'Edit' },
        },
      ],
    },
    {
      path: 'categories',
      meta: { breadcrumb: 'Categories' },
      children: [
        {
          path: '',
          name: 'catalog.taxa.list',
          component: () => import('@/features/catalog/taxonomies/taxa/views/TaxonList.View.vue'),
        },
        {
          path: ':taxonomyId/manage',
          component: () => import('@/features/catalog/taxonomies/taxa/views/TaxonTreeManager.View.vue'),
          name: 'catalog.taxa.manager',
          children: [
            {
              path: 'create',
              name: 'catalog.taxa.create',
              component: () => import('@/features/catalog/taxonomies/taxa/views/TaxonForm.View.vue'),
              meta: { breadcrumb: 'Create' },
            },
            {
              path: ':id/edit',
              name: 'catalog.taxa.edit',
              component: () => import('@/features/catalog/taxonomies/taxa/views/TaxonForm.View.vue'),
              meta: { breadcrumb: 'Edit' },
            },
          ]
        }
      ]
    },
    {
      path: 'option-types',
      meta: { breadcrumb: 'Option Types' },
      component: () => import('@/features/catalog/option-types/views/OptionTypeManager.View.vue'),
      children: [
        {
          path: '',
          name: 'catalog.option-types.list',
          component: () => import('@/shared/components/ManagerWelcome.Component.vue'),
          props: {
            title: 'Option Type Manager',
            description: 'Select an option type from the left to edit its configuration and values, or create a new one to add more product attributes.',
            icon: 'pi-list'
          }
        },
        {
          path: 'create',
          name: 'catalog.option-types.create',
          component: () => import('@/features/catalog/option-types/views/OptionTypeForm.View.vue'),
          meta: { breadcrumb: 'Create' },
        },
        {
          path: ':id/edit',
          name: 'catalog.option-types.edit',
          component: () => import('@/features/catalog/option-types/views/OptionTypeForm.View.vue'),
          meta: { breadcrumb: 'Edit' },
        },
      ],
    },
    {
      path: 'option-values',
      name: 'catalog.option-values.list',
      component: () => import('@/features/catalog/option-types/option-values/views/OptionValueList.View.vue'),
      meta: { breadcrumb: 'Option Values' },
    },
    {
      path: 'property-types',
      meta: { breadcrumb: 'Property Types' },
      children: [
        {
          path: '',
          name: 'catalog.property-types.list',
          component: () =>
            import('@/features/catalog/property-types/views/PropertyTypeList.View.vue'),
        },
        {
          path: 'create',
          name: 'catalog.property-types.create',
          component: () =>
            import('@/features/catalog/property-types/views/PropertyTypeForm.View.vue'),
          meta: { breadcrumb: 'Create' },
        },
        {
          path: ':id/edit',
          name: 'catalog.property-types.edit',
          component: () =>
            import('@/features/catalog/property-types/views/PropertyTypeForm.View.vue'),
          meta: { breadcrumb: 'Edit' },
        },
      ],
    },
  ],
}
