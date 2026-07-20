import type { RouteRecordRaw } from 'vue-router';

export const rolesRoutes: RouteRecordRaw = {
    path: 'roles',
    meta: {
        breadcrumb: 'roles.title'
    },
    children: [
        {
            path: '',
            name: 'users.roles.list',
            component: () => import('./roles/pages/RoleListPage.vue'),
            meta: {
                breadcrumb: 'List'
            }
        },
        {
            path: 'create',
            name: 'users.roles.create',
            component: () => import('./roles/pages/RoleFormPage.vue'),
            meta: {
                breadcrumb: 'Create Role'
            }
        },
        {
            path: ':id/edit',
            name: 'users.roles.edit',
            component: () => import('./roles/pages/RoleFormPage.vue'),
            meta: {
                breadcrumb: 'Edit Role'
            }
        },
        {
            path: ':id/permissions',
            name: 'users.roles.permissions',
            component: () => import('./roles/pages/RolePermissionsManagerPage.vue'),
            meta: {
                breadcrumb: 'Manage Permissions'
            }
        }
    ]
};
