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
          name: 'users.staff.list',
          component: () => import('./views/AdminUserList.View.vue'),
          meta: { breadcrumb: 'Staff' },
        },
        {
          path: 'create',
          name: 'users.staff.create',
          component: () => import('./views/StaffForm.View.vue'),
          meta: { breadcrumb: 'Invite Staff' },
        },
        {
          path: ':id',
          name: 'users.staff.detail',
          component: () => import('./views/StaffDetail.View.vue'),
          meta: { breadcrumb: 'Staff Details' },
        },
        {
          path: ':id/edit',
          name: 'users.staff.edit',
          component: () => import('./views/StaffForm.View.vue'),
          meta: { breadcrumb: 'Edit Staff' },
        },
      ],
    },
    {
      path: 'customers',
      children: [
        {
          path: '',
          name: 'users.customers.list',
          component: () => import('./views/CustomerList.View.vue'),
          meta: { breadcrumb: 'Customers' },
        },
        {
          path: ':id',
          name: 'users.customers.detail',
          component: () => import('./views/CustomerDetail.View.vue'),
          meta: { breadcrumb: 'Customer Details' },
        },
      ],
    },
  ],
}
