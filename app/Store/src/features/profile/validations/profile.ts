import { z } from 'zod'

import { zodMessages } from '@/shared/validations/messages'

// Validate: Full profile shape returned by GET /profiles/profiles
export const ProfileDetailSchema = z.object({
  id: z.string(),
  userId: z.string(),
  fullName: z.string(),
  firstName: z.string(),
  lastName: z.string(),
  email: z.string().email(),
  phoneNumber: z.string().nullable(),
  dateOfBirth: z.string().nullable(),
  preferences: z.record(z.string(), z.unknown()).nullable(),
  notifications: z.record(z.string(), z.boolean()).nullable(),
  emailConfirmed: z.boolean(),
  phoneNumberConfirmed: z.boolean(),
  createdAtUtc: z.string(),
  modifiedAtUtc: z.string().nullable(),
})

// Enforce: Name fields required; email must be valid RFC 5322 format
export const UpdateProfileRequestSchema = z.object({
  firstName: z.string().min(1, zodMessages.required('First name')).max(200, zodMessages.maxLength('First name', 200)),
  lastName: z.string().min(1, zodMessages.required('Last name')).max(200, zodMessages.maxLength('Last name', 200)),
  email: z.string().email(zodMessages.email),
  phoneNumber: z.string().optional(),
})
