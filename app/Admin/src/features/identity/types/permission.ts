export interface PermissionItem {
  identifier: string
  name: string
  description: string
  action: string
  isAssigned: boolean
}

export interface PermissionResource {
  resource: string
  description: string | null
  permissions: PermissionItem[]
}

export interface PermissionCategory {
  category: string
  description: string | null
  resources: PermissionResource[]
}

export interface PermissionGroupResponse {
  categories: PermissionCategory[]
}

export interface PermissionMetadata {
  domain: string
  category: string
  resource: string
  action: string
  identifier: string
  name: string
  description: string
}
