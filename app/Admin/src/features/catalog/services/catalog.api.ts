import { createModuleApi, apiClient } from '@/shared/api'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { ProductDetail, ProductSummary, CreateProductRequest, UpdateProductRequest } from '../products/types/product.types'
import type { VariantDetail, VariantSummary, CreateVariantRequest, UpdateVariantRequest } from '../products/types/variant.types'
import type { OptionTypeDetail } from '../option-types/types/option-type.types'
import type { OptionValueListItem, UpdateOptionValuePositionsRequest } from '../option-types/option-values/types/option-value.types'
import type { PropertyTypeDetail } from '../property-types/types/property-type.types'
import type { TaxonomyDetail, TaxonomyListItem } from '../taxonomies/types/taxonomy.types'
import type { TaxonDetail, TaxonListItem, CreateTaxonRequest, UpdateTaxonRequest, TaxonRuleListItem, CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from '../taxonomies/taxa/types/taxon.types'
import type { CatalogSummary } from '../dashboard/types/catalog-dashboard.types'
import { CATALOG } from '@/shared/api/constants'

const catalog = createModuleApi<ProductDetail>({ basePath: CATALOG })

export const catalogApi = {
  products: {
    ...catalog,
    async list(params?: ServerQueryingParameters): Promise<ApiResult<ProductSummary[]>> {
      return apiClient.get(`${CATALOG}/products`, { params })
    },
    async getById(id: string): Promise<ApiResult<ProductDetail>> {
      return apiClient.get(`${CATALOG}/products/${id}`)
    },
    async create(data: CreateProductRequest): Promise<ApiResult<ProductDetail>> {
      const payload = { ...data, presentation: (data as any).presentation || data.name }
      return apiClient.post(`${CATALOG}/products`, payload)
    },
    async update(id: string, data: UpdateProductRequest): Promise<ApiResult<ProductDetail>> {
      return apiClient.put(`${CATALOG}/products/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/${id}`)
    },

    getOptionTypes(productId: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${CATALOG}/products/option-types`, { params: { productId } })
    },
    updateOptionTypes(productId: string, optionTypeIds: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/option-types`, { productId, optionTypeIds })
    },
    getProperties(productId: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${CATALOG}/products/properties`, { params: { productId } })
    },
    updateProperties(productId: string, properties: any[]): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/properties`, { productId, properties })
    },
    getImages(productId: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${CATALOG}/products/images`, { params: { productId } })
    },
    uploadImage(productId: string, file: File, role: number, alt?: string): Promise<ApiResult<any>> {
      const formData = new FormData()
      formData.append('file', file)
      let url = `${CATALOG}/products/images?productId=${productId}&role=${role}`
      if (alt) url += `&alt=${encodeURIComponent(alt)}`
      return apiClient.post(url, formData, { headers: { 'Content-Type': 'multipart/form-data' } })
    },
    updateImage(productId: string, imageId: string, role: number, alt: string): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/images/${imageId}?productId=${productId}`, { role, alt, position: 0 })
    },
    deleteImage(productId: string, imageId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/images/${imageId}`, { params: { productId } })
    },
  },

  variants: {
    ...createModuleApi<VariantDetail>({ basePath: `${CATALOG}/variants` }),
    async listByProductId(productId: string): Promise<ApiResult<VariantSummary[]>> {
      return apiClient.get(`${CATALOG}/products/${productId}/variants`) as unknown as Promise<ApiResult<VariantSummary[]>>
    },
    async create(productId: string, data: CreateVariantRequest): Promise<ApiResult<VariantDetail>> {
      return apiClient.post(`${CATALOG}/products/${productId}/variants`, data) as unknown as Promise<ApiResult<VariantDetail>>
    },
    async setMaster(id: string): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/variants/${id}/set-master`) as unknown as Promise<ApiResult<void>>
    },
    async updateOptionValues(id: string, optionValueIds: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/variants/${id}/option-values`, { optionValueIds }) as unknown as Promise<ApiResult<void>>
    },
  },

  optionTypes: createModuleApi<OptionTypeDetail>({ basePath: `${CATALOG}/option-types` }),

  optionValues: {
    ...createModuleApi<OptionValueListItem>({ basePath: `${CATALOG}/option-values` }),
    async reorder(data: UpdateOptionValuePositionsRequest): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/option-values/positions`, data)
    },
  },

  propertyTypes: createModuleApi<PropertyTypeDetail>({ basePath: `${CATALOG}/property-types` }),

  taxonomies: {
    ...createModuleApi<TaxonomyDetail>({ basePath: `${CATALOG}/taxonomies` }),
    async list(params?: ServerQueryingParameters): Promise<ApiResult<TaxonomyListItem[]>> {
      return apiClient.get(`${CATALOG}/taxonomies`, { params })
    },
  },

  taxons: {
    async getTaxons(params?: ServerQueryingParameters): Promise<ApiResult<TaxonListItem[]>> {
      return apiClient.get(`${CATALOG}/taxons`, { params })
    },
    async getTree(params?: ServerQueryingParameters): Promise<ApiResult<any>> {
      return apiClient.get(`${CATALOG}/taxons/tree`, { params })
    },
    async getById(taxonId: string): Promise<ApiResult<TaxonDetail>> {
      return apiClient.get(`${CATALOG}/taxons/${taxonId}`)
    },
    async create(request: CreateTaxonRequest): Promise<ApiResult<TaxonDetail>> {
      return apiClient.post(`${CATALOG}/taxons`, request)
    },
    async update(taxonId: string, request: UpdateTaxonRequest): Promise<ApiResult<TaxonDetail>> {
      return apiClient.put(`${CATALOG}/taxons/${taxonId}`, request)
    },
    async delete(taxonId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/taxons/${taxonId}`)
    },
    async getRules(taxonId: string): Promise<ApiResult<TaxonRuleListItem[]>> {
      return apiClient.get(`${CATALOG}/taxons/${taxonId}/rules`)
    },
    async addRule(taxonId: string, request: CreateTaxonRuleRequest): Promise<ApiResult<TaxonRuleListItem>> {
      return apiClient.post(`${CATALOG}/taxons/${taxonId}/rules`, request)
    },
    async updateRule(taxonId: string, ruleId: string, request: UpdateTaxonRuleRequest): Promise<ApiResult<TaxonRuleListItem>> {
      return apiClient.put(`${CATALOG}/taxons/${taxonId}/rules/${ruleId}`, request)
    },
    async deleteRule(taxonId: string, ruleId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/taxons/${taxonId}/rules/${ruleId}`)
    },
    async regenerateProducts(taxonId: string): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/taxons/${taxonId}/rules/regenerate`, {})
    },
    async getProductPreview(taxonId: string, params?: { page?: number, pageSize?: number }): Promise<ApiResult<any>> {
      return apiClient.get(`${CATALOG}/taxons/${taxonId}/preview`, { params })
    },
  },

  dashboard: {
    async getSummary(): Promise<ApiResult<CatalogSummary>> {
      return apiClient.get('/admin/dashboard/catalog-summary')
    },
  },
}
