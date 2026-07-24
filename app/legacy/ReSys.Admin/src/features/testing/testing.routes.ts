import type { RouteRecordRaw } from 'vue-router'

export const testingRoutes: RouteRecordRaw = {
  path: 'testing',
  meta: { breadcrumb: 'navigation.testing' },
  children: [
    {
      path: 'examples',
      meta: { breadcrumb: 'titles.breadcrumb_parent' },
      children: [
        {
          path: '',
          name: 'testing.examples.list',
          component: () => import('@/features/testing/examples/views/example-list.view.vue'),
        },
        {
          path: 'create',
          name: 'testing.examples.create',
          component: () => import('@/features/testing/examples/views/example-form.view.vue'),
          meta: { breadcrumb: 'actions.create' },
        },
        {
          path: 'edit/:id',
          name: 'testing.examples.edit',
          component: () => import('@/features/testing/examples/views/example-form.view.vue'),
          meta: { breadcrumb: 'actions.edit' },
        },
      ],
    },
    {
      path: 'example-categories',
      meta: { breadcrumb: 'titles.breadcrumb_parent' },
      children: [
        {
          path: '',
          name: 'testing.example-categories.list',
          component: () =>
            import(
              '@/features/testing/example-categories/views/example-category-list.view.vue'
            ),
        },
        {
          path: 'create',
          name: 'testing.example-categories.create',
          component: () =>
            import(
              '@/features/testing/example-categories/views/example-category-form.view.vue'
            ),
          meta: { breadcrumb: 'actions.create' },
        },
        {
          path: 'edit/:id',
          name: 'testing.example-categories.edit',
          component: () =>
            import(
              '@/features/testing/example-categories/views/example-category-form.view.vue'
            ),
          meta: { breadcrumb: 'actions.edit' },
        },
      ],
    },
  ],
}
