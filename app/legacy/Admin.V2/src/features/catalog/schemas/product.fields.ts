import { z } from 'zod'

export type TFunction = (key: string) => string

export class ProductFields {
  constructor(private t: TFunction) {}

  name() { return z.string().min(1, this.t('catalog.validation.name.required')) }
  slug() { return z.string().min(1, this.t('catalog.validation.slug.required')) }
  description() { return z.string().optional() }
  status() { return z.union([z.literal('Draft'), z.literal('Active'), z.literal('Archived')]).optional() }
  department() { return z.string().optional() }
  genderTarget() { return z.string().optional() }
  styleCode() { return z.string().optional() }
}
