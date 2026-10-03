import type { ShippingMethodListItem, ShippingMethodDetail } from './shipping-method.response'

export interface ShippingMethodListItemModel extends ShippingMethodListItem {
  statusLabel: string
}

export type ShippingMethodDetailModel = ShippingMethodListItemModel

export function toShippingMethodListItemModel(dto: ShippingMethodListItem): ShippingMethodListItemModel {
  return {
    ...dto,
    statusLabel: dto.isActive ? 'Active' : 'Inactive',
  }
}

export function toShippingMethodDetailModel(dto: ShippingMethodDetail): ShippingMethodDetailModel {
  return toShippingMethodListItemModel(dto)
}
