import { z } from 'zod'
import type { TFunction } from './profile.fields'
import { ProfileFields } from './profile.fields'

export class ProfileForms {
  private f: ProfileFields
  constructor(private t: TFunction) { this.f = new ProfileFields(t) }
  update() { return z.object({ firstName: this.f.firstName(), lastName: this.f.lastName(), phone: this.f.phone(), avatarUrl: this.f.avatarUrl(), dateOfBirth: this.f.dateOfBirth() }) }
}
export type UpdateProfileForm = z.input<ReturnType<ProfileForms['update']>>
