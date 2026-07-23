import { z } from 'zod'

export type TFunction = (key: string) => string

export class CatalogFields {
  constructor(private t: TFunction) {}

  name() {
    return z
      .string()
      .min(1, this.t('catalog.validation.name.required'))
      .max(255, this.t('catalog.validation.name.max_length'))
  }

  slug() {
    return z
      .string()
      .min(1, this.t('catalog.validation.slug.required'))
      .regex(/^[a-z0-9-]+$/, this.t('catalog.validation.slug.format'))
  }

  description() {
    return z.string().optional().nullable()
  }

  status() {
    return z.enum(['Draft', 'Active', 'Archived']).optional()
  }

  presentation() {
    return z.string().optional().nullable()
  }

  department() {
    return z.string().optional().nullable()
  }

  genderTarget() {
    return z.string().optional().nullable()
  }

  styleCode() {
    return z.string().optional().nullable()
  }

  position() {
    return z.coerce.number().int().optional()
  }

  filterable() {
    return z.boolean().optional()
  }
}

export function createFields(t: TFunction) {
  return new CatalogFields(t)
}
