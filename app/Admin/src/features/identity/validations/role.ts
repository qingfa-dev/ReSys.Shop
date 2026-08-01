import { z } from 'zod'

export const roleName = z.string()
  .min(1, 'Role name is required.')
  .max(64, 'Role name must not exceed 64 characters.')

export const roleDescription = z.string()
  .max(256, 'Role description must not exceed 256 characters.')
  .optional()

export const rolePresentation = z.string()
  .max(256, 'Role presentation must not exceed 256 characters.')
  .optional()

export const roleSchema = z.object({
  name: roleName,
  description: roleDescription,
  presentation: rolePresentation,
})

export type RoleForm = z.infer<typeof roleSchema>
