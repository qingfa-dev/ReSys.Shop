export const RouteName = {
  Login: 'login',
  Dashboard: 'dashboard',
  Users: 'users',
  UserCreate: 'user-create',
  UserEdit: 'user-edit',
  UserDetails: 'user-details',
} as const

export type RouteNameValue = (typeof RouteName)[keyof typeof RouteName]
