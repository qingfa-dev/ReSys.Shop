import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useActiveCountries } from '../useActiveCountries'
import { CountryApi } from '../../services/countryApi'
import type { PagedResult } from '@/shared/types/result'
import type { CountryListItem } from '../../types/country'

vi.mock('../../services/countryApi', () => ({
  CountryApi: { getCountries: vi.fn<() => Promise<PagedResult<CountryListItem>>>() },
}))

const mockGetCountries = vi.mocked(CountryApi.getCountries)

function okResult(items: CountryListItem[] = [{ id: 'us', name: 'United States', isoCode: 'US', callingCode: '+1', statesRequired: true, isActive: true }]): PagedResult<CountryListItem> {
  return { isSuccess: true, statusCode: 200, items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1, errors: [], message: null, metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useActiveCountries', () => {
  it('loads active countries via the CountryApi', async () => {
    mockGetCountries.mockResolvedValue(okResult())
    const { items, load } = useActiveCountries()

    await load()

    expect(mockGetCountries).toHaveBeenCalledWith({ isActive: true })
    expect(items.value).toHaveLength(1)
    expect(items.value[0]!.name).toBe('United States')
  })
})
