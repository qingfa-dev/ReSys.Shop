export interface UpdateUserPreferencesRequest {
  currency?: string
  language?: string
  timezone?: string
  newsletter?: boolean
  notifications?: {
    email?: boolean
    sms?: boolean
    push?: boolean
  }
}

export interface UpdateAccountSettingsRequest {
  email?: string
  firstName?: string
  lastName?: string
  phone?: string
  avatar?: string
}

export interface DeleteAccountRequest {
  confirmation: string
  reason?: string
}

export interface ExportDataRequest {
  format?: 'json' | 'csv' | 'xml'
}