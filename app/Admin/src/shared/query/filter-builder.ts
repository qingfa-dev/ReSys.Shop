import type { FilterClause, FilterLogic } from './types'

const OP_SYMBOLS: Record<string, string> = {
  eq: '=',
  neq: '!=',
  gt: '>',
  gte: '>=',
  lt: '<',
  lte: '<=',
}

export class FilterBuilder {
  private clauses: FilterClause[] = []
  private nextLogic: FilterLogic = 'and'

  where(field: string): FilterOperatorBuilder {
    return new FilterOperatorBuilder(this, field, this.nextLogic)
  }

  and(): this {
    this.nextLogic = 'and'
    return this
  }

  or(): this {
    this.nextLogic = 'or'
    return this
  }

  add(clause: FilterClause): this {
    this.clauses.push(clause)
    return this
  }

  build(): string | undefined {
    if (this.clauses.length === 0) return undefined

    return this.clauses
      .map((c) => this.renderClause(c))
      .join(', ')
  }

  private renderClause(c: FilterClause): string {
    const symbol = OP_SYMBOLS[c.operator]
    if (c.operator === 'contains') return `${c.field} = *${c.value}*`
    if (c.operator === 'starts') return `${c.field} = ${c.value}*`
    if (c.operator === 'ends') return `${c.field} = *${c.value}`

    const op = (c.caseSensitive && c.operator === 'eq') ? '==' : symbol
    return `${c.field} ${op} ${c.value}`
  }
}

class FilterOperatorBuilder {
  constructor(
    private readonly builder: FilterBuilder,
    private readonly field: string,
    private readonly logic: FilterLogic,
  ) {}

  eq(value: string | number, caseSensitive?: boolean): FilterBuilder {
    return this.builder.add({
      field: this.field,
      operator: 'eq',
      value: String(value),
      logic: this.logic,
      caseSensitive,
    })
  }

  neq(value: string | number): FilterBuilder {
    return this.builder.add({
      field: this.field,
      operator: 'neq',
      value: String(value),
      logic: this.logic,
    })
  }

  gt(value: string | number): FilterBuilder {
    return this.builder.add({
      field: this.field,
      operator: 'gt',
      value: String(value),
      logic: this.logic,
    })
  }

  gte(value: string | number): FilterBuilder {
    return this.builder.add({
      field: this.field,
      operator: 'gte',
      value: String(value),
      logic: this.logic,
    })
  }

  lt(value: string | number): FilterBuilder {
    return this.builder.add({
      field: this.field,
      operator: 'lt',
      value: String(value),
      logic: this.logic,
    })
  }

  lte(value: string | number): FilterBuilder {
    return this.builder.add({
      field: this.field,
      operator: 'lte',
      value: String(value),
      logic: this.logic,
    })
  }

  contains(value: string, caseSensitive?: boolean): FilterBuilder {
    return this.builder.add({
      field: this.field,
      operator: 'contains',
      value: String(value),
      logic: this.logic,
      caseSensitive,
    })
  }

  starts(value: string, caseSensitive?: boolean): FilterBuilder {
    return this.builder.add({
      field: this.field,
      operator: 'starts',
      value: String(value),
      logic: this.logic,
      caseSensitive,
    })
  }

  ends(value: string, caseSensitive?: boolean): FilterBuilder {
    return this.builder.add({
      field: this.field,
      operator: 'ends',
      value: String(value),
      logic: this.logic,
      caseSensitive,
    })
  }
}
