import { z } from 'zod'
import { ProductFields } from './product.fields'
import type { TFunction } from './product.fields'

export class ProductForms {
  private f: ProductFields
  constructor(private t: TFunction) { this.f = new ProductFields(t) }

  create() {
    return z.object({
      name: this.f.name(),
      slug: this.f.slug(),
      description: this.f.description(),
      status: this.f.status(),
      department: this.f.department(),
      genderTarget: this.f.genderTarget(),
      styleCode: this.f.styleCode(),
    })
  }

  update() {
    return z.object({
      name: this.f.name(),
      slug: this.f.slug(),
      description: this.f.description(),
      status: this.f.status(),
      department: this.f.department(),
      genderTarget: this.f.genderTarget(),
      styleCode: this.f.styleCode(),
    })
  }
}

export type CreateProductForm = z.input<ReturnType<ProductForms['create']>>
export type UpdateProductForm = z.input<ReturnType<ProductForms['update']>>
