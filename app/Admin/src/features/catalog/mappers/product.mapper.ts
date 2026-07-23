import type { CreateProductForm, UpdateProductForm } from '../schemas'
import type { CreateProductRequest, UpdateProductRequest } from '../types'

export class ProductFormMapper {
  static toCreate(form: CreateProductForm): CreateProductRequest {
    return {
      name: form.name,
      slug: form.slug,
      description: form.description ?? null,
      status: form.status,
      department: form.department ?? null,
      genderTarget: form.genderTarget ?? null,
      styleCode: form.styleCode ?? null,
    }
  }

  static toUpdate(form: UpdateProductForm): UpdateProductRequest {
    return {
      name: form.name,
      slug: form.slug,
      description: form.description ?? null,
      status: form.status,
      department: form.department ?? null,
      genderTarget: form.genderTarget ?? null,
      styleCode: form.styleCode ?? null,
    }
  }
}
