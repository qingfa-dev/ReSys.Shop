import { productApiRepository } from '../../repositories/product/product.api'
import { buildProductFilter } from '../../types/params/product.params'
import type { IProductService } from './product.service.interface'
import type { Product, ProductFilter } from '../../types'
import type { PagedResult, Result } from '@/core/models/result'
import { mapResponseToEntity } from '../../mapping'
import { resultMap, succeed, fail } from '@/core/utils/result-helpers'

export class ProductService implements IProductService {
  private readonly productRepository = productApiRepository

  async getProducts(filter?: ProductFilter, page = 1, pageSize = 12): Promise<PagedResult<Product>> {
    const params = { ...buildProductFilter(filter ?? {}), page, pageSize }
    const response = await this.productRepository.getAll(params)
    return {
      ...response,
      items: response.items.map(mapResponseToEntity),
    }
  }

  async getProduct(id: string): Promise<Result<Product>> {
    const response = await this.productRepository.getById(id)
    return resultMap(response, mapResponseToEntity)
  }

  async getProductBySlug(slug: string): Promise<Result<Product>> {
    const response = await this.productRepository.getProductBySlug(slug)
    return resultMap(response, mapResponseToEntity)
  }

  async searchProducts(query: string, limit = 10): Promise<Result<Product[]>> {
    const response = await this.productRepository.getAll({
      search: query,
      searchFields: ['Name', 'Description'],
      page: 1,
      pageSize: limit,
    })
    if (response.isFailure) {
      return fail(response.message ?? 'Search failed', response.statusCode, response.errors)
    }
    return succeed(response.items.map(mapResponseToEntity), response.statusCode)
  }

  async getFeaturedProducts(limit = 8): Promise<Result<Product[]>> {
    const response = await this.productRepository.getAll({
      page: 1,
      pageSize: limit,
    })
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to get featured products', response.statusCode, response.errors)
    }
    return succeed(response.items.map(mapResponseToEntity), response.statusCode)
  }

  async getNewArrivals(limit = 8): Promise<Result<Product[]>> {
    const response = await this.productRepository.getAll({
      page: 1,
      pageSize: limit,
      sort: ['-CreatedAtUtc'],
    })
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to get new arrivals', response.statusCode, response.errors)
    }
    return succeed(response.items.map(mapResponseToEntity), response.statusCode)
  }
}

export const productService = new ProductService()