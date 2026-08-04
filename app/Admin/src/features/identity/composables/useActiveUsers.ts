import { useActiveList } from '@/shared/composables'
import type { UserListItem } from '../types/user'
import { UserApi } from '../services/userApi'

export function useActiveUsers() {
  // Call: Identity service — registered users for the dashboard stat card
  return useActiveList<UserListItem>(() => UserApi.getUsers({}))
}
