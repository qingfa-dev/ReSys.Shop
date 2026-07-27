import type { CreateCountryForm, UpdateCountryForm } from '../schemas'
import type { CreateCountryRequest, UpdateCountryRequest } from '../types'

export class CountryFormMapper {
  static toCreate(form: CreateCountryForm): CreateCountryRequest { return form }
  static toUpdate(form: UpdateCountryForm): UpdateCountryRequest { return form }
}
