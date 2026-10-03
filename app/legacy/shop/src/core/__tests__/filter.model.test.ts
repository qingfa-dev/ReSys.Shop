import { describe, it, expect } from 'vitest'
import type {
  FilterOperator,
  FilterParams,
  FieldMapping,
  NestedKeyOf,
  FilterSchema,
} from '../models/filter.model'

describe('Filter Model', () => {
  describe('FilterOperator', () => {
    it('should have all comparison operators', () => {
      const operators: FilterOperator[] = ['=', '!=', '>', '<', '>=', '<=']
      operators.forEach((op) => expect(op).toBeDefined())
    })

    it('should have string operators', () => {
      const operators: FilterOperator[] = ['!*', '*', '^', '$']
      operators.forEach((op) => expect(op).toBeDefined())
    })
  })

  describe('FilterParams', () => {
    it('should allow optional filter string', () => {
      const params: FilterParams = {}
      expect(params.filter).toBeUndefined()

      params.filter = 'name=John'
      expect(params.filter).toBe('name=John')
    })
  })

  describe('FieldMapping', () => {
    it('should have correct structure', () => {
      const mapping: FieldMapping = {
        source: 'minPrice',
        target: 'Price',
        operator: '>=',
      }
      expect(mapping.source).toBe('minPrice')
      expect(mapping.target).toBe('Price')
      expect(mapping.operator).toBe('>=')
    })

    it('should support transform function', () => {
      const mapping: FieldMapping = {
        source: 'price',
        target: 'Price',
        transform: (v) => Number(v) * 100,
      }
      expect(mapping.transform).toBeDefined()
      expect(mapping.transform!('10')).toBe(1000)
    })

    it('should support skip function', () => {
      const mapping: FieldMapping = {
        source: 'price',
        target: 'Price',
        skip: (v) => v === undefined,
      }
      expect(mapping.skip).toBeDefined()
      expect(mapping.skip!(undefined)).toBe(true)
      expect(mapping.skip!(10)).toBe(false)
    })
  })

  describe('NestedKeyOf', () => {
    interface TestProduct {
      id: string
      name: string
      price: number
      category: {
        name: string
        parent: {
          id: string
        }
      }
    }

    type ProductKeys = NestedKeyOf<TestProduct>

    it('should extract top-level keys', () => {
      const keys: ProductKeys = 'id'
      expect(keys).toBe('id')
    })

    it('should include nested keys', () => {
      const keys: ProductKeys = 'category.name'
      expect(keys).toBe('category.name')
    })

    it('should include deeply nested keys', () => {
      const keys: ProductKeys = 'category.parent.id'
      expect(keys).toBe('category.parent.id')
    })
  })

  describe('FilterSchema', () => {
    it('should allow arbitrary key-value pairs', () => {
      const schema: FilterSchema = {
        category: 'electronics',
        minPrice: 100,
        maxPrice: 500,
        inStock: true,
      }
      expect(schema.category).toBe('electronics')
      expect(schema.minPrice).toBe(100)
    })
  })
})