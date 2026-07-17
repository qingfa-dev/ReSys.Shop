import type { RouteRecordRaw } from 'vue-router';

export const permissionsRoutes: RouteRecordRaw = {
    path: '/permissions',
    meta: {
        breadcrumb: 'Permissions'
    },
    children: [
        {
            path: '',
            name: 'permissions-list',
            component: () => import('./permissions/views/PermissionList.View.vue'),
            meta: {
                breadcrumb: 'List'
            }
        }
    ]
};
