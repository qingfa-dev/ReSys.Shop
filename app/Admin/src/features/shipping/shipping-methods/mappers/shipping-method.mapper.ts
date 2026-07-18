import type { ShippingMethodListItem, ShippingMethodDetail } from '../types/shipping-method.response.type'
import { toShippingMethodListItemModel, toShippingMethodDetailModel, type ShippingMethodListItemModel, type ShippingMethodDetailModel } from '../types/shipping-method.model.type'

export function mapShippingMethodListItem(dto: ShippingMethodListItem): ShippingMethodListItemModel {
  return toShippingMethodListItemModel(dto)
}

export function mapShippingMethodDetail(dto: ShippingMethodDetail): ShippingMethodDetailModel {
  return toShippingMethodDetailModel(dto)
}
