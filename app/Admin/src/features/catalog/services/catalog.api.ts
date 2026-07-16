import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ApiResult } from '@/shared/api/types/api.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query-params.types'
import type { ProductDetail, ProductSummary, CreateProductRequest, UpdateProductRequest } from '../products/types/product.types'
import type { VariantDetail, VariantSummary, CreateVariantRequest, UpdateVariantRequest } from '../products/types/variant.types'
import type { OptionTypeDetail, OptionTypeListItem } from '../option-types/types/option-type.types'
import type { OptionValueListItem } from '../option-types/option-values/types/option-value.types'
import type { PropertyTypeDetail } from '../property-types/types/property-type.types'
import type { TaxonomyDetail, TaxonomyListItem, CreateTaxonomyRequest, UpdateTaxonomyRequest } from '../taxonomies/types/taxonomy.types'
import type { TaxonDetail, TaxonListItem, TaxonTreeItem, CreateTaxonRequest, UpdateTaxonRequest, TaxonRuleListItem, CreateTaxonRuleRequest, UpdateTaxonRuleRequest } from '../taxonomies/taxa/types/taxon.types'

export const catalogApi = {
  products: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<ProductSummary[]>> {
      return apiClient.get(`${CATALOG}/products`, { params })
    },
    async getById(id: string): Promise<ApiResult<ProductDetail>> {
      return apiClient.get(`${CATALOG}/products/${id}`)
    },
    async create(data: CreateProductRequest): Promise<ApiResult<ProductDetail>> {
      return apiClient.post(`${CATALOG}/products`, data)
    },
    async update(id: string, data: UpdateProductRequest): Promise<ApiResult<ProductDetail>> {
      return apiClient.put(`${CATALOG}/products/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/${id}`)
    },
    async activate(id: string): Promise<ApiResult<void>> {
      return apiClient.patch(`${CATALOG}/products/${id}/activate`)
    },
    async discontinue(id: string): Promise<ApiResult<void>> {
      return apiClient.patch(`${CATALOG}/products/${id}/discontinue`)
    },
    async getOptionTypes(productId: string): Promise<ApiResult<OptionTypeDetail[]>> {
      return apiClient.get(`${CATALOG}/products/${productId}/option-types`)
    },
    async syncOptionTypes(productId: string, optionTypeIds: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/${productId}/option-types/sync`, { optionTypeIds })
    },
    async getClassifications(productId: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${CATALOG}/products/${productId}/classifications`)
    },
    async syncClassifications(productId: string, data: { taxonIds: string[]; mainTaxonId?: string }): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/${productId}/classifications/sync`, data)
    },
  },

  variants: {
    async getById(id: string): Promise<ApiResult<VariantDetail>> {
      return apiClient.get(`${CATALOG}/products/variants/${id}`)
    },
    async listByProductId(productId: string): Promise<ApiResult<VariantSummary[]>> {
      return apiClient.get(`${CATALOG}/products/${productId}/variants`)
    },
    async create(productId: string, data: CreateVariantRequest): Promise<ApiResult<VariantDetail>> {
      return apiClient.post(`${CATALOG}/products/${productId}/variants`, data)
    },
    async update(id: string, data: UpdateVariantRequest): Promise<ApiResult<VariantDetail>> {
      return apiClient.put(`${CATALOG}/products/variants/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/variants/${id}`)
    },
    async listPrices(variantId: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${CATALOG}/products/variants/${variantId}/prices`)
    },
    async setPrice(variantId: string, data: { amount: number; currency: string }): Promise<ApiResult<any>> {
      return apiClient.post(`${CATALOG}/products/variants/${variantId}/prices`, data)
    },
    async deletePrice(variantId: string, priceId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/variants/${variantId}/prices/${priceId}`)
    },
    async syncPrices(variantId: string, prices: Array<{ amount: number; currency: string }>): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/products/variants/${variantId}/prices/sync`, prices)
    },
    async syncOptionValues(variantId: string, optionValueIds: string[]): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/variants/${variantId}/option-values/sync`, { optionValueIds })
    },
    async listImages(variantId: string): Promise<ApiResult<any[]>> {
      return apiClient.get(`${CATALOG}/products/variants/${variantId}/images`)
    },
    async uploadImage(variantId: string, file: File, role?: number): Promise<ApiResult<any>> {
      const formData = new FormData()
      formData.append('file', file)
      let url = `${CATALOG}/products/variants/${variantId}/images`
      if (role !== undefined) url += `?role=${role}`
      return apiClient.post(url, formData, { headers: { 'Content-Type': 'multipart/form-data' } })
    },
    async deleteImage(imageId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/products/variants/images/${imageId}`)
    },
    async updateImage(imageId: string, data: { alt?: string; role?: number }): Promise<ApiResult<void>> {
      return apiClient.put(`${CATALOG}/products/variants/images/${imageId}`, data)
    },
  },

  optionTypes: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<OptionTypeListItem[]>> {
      return apiClient.get(`${CATALOG}/option-types`, { params })
    },
    async getById(id: string): Promise<ApiResult<OptionTypeDetail>> {
      return apiClient.get(`${CATALOG}/option-types/${id}`)
    },
    async create(data: { name: string; presentation: string; filterable?: boolean; position?: number }): Promise<ApiResult<OptionTypeDetail>> {
      return apiClient.post(`${CATALOG}/option-types`, data)
    },
    async update(id: string, data: Partial<{ name: string; presentation: string; filterable: boolean; position: number }>): Promise<ApiResult<OptionTypeDetail>> {
      return apiClient.put(`${CATALOG}/option-types/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/option-types/${id}`)
    },
    async listValues(optionTypeId: string, params?: ServerQueryingParameters): Promise<ApiResult<OptionValueListItem[]>> {
      return apiClient.get(`${CATALOG}/option-types/${optionTypeId}/values`, { params })
    },
    async createValue(optionTypeId: string, data: { name: string; presentation: string; position?: number }): Promise<ApiResult<OptionValueListItem>> {
      return apiClient.post(`${CATALOG}/option-types/${optionTypeId}/values`, data)
    },
    async updateValue(optionTypeId: string, valueId: string, data: { name?: string; presentation?: string; position?: number }): Promise<ApiResult<OptionValueListItem>> {
      return apiClient.put(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`, data)
    },
    async deleteValue(optionTypeId: string, valueId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/option-types/${optionTypeId}/values/${valueId}`)
    },
  },

  propertyTypes: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<PropertyTypeDetail[]>> {
      return apiClient.get(`${CATALOG}/property-types`, { params })
    },
    async getById(id: string): Promise<ApiResult<PropertyTypeDetail>> {
      return apiClient.get(`${CATALOG}/property-types/${id}`)
    },
    async create(data: { name: string; presentation: string; kind?: number; filterable?: boolean }): Promise<ApiResult<PropertyTypeDetail>> {
      return apiClient.post(`${CATALOG}/property-types`, data)
    },
    async update(id: string, data: Partial<{ name: string; presentation: string; kind: number; filterable: boolean }>): Promise<ApiResult<PropertyTypeDetail>> {
      return apiClient.put(`${CATALOG}/property-types/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/property-types/${id}`)
    },
  },

  taxonomies: {
    async list(params?: ServerQueryingParameters): Promise<ApiResult<TaxonomyListItem[]>> {
      return apiClient.get(`${CATALOG}/taxonomies`, { params })
    },
    async getById(id: string): Promise<ApiResult<TaxonomyDetail>> {
      return apiClient.get(`${CATALOG}/taxonomies/${id}`)
    },
    async create(data: CreateTaxonomyRequest): Promise<ApiResult<TaxonomyDetail>> {
      return apiClient.post(`${CATALOG}/taxonomies`, data)
    },
    async update(id: string, data: UpdateTaxonomyRequest): Promise<ApiResult<TaxonomyDetail>> {
      return apiClient.put(`${CATALOG}/taxonomies/${id}`, data)
    },
    async delete(id: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/taxonomies/${id}`)
    },
    async restore(id: string): Promise<ApiResult<void>> {
      return apiClient.patch(`${CATALOG}/taxonomies/${id}/restore`)
    },
    async listTaxons(taxonomyId: string, params?: ServerQueryingParameters & { includeLeavesOnly?: boolean }): Promise<ApiResult<TaxonListItem[]>> {
      return apiClient.get(`${CATALOG}/taxonomies/${taxonomyId}/taxons`, { params })
    },
    async getTaxonTree(taxonomyId: string): Promise<ApiResult<TaxonTreeItem[]>> {
      return apiClient.get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/tree`)
    },
    async getTaxonById(taxonomyId: string, taxonId: string): Promise<ApiResult<TaxonDetail>> {
      return apiClient.get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`)
    },
    async createTaxon(taxonomyId: string, data: CreateTaxonRequest): Promise<ApiResult<TaxonDetail>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons`, data)
    },
    async updateTaxon(taxonomyId: string, taxonId: string, data: UpdateTaxonRequest): Promise<ApiResult<TaxonDetail>> {
      return apiClient.put(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`, data)
    },
    async deleteTaxon(taxonomyId: string, taxonId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}`)
    },
    async repositionTaxon(taxonomyId: string, taxonId: string, position: number): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/reposition`, { position })
    },
    async restoreTaxon(taxonomyId: string, taxonId: string): Promise<ApiResult<void>> {
      return apiClient.patch(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/restore`)
    },
    async listTaxonRules(taxonomyId: string, taxonId: string): Promise<ApiResult<TaxonRuleListItem[]>> {
      return apiClient.get(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`)
    },
    async createTaxonRule(taxonomyId: string, taxonId: string, data: CreateTaxonRuleRequest): Promise<ApiResult<TaxonRuleListItem>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules`, data)
    },
    async updateTaxonRule(taxonomyId: string, taxonId: string, ruleId: string, data: UpdateTaxonRuleRequest): Promise<ApiResult<TaxonRuleListItem>> {
      return apiClient.put(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/${ruleId}`, data)
    },
    async deleteTaxonRule(taxonomyId: string, taxonId: string, ruleId: string): Promise<ApiResult<void>> {
      return apiClient.delete(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/${ruleId}`)
    },
    async syncTaxonRules(taxonomyId: string, taxonId: string, rules: CreateTaxonRuleRequest[]): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/sync`, rules)
    },
    async regenerateTaxonProducts(taxonomyId: string, taxonId: string): Promise<ApiResult<void>> {
      return apiClient.post(`${CATALOG}/taxonomies/${taxonomyId}/taxons/${taxonId}/rules/regenerate`)
    },
  },
}
