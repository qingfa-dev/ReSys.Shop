import type { Profile } from '../types/profile.domain.types'

export function mapProfileResponse(dto: Profile): Profile {
  return {
    ...dto,
    id: dto.id,
    email: dto.email,
    firstName: dto.firstName,
    lastName: dto.lastName,
    phoneNumber: dto.phoneNumber,
    dateOfBirth: dto.dateOfBirth,
    gender: dto.gender,
    bio: dto.bio,
    avatarUrl: dto.avatarUrl,
    preferences: dto.preferences,
    notifications: dto.notifications,
    isActive: dto.isActive,
    acceptsEmailMarketing: dto.acceptsEmailMarketing,
    createdAtUtc: dto.createdAtUtc,
    modifiedAtUtc: dto.modifiedAtUtc,
  }
}
