import apiClient from "@/common/api/http/api.client";
import { CATALOG } from "@/common/api/constants";
import type { ServerResult } from "@/common/api/types/result.types";
import type { VariantImage } from "../models/image.response";
import type { UpdateVariantImageRequest } from "../types/image.request";
import { ImageMapper } from "./image.mapper";

export const imageApi = {
  listByVariant: async (variantId: string): Promise<ServerResult<VariantImage[]>> => {
    const res = await apiClient.get(`${CATALOG}/variants/${variantId}/images`);
    const result = res.data as ServerResult<VariantImage[]>;
    if (result.value) result.value = result.value.map(ImageMapper.toImage)
    return result;
  },

  upload: async (variantId: string, file: File, role?: number): Promise<ServerResult<VariantImage>> => {
    const formData = new FormData();
    formData.append("file", file);
    let url = `${CATALOG}/variants/${variantId}/images`;
    if (role !== undefined) url += `?role=${role}`;
    const res = await apiClient.post(url, formData, { headers: { "Content-Type": "multipart/form-data" } });
    const result = res.data as ServerResult<VariantImage>;
    if (result.value) result.value = ImageMapper.toImage(result.value)
    return result;
  },

  update: (imageId: string, data: UpdateVariantImageRequest): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/variants/images/${imageId}`, data).then((res) => res.data as ServerResult<void>),

  delete: (imageId: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/variants/images/${imageId}`).then((res) => res.data as ServerResult<void>),

  getById: async (imageId: string): Promise<ServerResult<VariantImage>> => {
    const res = await apiClient.get(`${CATALOG}/variants/images/${imageId}`);
    const result = res.data as ServerResult<VariantImage>;
    if (result.value) result.value = ImageMapper.toImage(result.value)
    return result;
  },

  download: (imageId: string): Promise<Blob> =>
    apiClient.get(`${CATALOG}/variants/images/${imageId}/download`, { responseType: 'blob' }).then((res) => res.data as Blob),

  generateEmbedding: (imageId: string): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/variants/images/${imageId}/embeddings`).then((res) => res.data as ServerResult<void>),
};
