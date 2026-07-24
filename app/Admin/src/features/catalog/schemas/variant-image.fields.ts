import { z } from 'zod'

export type TFunction = (key: string) => string

export class VariantImageFields {
  constructor(private t: TFunction) {}

  alt() { return z.string().optional() }
  position() { return z.coerce.number().int().min(0) }
  type() { return z.string().min(1, this.t('catalog.validation.image_type.required')) }
}
