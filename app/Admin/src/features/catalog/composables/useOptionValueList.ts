import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { OPTION_VALUE_FILTER_FIELDS, OPTION_VALUE_SORT_FIELDS } from '../types/optionValue'
import type { OptionValueListItem } from '../types/optionValue'

export function useOptionValueList(options?: UsePagedQueryOptions) {
  return usePagedQuery<OptionValueListItem>(`api/admin/catalog/option-values`, {
    allowedFilterFields: OPTION_VALUE_FILTER_FIELDS,
    allowedSortFields: OPTION_VALUE_SORT_FIELDS,
    ...options,
  })
}
