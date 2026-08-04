import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'
import { CATALOG } from '@/shared/constants/api'
import { OPTION_TYPE_FILTER_FIELDS, OPTION_TYPE_SORT_FIELDS } from '../types/optionType'
import type { OptionTypeListItem } from '../types/optionType'

export function useOptionTypeList(options?: UsePagedQueryOptions) {
  return usePagedQuery<OptionTypeListItem>(`${CATALOG}/option-types`, {
    allowedFilterFields: OPTION_TYPE_FILTER_FIELDS,
    allowedSortFields: OPTION_TYPE_SORT_FIELDS,
    ...options,
  })
}
