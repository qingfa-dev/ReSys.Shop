export const PERMISSIONS = {
  CATALOG: {
    VIEW: 'catalog.view',
    CREATE: 'catalog.create',
    EDIT: 'catalog.edit',
    DELETE: 'catalog.delete',
  },
  INVENTORY: {
    VIEW: 'inventory.view',
    CREATE: 'inventory.create',
    EDIT: 'inventory.edit',
    DELETE: 'inventory.delete',
  },
  ORDERING: {
    VIEW: 'ordering.view',
    CREATE: 'ordering.create',
    EDIT: 'ordering.edit',
    DELETE: 'ordering.delete',
    FULFILL: 'ordering.fulfill',
  },
  USERS: {
    VIEW: 'users.view',
    CREATE: 'users.create',
    EDIT: 'users.edit',
    DELETE: 'users.delete',
    MANAGE_ROLES: 'users.manage_roles',
  },
  SETTINGS: {
    VIEW: 'settings.view',
    EDIT: 'settings.edit',
  },
} as const

export type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS][keyof (typeof PERMISSIONS)[keyof typeof PERMISSIONS]]
