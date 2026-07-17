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
          component: () => import('./views/AdminUserList.View.vue'),
          meta: {
            breadcrumb: 'Staff'
          }
        },
        {
          path: 'create',
          name: 'admin-user-create',
          component: () => import('./views/StaffForm.View.vue'),
          meta: {
            breadcrumb: 'Invite Staff'
          }
        },
        {
          path: ':id',
          name: 'admin-user-detail',
          component: () => import('./views/StaffDetail.View.vue'),
          meta: {
            breadcrumb: 'Staff Details'
          }
        },
        {
          path: ':id/edit',
          name: 'admin-user-edit',
          component: () => import('./views/StaffForm.View.vue'),
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
          component: () => import('./views/CustomerList.View.vue'),
          meta: {
            breadcrumb: 'Customers'
          }
        },
        {
          path: ':id',
          name: 'customer-detail',
          component: () => import('./views/CustomerDetail.View.vue'), // Placeholder
          meta: {
            breadcrumb: 'Customer Details'
          }
        }
      ]
    }],
}
