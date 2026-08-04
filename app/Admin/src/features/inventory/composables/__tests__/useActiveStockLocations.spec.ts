import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useActiveStockLocations } from '../useActiveStockLocations'
import { StockLocationApi } from '../../services/stockLocationApi'
import type { PagedResult } from '@/shared/types/result'
import type { StockLocationListItem } from '../../types/stockLocation'

vi.mock('../../services/stockLocationApi', () => ({
  StockLocationApi: { getStockLocations: vi.fn<() => Promise<PagedResult<StockLocationListItem>>>() },
}))

const mockGetStockLocations = vi.mocked(StockLocationApi.getStockLocations)

function okResult(items: StockLocationListItem[] = [{ id: 'loc1', name: 'Warehouse A', code: 'WH-A', active: true, default: false, backorderableDefault: false, propagateAllVariants: false, position: 1, createdAtUtc: '2026-01-01T00:00:00Z' }]): PagedResult<StockLocationListItem> {
  return { isSuccess: true, statusCode: 200, items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useActiveStockLocations', () => {
  it('loads all stock locations sorted by name', async () => {
    mockGetStockLocations.mockResolvedValue(okResult())
    const { items, load } = useActiveStockLocations()

    await load()

    expect(mockGetStockLocations).toHaveBeenCalledWith({ pageSize: 100, sortBy: 'name', sortDirection: 'asc' })
    expect(items.value).toHaveLength(1)
  })
})
