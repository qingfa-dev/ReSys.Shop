import { z } from 'zod'

export const LoginSchema = z.object({
  credential: z
    .string()
    .min(1, 'Email or Username is required')
    .max(255, 'Credential must not exceed 255 characters'),
  password: z
    .string()
    .min(1, 'Password is required')
    .max(128, 'Password must not exceed 128 characters'),
  rememberMe: z.boolean().optional().default(false),
})

export type LoginParameters = z.infer<typeof LoginSchema>
