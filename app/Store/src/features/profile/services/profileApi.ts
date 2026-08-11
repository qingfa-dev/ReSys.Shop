import { get, put, patch } from '@/shared/api/client'
import { ProfileDetailSchema } from '../validations/profile'
import type { Result } from '@/shared/types'
import type { ProfileDetail, UpdateProfileRequest } from '../types'

export class ProfileApi {
  private static readonly BASE = '/api/storefront/customer'

  // Call: Fetch authenticated user profile from profile API
  static async getProfile(): Promise<Result<ProfileDetail>> {
    const result = await get<Result<ProfileDetail>>(this.BASE)
    if (!result.isSuccess) return result
    // Transform: Validate server response against ProfileDetailSchema
    result.value = ProfileDetailSchema.parse(result.value)
    return result
  }

  // Call: Submit profile changes to profile API
  static async updateProfile(req: UpdateProfileRequest): Promise<Result<ProfileDetail>> {
    const result = await patch<Result<ProfileDetail>>(this.BASE, req)
    if (!result.isSuccess) return result
    // Transform: Validate updated profile response
    result.value = ProfileDetailSchema.parse(result.value)
    return result
  }

  // Call: Send profile deletion request
  static async deleteProfile(): Promise<Result<void>> {
    return await put<Result<void>>(`${this.BASE}/delete`, {})
  }
}
