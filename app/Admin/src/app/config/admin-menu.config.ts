import type { RouteLocationRaw } from 'vue-router'

export interface MenuItem {
  label: string
  icon?: string
  to?: RouteLocationRaw
  url?: string
  target?: string
  items?: MenuItem[]
  permission?: string
  visible?: boolean
  separator?: boolean
  badge?: string | number
  class?: string
  disabled?: boolean
  command?: (event: { originalEvent: Event; item: MenuItem }) => void
}

export interface MenuGroup {
  label: string
  icon?: string
  path?: string
  separator?: boolean
  items: MenuItem[]
}

export const adminMenuConfig: MenuGroup[] = [
  {
    label: 'Home',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-home', to: { name: 'reports.dashboard' } },
      { label: 'My Profile', icon: 'pi pi-fw pi-user', to: { name: 'profile.view' } },
    ],
  },
  {
    label: 'Catalog',
    path: '/catalog',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-th-large', to: { name: 'catalog.dashboard' }, permission: 'Catalog' },
      {
        label: 'Products',
        icon: 'pi pi-fw pi-shopping-bag',
        permission: 'Catalog.Products',
        items: [
          { label: 'All Products', icon: 'pi pi-fw pi-list', to: { name: 'catalog.products.list' } },
        ],
      },
      {
        label: 'Categories',
        icon: 'pi pi-fw pi-sitemap',
        permission: 'Catalog.Taxonomies',
        items: [
          { label: 'All Taxonomies', icon: 'pi pi-fw pi-tags', to: { name: 'catalog.taxonomies.list' } },
        ],
      },
      {
        label: 'Option Types',
        icon: 'pi pi-fw pi-list',
        permission: 'Catalog.OptionTypes',
        items: [
          { label: 'All Types', icon: 'pi pi-fw pi-list', to: { name: 'catalog.option-types.list' } },
        ],
      },
    ],
  },
  {
    label: 'Inventory',
    path: '/inventory',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-chart-bar', to: { name: 'inventory.dashboard' }, permission: 'Inventory' },
      { label: 'Stock Items', icon: 'pi pi-fw pi-box', to: { name: 'inventory.stocks.list' }, permission: 'Inventory' },
      { label: 'Locations', icon: 'pi pi-fw pi-building', to: { name: 'inventory.locations.list' }, permission: 'Inventory' },
      { label: 'Movements', icon: 'pi pi-fw pi-history', to: { name: 'inventory.movements.list' }, permission: 'Inventory' },
      { label: 'Transfers', icon: 'pi pi-fw pi-arrow-right-arrow-left', to: { name: 'inventory.transfers.list' }, permission: 'Inventory' },
    ],
  },
  {
    label: 'Orders',
    path: '/ordering',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-chart-line', to: { name: 'ordering.dashboard' }, permission: 'Ordering' },
      {
        label: 'All Orders',
        icon: 'pi pi-fw pi-shopping-cart',
        permission: 'Ordering.Orders',
        items: [
          { label: 'List', icon: 'pi pi-fw pi-list', to: { name: 'ordering.orders.list' } },
        ],
      },
      { label: 'Fulfillment', icon: 'pi pi-fw pi-truck', to: { name: 'ordering.fulfillment.queue' }, permission: 'Ordering.Fulfillment' },
    ],
  },
  {
    label: 'Payments',
    path: '/payments',
    items: [
      { label: 'All Payments', icon: 'pi pi-fw pi-wallet', to: { name: 'payment.payments.list' }, permission: 'Payment' },
      { label: 'Payment Methods', icon: 'pi pi-fw pi-credit-card', to: { name: 'payment.methods.list' }, permission: 'Payment' },
    ],
  },
  {
    label: 'Shipping',
    path: '/shipping',
    items: [
      { label: 'Methods', icon: 'pi pi-fw pi-truck', to: { name: 'shipping.methods.list' }, permission: 'Shipping' },
      { label: 'Rates', icon: 'pi pi-fw pi-tag', to: { name: 'shipping.rates.list' }, permission: 'Shipping' },
    ],
  },
  {
    label: 'Locations',
    path: '/locations',
    items: [
      { label: 'Countries', icon: 'pi pi-fw pi-globe', to: { name: 'location.countries.list' }, permission: 'Location' },
      { label: 'States', icon: 'pi pi-fw pi-map', to: { name: 'location.states.list' }, permission: 'Location' },
    ],
  },
  {
    label: 'Users',
    path: '/users',
    items: [
      {
        label: 'Staff',
        icon: 'pi pi-fw pi-id-card',
        permission: 'Identity.Users.Staff',
        items: [
          { label: 'All Staff', icon: 'pi pi-fw pi-list', to: { name: 'users.staff.list' } },
        ],
      },
      { label: 'Customers', icon: 'pi pi-fw pi-users', to: { name: 'users.customers.list' }, permission: 'Identity.Users.Customers' },
      { label: 'Addresses', icon: 'pi pi-fw pi-address-book', to: { name: 'profile.addresses' }, permission: 'Identity.Users' },
    ],
  },
  {
    label: 'Access Control',
    path: '/users',
    items: [
      { label: 'Roles', icon: 'pi pi-fw pi-shield', to: { name: 'users.roles.list' }, permission: 'Identity.Roles' },
      { label: 'Permissions', icon: 'pi pi-fw pi-key', to: { name: 'users.permissions.list' }, permission: 'Identity.Permissions' },
    ],
  },
]
