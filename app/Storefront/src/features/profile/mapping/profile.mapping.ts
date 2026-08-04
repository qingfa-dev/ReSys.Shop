import type { Profile } from '../types/entity'
import type { ProfileResponse } from '../types/response'

export function mapResponseToEntity(response: ProfileResponse): Profile {
  return {
    id: response.id,
    email: response.email,
    firstName: response.first_name,
    lastName: response.last_name,
    displayName: response.display_name,
    phone: response.phone,
    avatar: response.avatar,
    dateOfBirth: response.date_of_birth,
    gender: response.gender,
    createdAt: response.created_at,
    updatedAt: response.updated_at,
  }
}

export function mapEntityToResponse(entity: Profile): ProfileResponse {
  return {
    id: entity.id,
    email: entity.email,
    first_name: entity.firstName,
    last_name: entity.lastName,
    display_name: entity.displayName,
    phone: entity.phone,
    avatar: entity.avatar,
    date_of_birth: entity.dateOfBirth,
    gender: entity.gender,
    created_at: entity.createdAt,
    updated_at: entity.updatedAt,
  }
}
