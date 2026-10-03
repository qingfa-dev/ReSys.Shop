export * from './schemas'
export * from './entity'
export * from './request'
export * from './response'

export interface UserPreferences {
  currency: string
  language: string
  timezone: string
  newsletter: boolean
  notifications: {
    email: boolean
    sms: boolean
    push: boolean
  }
}

export interface AccountSettings {
  email: string
  firstName: string
  lastName: string
  phone?: string
  avatar?: string
}
