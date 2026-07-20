import type { ServerQueryingParameters } from '@/common/api/types/query.types'
export interface OptionValueQuery extends ServerQueryingParameters {
  optionTypeId?: string
}
