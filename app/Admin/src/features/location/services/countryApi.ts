import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { LOCATION } from '@/shared/constants/api'
import type { Result, PagedResult } from '@/shared/types'
import type {
  CountryRequest,
  CountryListItem,
  CountryDetail,
  CountryQuery,
} from '../types/country'
import {
  toCountryQueryParams,
  COUNTRY_FILTER_FIELDS,
  COUNTRY_SORT_FIELDS,
} from '../types/country'

export class CountryApi {
  private static readonly BASE = `${LOCATION}/countries`

  static getCountries(query: CountryQuery): Promise<PagedResult<CountryListItem>> {
    return getPaged<CountryListItem>(CountryApi.BASE, toCountryQueryParams(query), {
      allowedFilterFields: COUNTRY_FILTER_FIELDS,
      allowedSortFields: COUNTRY_SORT_FIELDS,
    })
  }

  static getCountry(id: string): Promise<Result<CountryDetail>> {
    return get<Result<CountryDetail>>(`${CountryApi.BASE}/${id}`)
  }

  static getCountryByIso(isoCode: string): Promise<Result<CountryDetail>> {
    return get<Result<CountryDetail>>(`${CountryApi.BASE}/by-iso/${isoCode}`)
  }

  static createCountry(request: CountryRequest): Promise<Result<CountryDetail>> {
    return post<Result<CountryDetail>>(CountryApi.BASE, request)
  }

  static updateCountry(id: string, request: CountryRequest): Promise<Result<CountryDetail>> {
    return put<Result<CountryDetail>>(`${CountryApi.BASE}/${id}`, request)
  }

  static deleteCountry(id: string): Promise<Result<CountryListItem>> {
    return del<Result<CountryListItem>>(`${CountryApi.BASE}/${id}`)
  }
}
