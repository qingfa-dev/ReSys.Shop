import { z } from 'zod'

export function createLoginSchema() {
  return z.object({
    credential: z
      .string()
      .min(1, 'Credential is required')
      .max(255, 'Credential is too long'),
    password: z
      .string()
      .min(1, 'Password is required')
      .max(128, 'Password is too long'),
    rememberMe: z.boolean().optional().default(false),
  })
}

export type LoginParameters = z.infer<ReturnType<typeof createLoginSchema>>
