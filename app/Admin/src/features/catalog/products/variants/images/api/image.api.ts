import apiClient from "@/shared/api/http/api.client";
import { CATALOG } from "@/shared/api/constants";
import type { ServerResult } from "@/shared/api/types/result.types";
import type { VariantImage } from "../types/image.response.type";
import type { UpdateVariantImageRequest } from "../types/image.request.type";

export const imageApi = {
  listByVariant: (variantId: string): Promise<ServerResult<VariantImage[]>> =>
    apiClient
      .get(`${CATALOG}/variants/${variantId}/images`)
      .then((res) => res.data as ServerResult<VariantImage[]>),

  upload: (variantId: string, file: File, role?: number): Promise<ServerResult<VariantImage>> => {
    const formData = new FormData();
    formData.append("file", file);
    let url = `${CATALOG}/variants/${variantId}/images`;
    if (role !== undefined) url += `?role=${role}`;
    return apiClient
      .post(url, formData, { headers: { "Content-Type": "multipart/form-data" } })
      .then((res) => res.data as ServerResult<VariantImage>);
  },

  update: (imageId: string, data: UpdateVariantImageRequest): Promise<ServerResult<void>> =>
    apiClient
      .put(`${CATALOG}/variants/images/${imageId}`, data)
      .then((res) => res.data as ServerResult<void>),

  delete: (imageId: string): Promise<ServerResult<void>> =>
    apiClient
      .delete(`${CATALOG}/variants/images/${imageId}`)
      .then((res) => res.data as ServerResult<void>),
};
