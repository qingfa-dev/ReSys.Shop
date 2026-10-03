import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerPagedResult, ServerResult } from "@/common/api/types/result.types";
import type { ServerQueryingParameters } from "@/common/api/types/query.types";
import type { ProductDetail, ProductSummary } from "../models/product.response";
import type { CreateProductRequest, UpdateProductRequest } from "../models/product.request";
import type { ProductSummaryModel, ProductDetailModel } from "../models/product.model";
import type { ProductImage } from "../types/product-image.response";
import type { ProductClassification } from '../../classifications/models/classification.response';
import type { SyncClassificationsRequest } from '../../classifications/models/classification.request';
import { productOptionTypeApi } from '../../product-option-types/api/product-option-type.api';
import type { ProductOptionTypeItem } from '../../product-option-types/models/product-option-type.response';
import { productClassificationApi } from '../../classifications/api/product-classification.api';
import { ProductMapper } from "./product.mapper";

export const productRepository = {
  list: async (params?: ServerQueryingParameters): Promise<ServerPagedResult<ProductSummaryModel>> => {
    const res = await apiClient.get(`${CATALOG}/products`, { params });
    const result = res.data as ServerPagedResult<ProductSummary>;
    return { ...result, items: result.items?.map(ProductMapper.toSummaryModel) ?? [] };
  },

  getById: async (id: string): Promise<ServerResult<ProductDetailModel>> => {
    const res = await apiClient.get(`${CATALOG}/products/${id}`);
    const result = res.data as ServerResult<ProductDetail>;
    if (result.value) result.value = ProductMapper.toDetailModel(result.value)
    return result;
  },

  create: async (data: CreateProductRequest): Promise<ServerResult<ProductDetailModel>> => {
    const res = await apiClient.post(`${CATALOG}/products`, data);
    const result = res.data as ServerResult<ProductDetail>;
    if (result.value) result.value = ProductMapper.toDetailModel(result.value)
    return result;
  },

  update: async (id: string, data: UpdateProductRequest): Promise<ServerResult<ProductDetailModel>> => {
    const res = await apiClient.put(`${CATALOG}/products/${id}`, data);
    const result = res.data as ServerResult<ProductDetail>;
    if (result.value) result.value = ProductMapper.toDetailModel(result.value)
    return result;
  },

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${id}`).then((res) => res.data as ServerResult<void>),

  activate: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/activate`).then((res) => res.data as ServerResult<void>),

  discontinue: (id: string): Promise<ServerResult<void>> =>
    apiClient.patch(`${CATALOG}/products/${id}/discontinue`).then((res) => res.data as ServerResult<void>),

  getOptionTypes: (productId: string): Promise<ServerResult<ProductOptionTypeItem[]>> =>
    productOptionTypeApi.getOptionTypes(productId),

  updateOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    productOptionTypeApi.syncOptionTypes(productId, optionTypeIds),

  getClassifications: (productId: string): Promise<ServerResult<ProductClassification[]>> =>
    productClassificationApi.getClassifications(productId),

  syncClassifications: (productId: string, data: SyncClassificationsRequest): Promise<ServerResult<void>> =>
    productClassificationApi.syncClassifications(productId, data),

  async getImages(_productId: string): Promise<ServerPagedResult<ProductImage>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 0, totalCount: 0 };
  },

  uploadImage: async (_productId: string, _file: File, _role?: number, _alt?: string): Promise<ServerResult<ProductImage>> => ({
    isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: {} as ProductImage,
  }),

  deleteImage: async (_imageId: string): Promise<ServerResult<void>> =>
    ({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }),

  updateImage: async (_imageId: string, _data: { alt?: string; role?: number }): Promise<ServerResult<void>> =>
    ({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }),
};
