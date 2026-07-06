import { withId } from '@/shared/api/query-keys'

export const authQueryKeys = {
  all: ['auth'] as const,
  currentUser: () => withId(authQueryKeys.all, 'current-user') as readonly unknown[],
}
