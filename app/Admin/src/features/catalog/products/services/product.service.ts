import { productRepository } from "../api/product.api";
import { productOptionTypeApi } from "../option-types/api/product-option-type.api";
import { productClassificationApi } from "../classifications/api/product-classification.api";
import type { ServerResult, ServerPagedResult } from "@/common/api/types/result.types";
import type { OptionTypeDetail } from "../../option-types/types/option-type.response.type";
import type { ProductClassification } from "../classifications/types/classification.response.type";
import type { ProductImage } from "../types/product-image.response.type";
import type { SyncClassificationsRequest } from "../classifications/types/classification.request.type";

export const productService = {
  list: productRepository.list,
  getById: productRepository.getById,
  create: productRepository.create,
  update: productRepository.update,

  delete: productRepository.delete,
  activate: productRepository.activate,
  discontinue: productRepository.discontinue,

  getOptionTypes: (productId: string): Promise<ServerResult<OptionTypeDetail[]>> =>
    productOptionTypeApi.getOptionTypes(productId),

  updateOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    productOptionTypeApi.syncOptionTypes(productId, optionTypeIds),

  getClassifications: (productId: string): Promise<ServerResult<ProductClassification[]>> =>
    productClassificationApi.getClassifications(productId),

  syncClassifications: (
    productId: string,
    data: SyncClassificationsRequest,
  ): Promise<ServerResult<void>> => productClassificationApi.syncClassifications(productId, data),

  async getImages(_productId: string): Promise<ServerPagedResult<ProductImage>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 0, totalCount: 0 }
  },
  uploadImage: async (_productId: string, _file: File, _role?: number, _alt?: string): Promise<ServerResult<ProductImage>> => ({
    isSuccess: true,
    statusCode: 200,
    errors: [],
    message: null,
    metadata: null,
    value: {} as ProductImage,
  }),
  deleteImage: async (_imageId: string): Promise<ServerResult<void>> => {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }
  },
  updateImage: async (_imageId: string, _data: { alt?: string; role?: number }): Promise<ServerResult<void>> => {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }
  },
};
