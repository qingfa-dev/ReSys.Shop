import type { OptionValueParameters } from '../schemas/option-value.schema'
export type CreateOptionValueRequest = OptionValueParameters & { optionTypeId: string }
export type UpdateOptionValueRequest = OptionValueParameters & { optionTypeId?: string }
export interface UpdateOptionValuePositionsRequest {
  optionTypeId: string
  positions: { id: string; position: number }[]
}
