import type { PriceRecord } from '../models/price.response'

export const PriceMapper = {
  toPriceRecord(dto: Record<string, unknown>): PriceRecord {
    return {
      id: String(dto.id ?? ''),
      amount: Number(dto.amount ?? 0),
      currency: String(dto.currency ?? ''),
    }
  },
}
