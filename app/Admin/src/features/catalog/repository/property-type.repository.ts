import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
import type { PropertyTypeDetail } from '../property-types/types/PropertyType.Response.Type'
import type { CreatePropertyTypeRequest, UpdatePropertyTypeRequest } from '../property-types/types/PropertyType.Request.Type'

export const propertyTypeRepository = {
  list: (params?: ServerQueryingParameters): Promise<ServerResult<PropertyTypeDetail[]>> =>
    apiClient.get(`${CATALOG}/property-types`, { params }).then(res => res.data as ServerResult<PropertyTypeDetail[]>),

  getById: (id: string): Promise<ServerResult<PropertyTypeDetail>> =>
    apiClient.get(`${CATALOG}/property-types/${id}`).then(res => res.data as ServerResult<PropertyTypeDetail>),

  create: (data: CreatePropertyTypeRequest): Promise<ServerResult<PropertyTypeDetail>> =>
    apiClient.post(`${CATALOG}/property-types`, data).then(res => res.data as ServerResult<PropertyTypeDetail>),

  update: (id: string, data: UpdatePropertyTypeRequest): Promise<ServerResult<PropertyTypeDetail>> =>
    apiClient.put(`${CATALOG}/property-types/${id}`, data).then(res => res.data as ServerResult<PropertyTypeDetail>),

  delete: (id: string): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/property-types/${id}`).then(res => res.data as ServerResult<void>),
}
