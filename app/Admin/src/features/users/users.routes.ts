import type { RouteRecordRaw } from 'vue-router'

export const usersRoutes: RouteRecordRaw = {
  path: 'users',
  meta: { breadcrumb: 'Users' },
  children: [
    {
      path: 'staff',
      children: [
        {
          path: '',
          name: 'admin-users',
          component: () => import('./views/admin-user-list.view.vue'),
          meta: {
            breadcrumb: 'Staff'
          }
        },
        {
          path: 'create',
          name: 'admin-user-create',
          component: () => import('./views/staff-form.view.vue'),
          meta: {
            breadcrumb: 'Invite Staff'
          }
        },
        {
          path: ':id',
          name: 'admin-user-detail',
          component: () => import('./views/staff-detail.view.vue'),
          meta: {
            breadcrumb: 'Staff Details'
          }
        },
        {
          path: ':id/edit',
          name: 'admin-user-edit',
          component: () => import('./views/staff-form.view.vue'),
          meta: {
            breadcrumb: 'Edit Staff'
          }
        }
      ]
    },
    {
      path: 'customers',
      children: [
        {
          path: '',
          name: 'customer-users',
          component: () => import('./views/customer-list.view.vue'),
          meta: {
            breadcrumb: 'Customers'
          }
        },
        {
          path: ':id',
          name: 'customer-detail',
          component: () => import('./views/customer-detail.view.vue'), // Placeholder
          meta: {
            breadcrumb: 'Customer Details'
          }
        }
      ]
    }],
}
