import { z } from 'zod'

export const profileFirstName = z.string()
  .min(1, 'First name is required.')
  .max(100, 'First name cannot exceed 100 characters.')

export const profileLastName = z.string()
  .min(1, 'Last name is required.')
  .max(100, 'Last name cannot exceed 100 characters.')

export const profileEmail = z.string()
  .min(1, 'Email is required.')
  .email('A valid email address is required.')
  .max(255, 'Email cannot exceed 255 characters.')

export const profilePhoneNumber = z.string()
  .max(20, 'Phone number cannot exceed 20 characters.')
  .optional()

export const profileDateOfBirth = z.string()
  .optional()

export const profileSchema = z.object({
  userId: z.string()
    .min(1, 'User is required.'),
  firstName: profileFirstName,
  lastName: profileLastName,
  email: profileEmail,
  phoneNumber: profilePhoneNumber,
  dateOfBirth: profileDateOfBirth,
})

export type ProfileForm = z.infer<typeof profileSchema>
