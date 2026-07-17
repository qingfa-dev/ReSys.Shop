import { profileApi } from './profile.api'

export const profileService = {
  getProfile: profileApi.get,
  updateProfile: profileApi.update,
}
