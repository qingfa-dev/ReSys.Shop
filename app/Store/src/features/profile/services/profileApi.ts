import { get, put } from '@/shared/api/client'
import { PROFILES } from '@/shared/constants/api'
import { ProfileDetailSchema } from '../validations/profile'
import type { Result } from '@/shared/types'
import type { ProfileDetail, UpdateProfileRequest } from '../types'

export class ProfileApi {
  private static readonly BASE = `${PROFILES}/profiles`

  static async getProfile(): Promise<Result<ProfileDetail>> {
    const result = await get<Result<ProfileDetail>>(this.BASE)
    if (!result.isSuccess) return result
    result.value = ProfileDetailSchema.parse(result.value)
    return result
  }

  static async updateProfile(req: UpdateProfileRequest): Promise<Result<ProfileDetail>> {
    const result = await put<Result<ProfileDetail>>(this.BASE, req)
    if (!result.isSuccess) return result
    result.value = ProfileDetailSchema.parse(result.value)
    return result
  }

  static async deleteProfile(): Promise<Result<void>> {
    return await put<Result<void>>(`${this.BASE}/delete`, {})
  }
}
