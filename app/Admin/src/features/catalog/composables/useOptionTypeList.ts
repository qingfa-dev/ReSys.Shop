import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { OptionTypeApi } from '../services/optionTypeApi'
import type { OptionTypeListItem } from '../types/optionType'

export function useOptionTypeList(options?: UsePagedQueryOptions) {
  return usePagedQuery<OptionTypeListItem>((params) => OptionTypeApi.getOptionTypes(params), {
    ...options,
  })
}
