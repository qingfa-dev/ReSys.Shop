import { post, get, put, del } from '@/shared/api/client'
import { getPaged } from '@/shared/api'

import type { Result, PagedResult, QueryingParameters } from '@/shared/types'
import type {
  CountryRequest,
  CountryListItem,
  CountryDetail,
} from '../types/country'
import {
  COUNTRY_FILTER_FIELDS,
  COUNTRY_SORT_FIELDS,
} from '../types/country'

export class CountryApi {
  static getCountries(params: QueryingParameters): Promise<PagedResult<CountryListItem>> {
    return getPaged<CountryListItem>('/api/admin/location/countries', params, {
      allowedFilterFields: COUNTRY_FILTER_FIELDS,
      allowedSortFields: COUNTRY_SORT_FIELDS,
      allowedSearchFields: COUNTRY_FILTER_FIELDS,
    })
  }

  static getCountry(id: string): Promise<Result<CountryDetail>> {
    return get<Result<CountryDetail>>(`/api/admin/location/countries/${id}`)
  }

  static getCountryByIso(isoCode: string): Promise<Result<CountryDetail>> {
    return get<Result<CountryDetail>>(`/api/admin/location/countries/by-iso/${isoCode}`)
  }

  static createCountry(request: CountryRequest): Promise<Result<CountryDetail>> {
    return post<Result<CountryDetail>>('/api/admin/location/countries', request)
  }

  static updateCountry(id: string, request: CountryRequest): Promise<Result<CountryDetail>> {
    return put<Result<CountryDetail>>(`/api/admin/location/countries/${id}`, request)
  }

  static deleteCountry(id: string): Promise<Result<CountryListItem>> {
    return del<Result<CountryListItem>>(`/api/admin/location/countries/${id}`)
  }
}
