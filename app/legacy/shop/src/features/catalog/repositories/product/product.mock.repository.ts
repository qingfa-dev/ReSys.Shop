import type { ProductResponse, ProductListResponse, ProductSingleResponse } from '../../types/response'
import type { PagingParams, FilterParams, SearchParams, SortParams } from '@/core/models'
import { mockProducts, getProductById, getProductBySlug, getFeaturedProducts, getNewArrivals, searchProducts } from '../../data/mock-products.data'
import { filterByOperator, searchByFields, sortByField, paginateResults, buildFilters, createSearchConfig, createSortConfig } from '@/core/helpers/mock-query.helper'

export interface ProductQueryParams {
  paging?: PagingParams
  filter?: FilterParams
  search?: SearchParams
  sort?: SortParams
}

function mapToProductResponse(product: typeof mockProducts[0]): ProductResponse {
  return {
    id: product.id,
    name: product.name,
    slug: product.slug,
    description: product.description,
    price: product.price,
    compareAtPrice: product.compareAtPrice,
    images: product.images,
    category: {
      id: product.category.id,
      name: product.category.name,
      slug: product.category.slug,
      parentId: product.category.parentId,
      image: product.category.image,
    },
    tags: product.tags,
    variants: product.variants?.map(v => ({
      id: v.id,
      productId: v.productId,
      name: v.name,
      sku: v.sku,
      price: v.price,
      options: v.options.map(o => ({ name: o.name, value: o.value })),
      inventory: {
        quantity: v.inventory.quantity,
        trackQuantity: v.inventory.trackQuantity,
        allowBackorder: v.inventory.allowBackorder,
        lowStockThreshold: v.inventory.lowStockThreshold,
      },
    })),
    inventory: {
      quantity: product.inventory.quantity,
      trackQuantity: product.inventory.trackQuantity,
      allowBackorder: product.inventory.allowBackorder,
      lowStockThreshold: product.inventory.lowStockThreshold,
    },
    createdAt: product.createdAt,
    updatedAt: product.updatedAt,
  }
}

export class MockProductRepository {
  async getAll(params?: ProductQueryParams): Promise<ProductListResponse> {
    const page = params?.paging?.page ?? 1
    const pageSize = params?.paging?.pageSize ?? 10

    let result = mockProducts.map(mapToProductResponse)

    if (params?.filter?.filter) {
      const parsedFilter = JSON.parse(params.filter.filter)
      const filters = buildFilters<ProductResponse>(parsedFilter)
      result = filterByOperator(result, filters)
    }

    if (params?.search?.search && params.search.searchFields?.length) {
      const searchConfig = createSearchConfig<ProductResponse>(params.search.search, params.search.searchFields)
      result = searchByFields(result, searchConfig)
    }

    if (params?.sort?.sortBy) {
      const sortConfig = createSortConfig<ProductResponse>(params.sort.sortBy, params.sort.sortOrder ?? 'asc')
      result = sortByField(result, sortConfig)
    }

    const { items, meta } = paginateResults(result, page, pageSize)

    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      items,
      page: meta.page,
      pageSize: meta.pageSize,
      totalCount: meta.totalCount,
      totalPages: meta.totalPages,
      hasNextPage: meta.hasNextPage,
      hasPreviousPage: meta.hasPreviousPage,
    }
  }

  async getById(id: string): Promise<ProductSingleResponse> {
    const product = getProductById(id)
    if (!product) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Product not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToProductResponse(product) }
  }

  async getProductBySlug(slug: string): Promise<ProductSingleResponse> {
    const product = getProductBySlug(slug)
    if (!product) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Product not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mapToProductResponse(product) }
  }

  async searchProducts(query: string, limit = 10): Promise<ProductListResponse> {
    const results = searchProducts(query, limit).map(mapToProductResponse)
    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      items: results,
      page: 1,
      pageSize: limit,
      totalCount: results.length,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
  }

  async getFeaturedProducts(limit = 8): Promise<ProductListResponse> {
    const featured = getFeaturedProducts().slice(0, limit).map(mapToProductResponse)
    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      items: featured,
      page: 1,
      pageSize: limit,
      totalCount: featured.length,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
  }

  async getNewArrivals(limit = 8): Promise<ProductListResponse> {
    const newArrivals = getNewArrivals(limit).map(mapToProductResponse)
    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      items: newArrivals,
      page: 1,
      pageSize: limit,
      totalCount: newArrivals.length,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
  }

  async getProductsByCategory(categorySlug: string, params?: ProductQueryParams): Promise<ProductListResponse> {
    const filterWithCategory = {
      ...params,
      filter: { filter: JSON.stringify({ category: categorySlug }) }
    }
    return this.getAll(filterWithCategory)
  }

  async getProductsInStock(params?: ProductQueryParams): Promise<ProductListResponse> {
    const inStockFilter = {
      ...params,
      filter: { filter: JSON.stringify({ 'inventory.quantity': { operator: '>', value: 0 } }) }
    }
    return this.getAll(inStockFilter)
  }

  async getProductsByPriceRange(min: number, max: number, params?: ProductQueryParams): Promise<ProductListResponse> {
    const priceFilter = {
      ...params,
      filter: { filter: JSON.stringify({ price: min, 'price:max': max }) }
    }
    return this.getAll(priceFilter)
  }
}

export const mockProductRepository = new MockProductRepository()