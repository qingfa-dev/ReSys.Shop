import { z } from 'zod'
import { VariantImageFields } from './variant-image.fields'
import type { TFunction } from './variant-image.fields'

export class VariantImageForms {
  private f: VariantImageFields
  constructor(private t: TFunction) { this.f = new VariantImageFields(t) }

  update() {
    return z.object({
      alt: this.f.alt(),
      position: this.f.position(),
      type: this.f.type(),
    })
  }
}

export type VariantImageForm = z.input<ReturnType<VariantImageForms['update']>>
