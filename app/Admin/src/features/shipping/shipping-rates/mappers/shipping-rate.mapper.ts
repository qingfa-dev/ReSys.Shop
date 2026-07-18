import type { ShippingRateListItem, ShippingRateDetail } from '../types/shipping-rate.response.type'
import { toShippingRateListItemModel, toShippingRateDetailModel, type ShippingRateListItemModel, type ShippingRateDetailModel } from '../types/shipping-rate.model.type'

export function mapShippingRateListItem(dto: ShippingRateListItem): ShippingRateListItemModel {
  return toShippingRateListItemModel(dto)
}

export function mapShippingRateDetail(dto: ShippingRateDetail): ShippingRateDetailModel {
  return toShippingRateDetailModel(dto)
}
