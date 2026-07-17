import { z } from 'zod'

export const PermissionSchema = z.object({
  identifier: z.string().min(1, 'Identifier is required').max(200, 'Identifier must not exceed 200 characters').regex(/^[a-z][a-z0-9_.]+$/, 'Identifier format: lowercase letters, numbers, underscores, dots'),
  name: z.string().min(1, 'Name is required').max(100, 'Name must not exceed 100 characters'),
  description: z.string().max(500, 'Description must not exceed 500 characters').optional(),
  action: z.string().min(1, 'Action is required').max(100, 'Action must not exceed 100 characters'),
})

export type PermissionParameters = z.infer<typeof PermissionSchema>
