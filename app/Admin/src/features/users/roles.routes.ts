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
            component: () => import('./roles/views/RoleList.View.vue'),
            meta: {
                breadcrumb: 'List'
            }
        },
        {
            path: 'create',
            name: 'role-create',
            component: () => import('./roles/views/RoleForm.View.vue'),
            meta: {
                breadcrumb: 'Create Role'
            }
        },
        {
            path: ':id/edit',
            name: 'role-edit',
            component: () => import('./roles/views/RoleForm.View.vue'),
            meta: {
                breadcrumb: 'Edit Role'
            }
        },
        {
            path: ':id/permissions',
            name: 'role-permissions',
            component: () => import('./roles/views/RolePermissionsManager.View.vue'),
            meta: {
                breadcrumb: 'Manage Permissions'
            }
        }
    ]
};
