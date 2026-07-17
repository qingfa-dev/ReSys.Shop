import { z } from 'zod'

export const ProfileSchema = z.object({
  firstName: z.string().min(1, 'First name is required').max(100, 'First name must not exceed 100 characters'),
  lastName: z.string().min(1, 'Last name is required').max(100, 'Last name must not exceed 100 characters'),
  phoneNumber: z.string().max(30, 'Phone number must not exceed 30 characters').optional(),
  dateOfBirth: z.string().optional(),
  gender: z.string().max(50).optional(),
  bio: z.string().max(1000, 'Bio must not exceed 1000 characters').optional(),
  avatarUrl: z.string().url('Invalid URL').optional().nullable(),
  acceptsEmailMarketing: z.boolean().optional(),
})

export type ProfileParameters = z.infer<typeof ProfileSchema>
