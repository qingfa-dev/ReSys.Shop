import type { Result, PagedResult } from '@/core/models/result'
import type { Country, ILocationReferenceRepository, State } from './ILocationReferenceRepository'
import { mockCountries, mockStates } from './mock-location-reference.data'

function paginate<T>(items: T[], page: number, pageSize: number): PagedResult<T> {
  const totalCount = items.length
  const totalPages = Math.ceil(totalCount / pageSize)
  const start = (page - 1) * pageSize
  const paged = items.slice(start, start + pageSize)
  return {
    isSuccess: true,
    isFailure: false,
    statusCode: 200,
    items: paged,
    page,
    pageSize,
    totalCount,
    totalPages,
    hasNextPage: page < totalPages,
    hasPreviousPage: page > 1,
  }
}

export class MockLocationReferenceRepository implements ILocationReferenceRepository {
  async getCountries(page = 1, pageSize = 50): Promise<PagedResult<Country>> {
    return paginate(mockCountries, page, pageSize)
  }

  async getCountryById(id: string): Promise<Result<Country>> {
    const country = mockCountries.find(c => c.id === id)
    if (!country) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Country not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: country }
  }

  async getCountryByIso(isoCode: string): Promise<Result<Country>> {
    const country = mockCountries.find(c => c.isoCode === isoCode)
    if (!country) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Country not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: country }
  }

  async getStates(countryId?: string, page = 1, pageSize = 50): Promise<PagedResult<State>> {
    const filtered = countryId ? mockStates.filter(s => s.countryId === countryId) : mockStates
    return paginate(filtered, page, pageSize)
  }
}

export const mockLocationReferenceRepository = new MockLocationReferenceRepository()
