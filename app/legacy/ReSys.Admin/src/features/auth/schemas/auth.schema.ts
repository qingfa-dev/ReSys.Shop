import { z } from 'zod'

export const LoginSchema = z.object({
  credential: z.string().min(1, 'Email or Username is required'),
  password: z.string().min(1, 'Password is required'),
  rememberMe: z.boolean().optional(),
})

export type LoginFormData = z.infer<typeof LoginSchema>
