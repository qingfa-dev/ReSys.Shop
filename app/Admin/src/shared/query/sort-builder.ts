import type { SortClause } from './types'

export class SortBuilder {
  private clauses: SortClause[] = []

  orderBy(field: string): this {
    this.clauses.push({ field, direction: 'asc' })
    return this
  }

  orderByDesc(field: string): this {
    this.clauses.push({ field, direction: 'desc' })
    return this
  }

  thenBy(field: string): this {
    this.clauses.push({ field, direction: 'asc' })
    return this
  }

  thenByDesc(field: string): this {
    this.clauses.push({ field, direction: 'desc' })
    return this
  }

  build(): string[] | undefined {
    if (this.clauses.length === 0) return undefined
    return this.clauses.map((c) => `${c.field}:${c.direction}`)
  }
}
