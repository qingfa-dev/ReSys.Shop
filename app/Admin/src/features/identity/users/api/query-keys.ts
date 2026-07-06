import { withFilters, withId } from '@/shared/api/query-keys'

export const usersQueryKeys = {
  all: ['users'] as const,
  list: (filters: Record<string, unknown> = {}) => withFilters(usersQueryKeys.all, filters),
  detail: (id: string) => withId(usersQueryKeys.all, id),
}
