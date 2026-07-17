import { z } from 'zod'
import { LoginSchema, ChangePasswordSchema } from '../schemas/auth.schema'

export type LoginFormData = z.infer<typeof LoginSchema>
export type ChangePasswordFormData = z.infer<typeof ChangePasswordSchema>
