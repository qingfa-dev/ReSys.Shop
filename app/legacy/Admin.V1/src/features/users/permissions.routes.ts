import type { RouteRecordRaw } from 'vue-router';

export const permissionsRoutes: RouteRecordRaw = {
    path: 'permissions',
    meta: {
        breadcrumb: 'permissions.title'
    },
    children: [
        {
            path: '',
            name: 'users.permissions.list',
            component: () => import('./permissions/pages/PermissionListPage.vue'),
            meta: {
                breadcrumb: 'List'
            }
        }
    ]
};
