import { z } from 'zod'

export const IDENTITY_FIELDS = {
  id: {
    Required: z.string().uuid('Invalid ID format'),
    Optional: z.string().uuid('Invalid ID format').optional(),
  },
  email: {
    Required: z.string().min(1, 'Email is required').email('Please enter a valid email'),
    Optional: z.string().email('Please enter a valid email').optional(),
  },
  password: {
    Required: z.string().min(1, 'Password is required').min(8, 'Password must be at least 8 characters'),
    Optional: z.string().min(8, 'Password must be at least 8 characters').optional(),
  },
  firstName: {
    Required: z.string().min(1, 'First name is required').max(100, 'First name must be less than 100 characters'),
    Optional: z.string().max(100).optional(),
  },
  lastName: {
    Required: z.string().min(1, 'Last name is required').max(100, 'Last name must be less than 100 characters'),
    Optional: z.string().max(100).optional(),
  },
  phone: {
    Optional: z.string().optional(),
  },
  avatar: {
    Optional: z.string().url('Invalid avatar URL').optional(),
  },
  role: {
    Required: z.enum(['customer', 'admin']),
    Optional: z.enum(['customer', 'admin']).optional(),
  },
  emailVerified: {
    Required: z.boolean(),
    Optional: z.boolean().optional(),
  },
  createdAt: {
    Required: z.string().datetime(),
    Optional: z.string().datetime().optional(),
  },
  updatedAt: {
    Required: z.string().datetime(),
    Optional: z.string().datetime().optional(),
  },
  accessToken: {
    Required: z.string().min(1, 'Access token is required'),
    Optional: z.string().optional(),
  },
  refreshToken: {
    Required: z.string().min(1, 'Refresh token is required'),
    Optional: z.string().optional(),
  },
  expiresIn: {
    Required: z.number().int().positive(),
    Optional: z.number().int().positive().optional(),
  },
  rememberMe: {
    Optional: z.boolean().optional(),
  },
} as const

export const UserFields = {
  Id: { Required: IDENTITY_FIELDS.id.Required, Optional: IDENTITY_FIELDS.id.Optional },
  Email: { Required: IDENTITY_FIELDS.email.Required, Optional: IDENTITY_FIELDS.email.Optional },
  Password: { Required: IDENTITY_FIELDS.password.Required, Optional: IDENTITY_FIELDS.password.Optional },
  FirstName: { Required: IDENTITY_FIELDS.firstName.Required, Optional: IDENTITY_FIELDS.firstName.Optional },
  LastName: { Required: IDENTITY_FIELDS.lastName.Required, Optional: IDENTITY_FIELDS.lastName.Optional },
  Phone: { Optional: IDENTITY_FIELDS.phone.Optional },
  Avatar: { Optional: IDENTITY_FIELDS.avatar.Optional },
  Role: { Required: IDENTITY_FIELDS.role.Required, Optional: IDENTITY_FIELDS.role.Optional },
  EmailVerified: { Required: IDENTITY_FIELDS.emailVerified.Required, Optional: IDENTITY_FIELDS.emailVerified.Optional },
  CreatedAt: { Required: IDENTITY_FIELDS.createdAt.Required, Optional: IDENTITY_FIELDS.createdAt.Optional },
  UpdatedAt: { Required: IDENTITY_FIELDS.updatedAt.Required, Optional: IDENTITY_FIELDS.updatedAt.Optional },
  AccessToken: { Required: IDENTITY_FIELDS.accessToken.Required, Optional: IDENTITY_FIELDS.accessToken.Optional },
  RefreshToken: { Required: IDENTITY_FIELDS.refreshToken.Required, Optional: IDENTITY_FIELDS.refreshToken.Optional },
  ExpiresIn: { Required: IDENTITY_FIELDS.expiresIn.Required, Optional: IDENTITY_FIELDS.expiresIn.Optional },
  RememberMe: { Optional: IDENTITY_FIELDS.rememberMe.Optional },
} as const

export const UserSchema = z.object({
  id: IDENTITY_FIELDS.id.Required,
  email: IDENTITY_FIELDS.email.Required,
  firstName: IDENTITY_FIELDS.firstName.Required,
  lastName: IDENTITY_FIELDS.lastName.Required,
  phone: IDENTITY_FIELDS.phone.Optional,
  avatar: IDENTITY_FIELDS.avatar.Optional,
  role: IDENTITY_FIELDS.role.Required,
  emailVerified: IDENTITY_FIELDS.emailVerified.Required,
  createdAt: IDENTITY_FIELDS.createdAt.Required,
  updatedAt: IDENTITY_FIELDS.updatedAt.Required,
})

export type User = z.infer<typeof UserSchema>

export const AuthTokensSchema = z.object({
  accessToken: IDENTITY_FIELDS.accessToken.Required,
  refreshToken: IDENTITY_FIELDS.refreshToken.Required,
  expiresIn: IDENTITY_FIELDS.expiresIn.Required,
})

export type AuthTokens = z.infer<typeof AuthTokensSchema>

export const UserRoleSchema = z.enum(['customer', 'admin'])
export type UserRole = z.infer<typeof UserRoleSchema>

export type UserSchemaType = User
export type AuthTokensSchemaType = AuthTokens