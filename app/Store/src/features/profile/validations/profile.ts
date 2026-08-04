import { z } from 'zod'

// Profile edit form schema. firstName/lastName are required by the backend update
// validator; email is required client-side because the update mapping writes the email
// unconditionally (sending an empty email would wipe the stored value).
export const profileSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  email: z.string().min(1, 'Email is required').email('Enter a valid email address'),
  phoneNumber: z.string(),
})

export type ProfileFormValues = z.infer<typeof profileSchema>
