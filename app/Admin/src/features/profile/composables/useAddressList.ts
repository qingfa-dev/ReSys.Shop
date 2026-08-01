import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { PROFILE } from '@/shared/constants/api'
import { ADDRESS_FILTER_FIELDS, ADDRESS_SORT_FIELDS, ADDRESS_SEARCH_FIELDS } from '../types/address'
import type { AddressResponse } from '../types/address'

export function useAddressList(userId: string, options?: UsePagedQueryOptions) {
  return usePagedQuery<AddressResponse>(() => `${PROFILE}/addresses?userId=${userId}`, {
    allowedFilterFields: ADDRESS_FILTER_FIELDS,
    allowedSortFields: ADDRESS_SORT_FIELDS,
    allowedSearchFields: ADDRESS_SEARCH_FIELDS,
    ...options,
  })
}
