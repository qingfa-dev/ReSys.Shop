import { describe, it, expect } from 'vitest'
import type {
  SearchParams,
  SearchFieldConfig,
  SearchableFields,
} from '../models/search.model'

describe('Search Model', () => {
  describe('SearchParams', () => {
    it('should allow optional search fields', () => {
      const params: SearchParams = {}
      expect(params.search).toBeUndefined()
      expect(params.searchFields).toBeUndefined()
    })

    it('should allow setting search and fields', () => {
      const params: SearchParams = {
        search: 'laptop',
        searchFields: ['name', 'description', 'category.name'],
      }
      expect(params.search).toBe('laptop')
      expect(params.searchFields).toHaveLength(3)
    })
  })

  describe('SearchFieldConfig', () => {
    it('should have correct structure', () => {
      const config: SearchFieldConfig = {
        source: 'q',
        apiFields: ['Name', 'Description', 'Category.Name'],
      }
      expect(config.source).toBe('q')
      expect(config.apiFields).toEqual(['Name', 'Description', 'Category.Name'])
    })
  })

  describe('SearchableFields', () => {
    interface TestProduct {
      id: string
      name: string
      description: string
      category: {
        name: string
        parent: {
          id: string
        }
      }
    }

    type ProductSearchable = SearchableFields<TestProduct>

    it('should extract top-level searchable fields', () => {
      const field: ProductSearchable = 'name'
      expect(field).toBe('name')
    })

    it('should include nested fields', () => {
      const field: ProductSearchable = 'category.name'
      expect(field).toBe('category.name')
    })

    it('should include deeply nested fields', () => {
      const field: ProductSearchable = 'category.parent.id'
      expect(field).toBe('category.parent.id')
    })
  })
})