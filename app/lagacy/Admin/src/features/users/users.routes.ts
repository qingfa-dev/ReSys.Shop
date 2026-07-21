import type { RouteRecordRaw } from 'vue-router'

export const usersRoutes: RouteRecordRaw = {
  path: 'users',
  meta: { breadcrumb: 'Users' },
  children: [
    {
      path: 'staff',
      meta: { breadcrumb: 'Staff' },
      children: [
        {
          path: '',
          name: 'users.staff.list',
          component: () => import('./pages/AdminUserListPage.vue'),
        },
        {
          path: 'create',
          name: 'users.staff.create',
          component: () => import('./pages/StaffFormPage.vue'),
          meta: { breadcrumb: 'Invite Staff' },
        },
        {
          path: ':id',
          name: 'users.staff.detail',
          component: () => import('./pages/StaffDetailPage.vue'),
          meta: { breadcrumb: 'Staff Details' },
        },
        {
          path: ':id/edit',
          name: 'users.staff.edit',
          component: () => import('./pages/StaffFormPage.vue'),
          meta: { breadcrumb: 'Edit Staff' },
        },
      ],
    },
    {
      path: 'customers',
      meta: { breadcrumb: 'Customers' },
      children: [
        {
          path: '',
          name: 'users.customers.list',
          component: () => import('./pages/CustomerListPage.vue'),
        },
        {
          path: ':id',
          name: 'users.customers.detail',
          component: () => import('./pages/CustomerDetailPage.vue'),
          meta: { breadcrumb: 'Customer Details' },
        },
      ],
    },
  ],
}
