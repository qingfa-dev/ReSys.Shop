import type { TaxonomyResponse } from '../types'

export class TaxonomyResponseMapper {
  static fromApi(taxonomy: TaxonomyResponse) {
    return taxonomy
  }
}
