import { z } from 'zod'

export const RoleSchema = z.object({
  name: z.string().min(1, 'Role name is required').max(100, 'Role name must not exceed 100 characters').regex(/^[a-z_]+$/, 'Role name may only contain lowercase letters and underscores'),
  displayName: z.string().max(100, 'Display name must not exceed 100 characters').optional(),
  description: z.string().max(500, 'Description must not exceed 500 characters').optional(),
  priority: z.number().int('Priority must be a whole number').min(0, 'Priority must be non-negative').default(0),
})

export type RoleParameters = z.infer<typeof RoleSchema>
