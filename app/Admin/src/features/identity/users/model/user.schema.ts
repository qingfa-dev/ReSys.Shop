import { z } from 'zod'

export const createUserSchema = z.object({
  email: z.string().email(),
  displayName: z.string().min(1).max(120),
  password: z.string().min(8),
  roleIds: z.array(z.string()).min(1),
})

export const updateUserSchema = z.object({
  id: z.string(),
  displayName: z.string().min(1).max(120).optional(),
  status: z.enum(['active', 'inactive', 'invited', 'suspended']).optional(),
  roleIds: z.array(z.string()).optional(),
})
