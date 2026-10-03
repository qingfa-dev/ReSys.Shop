import { z } from 'zod'

export type TFunction = (key: string) => string

export class TaxonomyFields {
  constructor(private t: TFunction) {}

  name() { return z.string().min(1, this.t('catalog.validation.name.required')) }
  presentation() { return z.string().optional() }
}
