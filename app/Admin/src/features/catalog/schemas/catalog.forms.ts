import { z } from 'zod'
import { CatalogFields } from './catalog.fields'
import type { TFunction } from './catalog.fields'

export class CatalogForms {
  private f: CatalogFields
  private t: TFunction

  constructor(t: TFunction) {
    this.t = t
    this.f = new CatalogFields(t)
  }

  createProduct() {
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

  updateProduct() {
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

  createTaxonomy() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
    })
  }

  updateTaxonomy() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
    })
  }

  createOptionType() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
      filterable: this.f.filterable(),
    })
  }

  updateOptionType() {
    return z.object({
      name: this.f.name(),
      presentation: this.f.presentation(),
      filterable: this.f.filterable(),
    })
  }
}

export type CreateProductForm = z.input<ReturnType<CatalogForms['createProduct']>>
export type UpdateProductForm = z.input<ReturnType<CatalogForms['updateProduct']>>
export type CreateTaxonomyForm = z.input<ReturnType<CatalogForms['createTaxonomy']>>
export type UpdateTaxonomyForm = z.input<ReturnType<CatalogForms['updateTaxonomy']>>
export type CreateOptionTypeForm = z.input<ReturnType<CatalogForms['createOptionType']>>
export type UpdateOptionTypeForm = z.input<ReturnType<CatalogForms['updateOptionType']>>
