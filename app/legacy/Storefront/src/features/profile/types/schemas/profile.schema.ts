import { z } from 'zod'

export const PROFILE_FIELDS = {
  id: {
    Required: z.string().uuid('Invalid ID format'),
    Optional: z.string().uuid('Invalid ID format').optional(),
  },
  email: {
    Required: z.string().min(1, 'Email is required').email('Please enter a valid email'),
    Optional: z.string().email('Please enter a valid email').optional(),
  },
  firstName: {
    Required: z.string().min(1, 'First name is required').max(100, 'First name must be less than 100 characters'),
    Optional: z.string().max(100).optional(),
  },
  lastName: {
    Required: z.string().min(1, 'Last name is required').max(100, 'Last name must be less than 100 characters'),
    Optional: z.string().max(100).optional(),
  },
  displayName: {
    Required: z.string().min(1, 'Display name is required').max(200, 'Display name must be less than 200 characters'),
    Optional: z.string().max(200).optional(),
  },
  phone: {
    Optional: z.string().optional(),
  },
  avatar: {
    Optional: z.string().url('Invalid avatar URL').optional(),
  },
  dateOfBirth: {
    Optional: z.string().optional(),
  },
  gender: {
    Optional: z.string().optional(),
  },
  createdAt: {
    Required: z.string().datetime(),
    Optional: z.string().datetime().optional(),
  },
  updatedAt: {
    Required: z.string().datetime(),
    Optional: z.string().datetime().optional(),
  },
} as const

export const ProfileFields = {
  Id: { Required: PROFILE_FIELDS.id.Required, Optional: PROFILE_FIELDS.id.Optional },
  Email: { Required: PROFILE_FIELDS.email.Required, Optional: PROFILE_FIELDS.email.Optional },
  FirstName: { Required: PROFILE_FIELDS.firstName.Required, Optional: PROFILE_FIELDS.firstName.Optional },
  LastName: { Required: PROFILE_FIELDS.lastName.Required, Optional: PROFILE_FIELDS.lastName.Optional },
  DisplayName: { Required: PROFILE_FIELDS.displayName.Required, Optional: PROFILE_FIELDS.displayName.Optional },
  Phone: { Optional: PROFILE_FIELDS.phone.Optional },
  Avatar: { Optional: PROFILE_FIELDS.avatar.Optional },
  DateOfBirth: { Optional: PROFILE_FIELDS.dateOfBirth.Optional },
  Gender: { Optional: PROFILE_FIELDS.gender.Optional },
  CreatedAt: { Required: PROFILE_FIELDS.createdAt.Required, Optional: PROFILE_FIELDS.createdAt.Optional },
  UpdatedAt: { Required: PROFILE_FIELDS.updatedAt.Required, Optional: PROFILE_FIELDS.updatedAt.Optional },
} as const

export const ProfileSchema = z.object({
  id: PROFILE_FIELDS.id.Required,
  email: PROFILE_FIELDS.email.Required,
  firstName: PROFILE_FIELDS.firstName.Required,
  lastName: PROFILE_FIELDS.lastName.Required,
  displayName: PROFILE_FIELDS.displayName.Required,
  phone: PROFILE_FIELDS.phone.Optional,
  avatar: PROFILE_FIELDS.avatar.Optional,
  dateOfBirth: PROFILE_FIELDS.dateOfBirth.Optional,
  gender: PROFILE_FIELDS.gender.Optional,
  createdAt: PROFILE_FIELDS.createdAt.Required,
  updatedAt: PROFILE_FIELDS.updatedAt.Required,
})

export type ProfileSchemaType = z.infer<typeof ProfileSchema>
