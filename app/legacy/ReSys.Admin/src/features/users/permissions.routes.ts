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
            component: () => import('./permissions/views/permission-list.view.vue'),
            meta: {
                breadcrumb: 'List'
            }
        }
    ]
};
