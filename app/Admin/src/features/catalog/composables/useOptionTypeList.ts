import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { OPTION_TYPE_FILTER_FIELDS, OPTION_TYPE_SORT_FIELDS } from '../types/optionType'
import type { OptionTypeListItem } from '../types/optionType'

export function useOptionTypeList(options?: UsePagedQueryOptions) {
  return usePagedQuery<OptionTypeListItem>(`api/admin/catalog/option-types`, {
    allowedFilterFields: OPTION_TYPE_FILTER_FIELDS,
    allowedSortFields: OPTION_TYPE_SORT_FIELDS,
    ...options,
  })
}
