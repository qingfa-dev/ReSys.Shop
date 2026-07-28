import type { UserEntity } from '../types/entity'
import type { UserResponse, AuthResponse, AuthTokensResponse } from '../types/response'
import type { UserSchemaType } from '../types/schemas'

export function mapResponseToEntity(response: UserResponse): UserEntity {
  return {
    id: response.id,
    email: response.email,
    firstName: response.firstName,
    lastName: response.lastName,
    phone: response.phone,
    avatar: response.avatar,
    role: response.role as UserEntity['role'],
    emailVerified: response.emailVerified,
    createdAt: response.createdAt,
    updatedAt: response.updatedAt,
  }
}

export function mapAuthResponseToEntity(response: AuthResponse): { user: UserEntity; tokens: import('../types/entity').AuthTokensEntity } {
  return {
    user: mapResponseToEntity(response.user),
    tokens: response.tokens as import('../types/entity').AuthTokensEntity,
  }
}

export function mapSchemaToEntity(schema: UserSchemaType): UserEntity {
  return {
    id: schema.id,
    email: schema.email,
    firstName: schema.firstName,
    lastName: schema.lastName,
    phone: schema.phone,
    avatar: schema.avatar,
    role: schema.role as UserEntity['role'],
    emailVerified: schema.emailVerified,
    createdAt: schema.createdAt,
    updatedAt: schema.updatedAt,
  }
}

export function mapEntityToResponse(entity: UserEntity): UserResponse {
  return {
    id: entity.id,
    email: entity.email,
    firstName: entity.firstName,
    lastName: entity.lastName,
    phone: entity.phone,
    avatar: entity.avatar,
    role: entity.role,
    emailVerified: entity.emailVerified,
    createdAt: entity.createdAt,
    updatedAt: entity.updatedAt,
  }
}