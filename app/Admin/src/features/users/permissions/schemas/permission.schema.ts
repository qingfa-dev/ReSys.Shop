import { z } from 'zod'

export function createPermissionSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  identifier: z.string().min(1, t('users.validation.identifier.required')).max(200, t('users.validation.identifier.max_length')).regex(/^[a-z][a-z0-9_.]+$/, t('users.validation.identifier.format')),
  name: z.string().min(1, t('catalog.validation.name.required')).max(100, t('catalog.validation.name.max_length')),
  description: z.string().max(500, t('catalog.validation.description.max_length')).optional(),
  action: z.string().min(1, t('users.validation.action.required')).max(100, t('users.validation.action.max_length')),
})
}

export type PermissionParameters = z.infer<ReturnType<typeof createPermissionSchema>>
