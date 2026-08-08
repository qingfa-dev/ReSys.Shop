import { z } from 'zod'

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

export const UpdateProfileRequestSchema = z.object({
  firstName: z.string().min(1).max(200),
  lastName: z.string().min(1).max(200),
  email: z.string().email(),
  phoneNumber: z.string().optional(),
})
