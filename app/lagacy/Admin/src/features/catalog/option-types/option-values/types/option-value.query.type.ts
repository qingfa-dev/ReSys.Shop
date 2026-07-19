import type { ServerQueryingParameters } from '@/shared/api/types/query.types'
export interface OptionValueQuery extends ServerQueryingParameters {
  optionTypeId?: string
}
