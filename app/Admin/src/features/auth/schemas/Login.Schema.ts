import { z } from 'zod'

export function createLoginSchema(t: (key: string, args?: Record<string, unknown>) => string) {
  return z.object({
  credential: z
    .string()
    .min(1, t('auth.validation.credential.required'))
    .max(255, t('auth.validation.credential.max_length')),
  password: z
    .string()
    .min(1, t('auth.validation.password.required'))
    .max(128, t('auth.validation.password.max_length')),
  rememberMe: z.boolean().optional().default(false),
})
}

export type LoginParameters = z.infer<ReturnType<typeof createLoginSchema>>
