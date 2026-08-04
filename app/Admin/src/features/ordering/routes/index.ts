import type { RouteRecordRaw } from 'vue-router'

const OrdersList = () => import('../views/OrdersList.vue')
const OrderDetail = () => import('../views/OrderDetail.vue')

export const orderingRoutes: RouteRecordRaw[] = [
  {
    path: 'ordering',
    redirect: { name: 'ordering-orders' },
  },
  {
    path: 'ordering/orders',
    name: 'ordering-orders',
    component: OrdersList,
    meta: { title: 'Orders' },
  },
  {
    path: 'ordering/orders/:id',
    name: 'ordering-order-detail',
    component: OrderDetail,
    meta: { title: 'Order Detail' },
  },
]

export const orderingMenuItems = [
  {
    label: 'Ordering',
    icon: 'pi pi-fw pi-shopping-cart',
    items: [
      { label: 'Orders', icon: 'pi pi-fw pi-list', route: '/ordering/orders' },
    ],
  },
]
