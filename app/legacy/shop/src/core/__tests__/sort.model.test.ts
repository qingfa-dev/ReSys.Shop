import { describe, it, expect } from 'vitest'
import type {
  SortDirection,
  SortParams,
  SortFieldConfig,
  SortableFields,
} from '../models/sort.model'

describe('Sort Model', () => {
  describe('SortDirection', () => {
    it('should have asc and desc', () => {
      const directions: SortDirection[] = ['asc', 'desc']
      directions.forEach((d) => expect(d).toBeDefined())
    })
  })

  describe('SortParams', () => {
    it('should allow optional sort fields', () => {
      const params: SortParams = {}
      expect(params.sortBy).toBeUndefined()
      expect(params.sortOrder).toBeUndefined()
      expect(params.orderBy).toBeUndefined()
    })

    it('should allow setting all fields', () => {
      const params: SortParams = {
        sortBy: 'name',
        sortOrder: 'desc',
        orderBy: ['price asc', 'createdAt desc'],
      }
      expect(params.sortBy).toBe('name')
      expect(params.sortOrder).toBe('desc')
      expect(params.orderBy).toHaveLength(2)
    })
  })

  describe('SortFieldConfig', () => {
    it('should have correct structure', () => {
      const config: SortFieldConfig = {
        source: 'name',
        defaultOrder: 'asc',
        mapping: {
          name: 'Name',
          price: 'Price',
        },
      }
      expect(config.source).toBe('name')
      expect(config.defaultOrder).toBe('asc')
      expect(config.mapping?.name).toBe('Name')
    })

    it('should allow optional defaultOrder', () => {
      const config: SortFieldConfig = {
        source: 'price',
      }
      expect(config.defaultOrder).toBeUndefined()
    })
  })

  describe('SortableFields', () => {
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

    type ProductSortable = SortableFields<TestProduct>

    it('should extract top-level sortable fields', () => {
      const field: ProductSortable = 'name'
      expect(field).toBe('name')
    })

    it('should include nested fields', () => {
      const field: ProductSortable = 'category.name'
      expect(field).toBe('category.name')
    })

    it('should include deeply nested fields', () => {
      const field: ProductSortable = 'category.parent.id'
      expect(field).toBe('category.parent.id')
    })
  })
})