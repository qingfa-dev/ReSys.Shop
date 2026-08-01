import { z } from 'zod'

export const userEmail = z.string()
  .min(1, 'Email is required.')
  .email('A valid email address is required.')

export const userUserName = z.string()
  .min(1, 'Username is required.')

export const userFirstName = z.string()
  .min(1, 'First name is required.')

export const userLastName = z.string()
  .min(1, 'Last name is required.')

export const userPhoneNumber = z.string()
  .optional()

export const userEmailConfirmed = z.boolean()

export const userPhoneNumberConfirmed = z.boolean()

export const userSchema = z.object({
  email: userEmail,
  userName: userUserName,
  firstName: userFirstName,
  lastName: userLastName,
  phoneNumber: userPhoneNumber,
  emailConfirmed: userEmailConfirmed,
  phoneNumberConfirmed: userPhoneNumberConfirmed,
})

export type UserForm = z.infer<typeof userSchema>
