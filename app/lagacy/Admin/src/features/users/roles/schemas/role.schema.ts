import { z } from 'zod'

export function createRoleSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  name: z.string().min(1, t('roles.validation.name.required')).max(100, t('roles.validation.name.max_length')).regex(/^[a-z_]+$/, t('roles.validation.name.format')),
  displayName: z.string().max(100, t('roles.validation.display_name.max_length')).optional(),
  description: z.string().max(500, t('catalog.validation.description.max_length')).optional(),
  priority: z.number().int(t('roles.validation.priority.whole')).min(0, t('roles.validation.priority.min')).default(0),
})
}

export type RoleParameters = z.infer<ReturnType<typeof createRoleSchema>>
