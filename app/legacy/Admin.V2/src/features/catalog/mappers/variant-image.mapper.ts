import type { VariantImageForm } from '../schemas'
import type { UpdateImageMetadataRequest } from '../types'

export class VariantImageFormMapper {
  static toUpdate(form: VariantImageForm): UpdateImageMetadataRequest {
    return {
      alt: form.alt ?? null,
      position: form.position,
      type: form.type,
    }
  }
}
