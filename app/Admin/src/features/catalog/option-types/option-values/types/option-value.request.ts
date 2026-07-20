import type { OptionValueParameters } from '../types/option-value.field'
export type CreateOptionValueRequest = OptionValueParameters & { optionTypeId: string }
export type UpdateOptionValueRequest = OptionValueParameters & { optionTypeId?: string }
export interface UpdateOptionValuePositionsRequest {
  optionTypeId: string
  positions: { id: string; position: number }[]
}
