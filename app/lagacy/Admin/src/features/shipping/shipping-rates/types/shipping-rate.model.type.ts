import type { ShippingRateListItem, ShippingRateDetail } from './shipping-rate.response.type'
import { decimalToDisplay } from '@/shared/utils/currency'

export interface ShippingRateListItemModel extends ShippingRateListItem {
  costDisplay: string
}

export type ShippingRateDetailModel = ShippingRateListItemModel

export function toShippingRateListItemModel(dto: ShippingRateListItem): ShippingRateListItemModel {
  return {
    ...dto,
    costDisplay: decimalToDisplay(dto.cost, dto.currency),
  }
}

export function toShippingRateDetailModel(dto: ShippingRateDetail): ShippingRateDetailModel {
  return toShippingRateListItemModel(dto)
}
