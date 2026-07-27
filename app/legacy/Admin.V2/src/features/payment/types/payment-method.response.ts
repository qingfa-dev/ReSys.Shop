export interface PaymentMethodResponse {
  id: string
  name: string
  code: string
  description?: string | null
  isActive: boolean
  isTestMode?: boolean
  displayOrder: number
  supportedCurrencies?: string | null
  createdAt: string
  updatedAt: string
}
