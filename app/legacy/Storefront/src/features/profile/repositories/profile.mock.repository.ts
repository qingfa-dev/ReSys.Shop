import type { ProfileResponse, ProfileSingleResponse } from '../types/response'
import { getProfileById } from '../data/mock-profiles.data'
import { mapEntityToResponse } from '../mapping'

export class MockProfileRepository {
  async getProfile(userId: string): Promise<ProfileSingleResponse> {
    const profile = getProfileById(userId)
    if (!profile) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Profile not found' }
    }
    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      data: mapEntityToResponse(profile),
    }
  }

  async updateProfile(userId: string, updates: Partial<ProfileResponse>): Promise<ProfileSingleResponse> {
    const profile = getProfileById(userId)
    if (!profile) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Profile not found' }
    }
    const updated: ProfileResponse = {
      ...mapEntityToResponse(profile),
      ...updates,
      updated_at: new Date().toISOString(),
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: updated }
  }

  async uploadAvatar(userId: string, file: File): Promise<ProfileSingleResponse> {
    const profile = getProfileById(userId)
    if (!profile) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Profile not found' }
    }
    const avatarUrl = URL.createObjectURL(file)
    const updated: ProfileResponse = {
      ...mapEntityToResponse(profile),
      avatar: avatarUrl,
      updated_at: new Date().toISOString(),
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: updated }
  }
}

export const mockProfileRepository = new MockProfileRepository()
