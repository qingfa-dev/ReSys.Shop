export interface PaymentMethodListItem {
  id: string
  name: string
  code: string
  isActive: boolean
  position: number
  description: string | null
}

export type PaymentMethodDetail = PaymentMethodListItem
