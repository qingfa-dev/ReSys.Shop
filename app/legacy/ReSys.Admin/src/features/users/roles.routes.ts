import type { RouteRecordRaw } from 'vue-router';

export const rolesRoutes: RouteRecordRaw = {
    path: '/roles',
    meta: {
        breadcrumb: 'Roles'
    },
    children: [
        {
            path: '',
            name: 'roles-list',
            component: () => import('./roles/views/role-list.view.vue'),
            meta: {
                breadcrumb: 'List'
            }
        },
        {
            path: 'create',
            name: 'role-create',
            component: () => import('./roles/views/role-form.view.vue'),
            meta: {
                breadcrumb: 'Create Role'
            }
        },
        {
            path: ':id/edit',
            name: 'role-edit',
            component: () => import('./roles/views/role-form.view.vue'),
            meta: {
                breadcrumb: 'Edit Role'
            }
        },
        {
            path: ':id/permissions',
            name: 'role-permissions',
            component: () => import('./roles/views/role-permissions-manager.view.vue'),
            meta: {
                breadcrumb: 'Manage Permissions'
            }
        }
    ]
};
