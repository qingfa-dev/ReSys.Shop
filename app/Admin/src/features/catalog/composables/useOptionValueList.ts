import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { OptionValueApi } from '../services/optionValueApi'
import type { OptionValueListItem } from '../types/optionValue'

export function useOptionValueList(options?: UsePagedQueryOptions) {
  return usePagedQuery<OptionValueListItem>((params) => OptionValueApi.getOptionValues(params), {
    ...options,
  })
}
