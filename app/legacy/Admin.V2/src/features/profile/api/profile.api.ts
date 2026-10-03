import apiClient from '@/shared/api/client'
import type { Result } from '@/shared/models'
import type { ProfileResponse, UpdateProfileRequest } from '../types'

export class ProfileApi {
  static async get(): Promise<Result<ProfileResponse>> {
    const res = await apiClient.get<Result<ProfileResponse>>('/profiles/profiles')
    return res.data
  }
  static async create(data: UpdateProfileRequest): Promise<Result<ProfileResponse>> {
    const res = await apiClient.post<Result<ProfileResponse>>('/profiles/profiles', data)
    return res.data
  }
  static async update(data: UpdateProfileRequest): Promise<Result<ProfileResponse>> {
    const res = await apiClient.put<Result<ProfileResponse>>('/profiles/profiles', data)
    return res.data
  }
  static async delete(): Promise<Result<void>> {
    const res = await apiClient.delete<Result<void>>('/profiles/profiles')
    return res.data
  }
}
