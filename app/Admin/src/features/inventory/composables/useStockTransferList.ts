import { usePagedQuery } from '@/shared/composables'
import type { UsePagedQueryOptions } from '@/shared/composables'

import { StockTransferApi } from '../services/stockTransferApi'
import type { StockTransferListItem } from '../types/stockTransfer'

export function useStockTransferList(options?: UsePagedQueryOptions) {
  return usePagedQuery<StockTransferListItem>((params) => StockTransferApi.getStockTransfers(params), {
    ...options,
  })
}