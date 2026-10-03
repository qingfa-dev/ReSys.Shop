import type { Result } from '@/core/models/result'
import type { UserPreferencesSchemaType, AccountSettingsSchemaType } from '../schemas'

export interface UserPreferencesResponse extends UserPreferencesSchemaType {}
export interface AccountSettingsResponse extends AccountSettingsSchemaType {}

export interface GetUserPreferencesResponse {
  preferences: UserPreferencesResponse
}

export interface UpdateUserPreferencesResponse {
  preferences: UserPreferencesResponse
  updatedAt: string
}

export interface GetAccountSettingsResponse {
  account: AccountSettingsResponse
}

export interface UpdateAccountSettingsResponse {
  account: AccountSettingsResponse
  updatedAt: string
}

export interface DeleteAccountResponse {
  success: boolean
  deletedAt: string
}

export interface ExportDataResponse {
  downloadUrl: string
  expiresAt: string
}

export type UserPreferencesSingleResponse = Result<UserPreferencesResponse>
export type AccountSettingsSingleResponse = Result<AccountSettingsResponse>