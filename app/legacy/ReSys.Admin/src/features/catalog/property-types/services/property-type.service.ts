import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';
import type { 
  PropertyTypeListItem, 
  PropertyTypeDetail, 
  CreatePropertyTypeRequest, 
  UpdatePropertyTypeRequest, 
  PropertyTypeQuery 
} from '../types/property-type.types';

const BASE_URL = '/admin/catalog/property-types';

export const propertyTypeService = {
  async getList(query?: PropertyTypeQuery): Promise<ApiResult<PropertyTypeListItem[]>> {
    return await apiClient.get<PropertyTypeListItem[]>(BASE_URL, { params: query }) as any;
  },

  async getById(id: string): Promise<ApiResult<PropertyTypeDetail>> {
    return await apiClient.get<PropertyTypeDetail>(`${BASE_URL}/${id}`) as any;
  },

  async create(request: CreatePropertyTypeRequest): Promise<ApiResult<PropertyTypeDetail>> {
    return await apiClient.post<PropertyTypeDetail>(BASE_URL, request) as any;
  },

  async update(id: string, request: UpdatePropertyTypeRequest): Promise<ApiResult<PropertyTypeDetail>> {
    return await apiClient.put<PropertyTypeDetail>(`${BASE_URL}/${id}`, request) as any;
  },

  async delete(id: string): Promise<ApiResult<void>> {
    return await apiClient.delete<void>(`${BASE_URL}/${id}`) as any;
  }
};
