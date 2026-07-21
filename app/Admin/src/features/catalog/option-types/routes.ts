import type { RouteRecordRaw } from 'vue-router'

export const optionTypeRoutes: RouteRecordRaw[] = [
  {
    path: 'option-types',
    component: () => import('./pages/OptionTypeManagerPage.vue'),
    children: [
      {
        path: '',
        name: 'catalog.option-types.list',
        component: () => import('@/shared/components/feedback/ManagerWelcome.vue'),
        props: {
          title: 'Option Type Manager',
          description: 'Select an option type from the left to edit its configuration and values, or create a new one to add more product attributes.',
          icon: 'pi-list',
        },
      },
      {
        path: 'create',
        name: 'catalog.option-types.create',
        component: () => import('./pages/OptionTypeFormPage.vue'),
      },
      {
        path: ':id/edit',
        name: 'catalog.option-types.edit',
        component: () => import('./pages/OptionTypeFormPage.vue'),
      },
    ],
  },
]
