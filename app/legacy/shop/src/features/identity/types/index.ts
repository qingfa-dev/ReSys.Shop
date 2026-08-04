export * from './response'
export * from './request'

export {
  IDENTITY_FIELDS as fields,
  UserFields,
  UserSchema,
  AuthTokensSchema,
  UserRoleSchema,
} from './schemas'
export type {
  User,
  AuthTokens,
  UserRole,
  UserSchemaType,
  AuthTokensSchemaType,
} from './schemas'

export type AuthResponse = {
  user: import('./schemas/user.schema').User
  tokens: import('./schemas/user.schema').AuthTokens
}
