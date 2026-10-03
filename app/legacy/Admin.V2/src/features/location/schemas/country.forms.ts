import { z } from 'zod'
import type { TFunction } from './country.fields'
import { CountryFields } from './country.fields'

export class CountryForms {
  private f: CountryFields
  constructor(private t: TFunction) { this.f = new CountryFields(t) }
  create() { return z.object({ name: this.f.name(), isoCode: this.f.isoCode(), iso3Code: this.f.iso3Code(), numericCode: this.f.numericCode(), phoneCode: this.f.phoneCode(), isActive: this.f.isActive() }) }
  update() { return this.create() }
}
export type CreateCountryForm = z.input<ReturnType<CountryForms['create']>>
export type UpdateCountryForm = z.input<ReturnType<CountryForms['update']>>
