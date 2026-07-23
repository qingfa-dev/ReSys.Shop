import type { CreateProductForm, UpdateProductForm } from '../schemas'

export type CreateProductRequest = CreateProductForm
export type UpdateProductRequest = UpdateProductForm

export interface ProductListParams {
  page?: number
  pageSize?: number
  search?: string
  sortBy?: string
  sortDirection?: 'asc' | 'desc'
  status?: 'Draft' | 'Active' | 'Archived'
}
