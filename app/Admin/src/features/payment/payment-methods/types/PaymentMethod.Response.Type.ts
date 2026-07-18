export interface PaymentMethodListItem {
  id: string
  name: string
  description: string | null
  provider: string
  isActive: boolean
  displayOrder: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface PaymentMethodDetail extends PaymentMethodListItem {
  configuration: Record<string, unknown> | null
}
