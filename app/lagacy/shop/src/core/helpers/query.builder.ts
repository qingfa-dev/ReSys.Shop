import type { NestedKeyOf, FilterOperator } from '../models/filter.model'

export class QueryBuilder<T extends object = Record<string, unknown>> {
  private _filterParts: string[] = []
  private _sorts: string[] = []
  private _searchText?: string
  private _searchFields: string[] = []
  private _page?: number
  private _pageSize?: number
  private _mappings: Map<string, string> = new Map()

  addMap(from: string, to: NestedKeyOf<T> | string): this {
    this._mappings.set(from, to as string)
    return this
  }

  where(field: NestedKeyOf<T> | string, operator: FilterOperator, value: unknown): this {
    if (value === undefined || value === '') return this

    this.appendSeparator()
    const mappedField = this._mappings.get(field as string) || field
    this._filterParts.push(`${mappedField}${operator}${this.formatValue(value)}`)
    return this
  }

  or(): this {
    if (this._filterParts.length > 0) {
      this._filterParts.push('|')
    }
    return this
  }

  startGroup(): this {
    this.appendSeparator()
    this._filterParts.push('(')
    return this
  }

  endGroup(): this {
    this._filterParts.push(')')
    return this
  }

  addRaw(filter: string): this {
    if (filter) {
      this.appendSeparator()
      this._filterParts.push(filter)
    }
    return this
  }

  orderBy(field: NestedKeyOf<T> | string, direction: 'asc' | 'desc' = 'asc'): this {
    const mappedField = this._mappings.get(field as string) || field
    if (direction === 'desc') {
      this._sorts.push(`${mappedField} desc`)
    } else {
      this._sorts.push(mappedField as string)
    }
    return this
  }

  orderByDescending(field: NestedKeyOf<T> | string): this {
    return this.orderBy(field, 'desc')
  }

  search(text: string, fields: (NestedKeyOf<T> | string)[]): this {
    if (!text) return this

    this._searchText = text
    this._searchFields = fields.map((f) => this._mappings.get(f as string) || f) as string[]
    return this
  }

  page(index: number, size: number): this {
    this._page = index
    this._pageSize = size
    return this
  }

  build(): {
    filter?: string
    orderBy?: string[]
    search?: string
    searchField?: string[]
    page?: number
    pageSize?: number
  } {
    const params: Record<string, unknown> = {}

    if (this._filterParts.length > 0) {
      params.filter = this._filterParts.join('')
    }

    if (this._sorts.length > 0) {
      params.orderBy = this._sorts
    }

    if (this._searchText) {
      params.search = this._searchText
      if (this._searchFields.length > 0) {
        params.searchField = this._searchFields
      }
    }

    if (this._page !== undefined) params.page = this._page
    if (this._pageSize !== undefined) params.pageSize = this._pageSize

    return params
  }

  buildFilterString(): string {
    return this._filterParts.join('')
  }

  private appendSeparator(): void {
    if (this._filterParts.length > 0) {
      const last = this._filterParts[this._filterParts.length - 1]
      if (last !== '(' && last !== '|') {
        this._filterParts.push(',')
      }
    }
  }

  private formatValue(value: unknown): string {
    if (value === null || value === undefined) return 'null'
    if (value instanceof Date) return value.toISOString()
    if (typeof value === 'boolean') return value ? 'true' : 'false'
    return String(value)
  }
}

export function queryBuilder<T extends object = Record<string, unknown>>(): QueryBuilder<T> {
  return new QueryBuilder<T>()
}