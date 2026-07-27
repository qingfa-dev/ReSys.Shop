import { z } from 'zod'
import type { TFunction } from './country.fields'
import { StateFields } from './state.fields'

export class StateForms {
  private f: StateFields
  constructor(private t: TFunction) { this.f = new StateFields(t) }
  create() { return z.object({ name: this.f.name(), isoCode: this.f.isoCode(), countryId: this.f.countryId(), isActive: this.f.isActive() }) }
  update() { return this.create() }
}
export type CreateStateForm = z.input<ReturnType<StateForms['create']>>
export type UpdateStateForm = z.input<ReturnType<StateForms['update']>>
