import type { Result, PagedResult } from '@/core/models/result'

export interface Country {
  id: string
  name: string
  isoCode: string
}

export interface State {
  id: string
  name: string
  isoCode: string
  countryId: string
}

export interface ILocationReferenceRepository {
  getCountries(page?: number, pageSize?: number): Promise<PagedResult<Country>>
  getCountryById(id: string): Promise<Result<Country>>
  getCountryByIso(isoCode: string): Promise<Result<Country>>
  getStates(countryId?: string, page?: number, pageSize?: number): Promise<PagedResult<State>>
}
