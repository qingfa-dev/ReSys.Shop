import { describe, it, expect } from 'vitest'
import { QueryBuilder, queryBuilder } from '../helpers/query.builder'

describe('QueryBuilder', () => {
  describe('queryBuilder factory', () => {
    it('should create a new builder instance', () => {
      const builder = queryBuilder()
      expect(builder).toBeInstanceOf(QueryBuilder)
    })
  })

  describe('where()', () => {
    it('should add simple equality filter', () => {
      const builder = queryBuilder()
      builder.where('name', '=', 'John')
      const result = builder.build()
      expect(result.filter).toBe('name=John')
    })

    it('should add greater than filter', () => {
      const builder = queryBuilder()
      builder.where('price', '>', 100)
      const result = builder.build()
      expect(result.filter).toBe('price>100')
    })

    it('should add contains filter', () => {
      const builder = queryBuilder()
      builder.where('name', '*', 'apple')
      const result = builder.build()
      expect(result.filter).toBe('name*apple')
    })

    it('should ignore undefined values', () => {
      const builder = queryBuilder()
      builder.where('name', '=', undefined)
      const result = builder.build()
      expect(result.filter).toBeUndefined()
    })

    it('should ignore empty string values', () => {
      const builder = queryBuilder()
      builder.where('name', '=', '')
      const result = builder.build()
      expect(result.filter).toBeUndefined()
    })

    it('should combine multiple conditions with comma', () => {
      const builder = queryBuilder()
      builder.where('name', '=', 'John').where('age', '>', 18)
      const result = builder.build()
      expect(result.filter).toBe('name=John,age>18')
    })

    it('should handle boolean values', () => {
      const builder = queryBuilder()
      builder.where('active', '=', true)
      const result = builder.build()
      expect(result.filter).toBe('active=true')
    })

    it('should handle null values', () => {
      const builder = queryBuilder()
      builder.where('deleted', '=', null)
      const result = builder.build()
      expect(result.filter).toBe('deleted=null')
    })

    it('should handle Date values', () => {
      const date = new Date('2024-01-15T00:00:00Z')
      const builder = queryBuilder()
      builder.where('created', '=', date)
      const result = builder.build()
      expect(result.filter).toContain('created=')
      expect(result.filter).toContain('2024-01-15')
    })

    it('should handle starts with operator', () => {
      const builder = queryBuilder()
      builder.where('name', '^', 'John')
      const result = builder.build()
      expect(result.filter).toBe('name^John')
    })

    it('should handle ends with operator', () => {
      const builder = queryBuilder()
      builder.where('name', '$', 'Doe')
      const result = builder.build()
      expect(result.filter).toBe('name$Doe')
    })

    it('should handle not equal operator', () => {
      const builder = queryBuilder()
      builder.where('status', '!=', 'inactive')
      const result = builder.build()
      expect(result.filter).toBe('status!=inactive')
    })
  })

  describe('or()', () => {
    it('should add OR operator between conditions', () => {
      const builder = queryBuilder()
      builder.where('status', '=', 'active').or().where('priority', '=', 'high')
      const result = builder.build()
      expect(result.filter).toBe('status=active|priority=high')
    })
  })

  describe('startGroup() and endGroup()', () => {
    it('should add parentheses for grouping', () => {
      const builder = queryBuilder()
      builder
        .startGroup()
        .where('a', '=', 1)
        .or()
        .where('b', '=', 2)
        .endGroup()
        .where('c', '=', 3)
      const result = builder.build()
      expect(result.filter).toBe('(a=1|b=2),c=3')
    })
  })

  describe('addRaw()', () => {
    it('should add raw filter string', () => {
      const builder = queryBuilder()
      builder.addRaw('CustomFunction(Price, 10)')
      const result = builder.build()
      expect(result.filter).toBe('CustomFunction(Price, 10)')
    })
  })

  describe('orderBy()', () => {
    it('should add ascending sort', () => {
      const builder = queryBuilder()
      builder.orderBy('name')
      const result = builder.build()
      expect(result.orderBy).toEqual(['name'])
    })

    it('should add descending sort', () => {
      const builder = queryBuilder()
      builder.orderBy('price', 'desc')
      const result = builder.build()
      expect(result.orderBy).toEqual(['price desc'])
    })

    it('should combine multiple sort fields', () => {
      const builder = queryBuilder()
      builder.orderBy('price', 'desc').orderBy('name')
      const result = builder.build()
      expect(result.orderBy).toEqual(['price desc', 'name'])
    })
  })

  describe('orderByDescending()', () => {
    it('should add descending sort', () => {
      const builder = queryBuilder()
      builder.orderByDescending('createdAt')
      const result = builder.build()
      expect(result.orderBy).toEqual(['createdAt desc'])
    })
  })

  describe('search()', () => {
    it('should add search text and fields', () => {
      const builder = queryBuilder()
      builder.search('laptop', ['name', 'description'])
      const result = builder.build()
      expect(result.search).toBe('laptop')
      expect(result.searchField).toEqual(['name', 'description'])
    })

    it('should ignore empty search text', () => {
      const builder = queryBuilder()
      builder.search('', ['name'])
      const result = builder.build()
      expect(result.search).toBeUndefined()
    })
  })

  describe('page()', () => {
    it('should set page and pageSize', () => {
      const builder = queryBuilder()
      builder.page(2, 25)
      const result = builder.build()
      expect(result.page).toBe(2)
      expect(result.pageSize).toBe(25)
    })
  })

  describe('build()', () => {
    it('should return all query params', () => {
      const builder = queryBuilder()
      builder
        .where('status', '=', 'active')
        .where('price', '>', 100)
        .orderBy('name')
        .search('product', ['name'])
        .page(1, 20)

      const result = builder.build()

      expect(result.filter).toBe('status=active,price>100')
      expect(result.orderBy).toEqual(['name'])
      expect(result.search).toBe('product')
      expect(result.searchField).toEqual(['name'])
      expect(result.page).toBe(1)
      expect(result.pageSize).toBe(20)
    })

    it('should omit undefined values', () => {
      const builder = queryBuilder()
      builder.where('name', '=', 'test')

      const result = builder.build()

      expect(result.filter).toBeDefined()
      expect(result.orderBy).toBeUndefined()
      expect(result.search).toBeUndefined()
      expect(result.page).toBeUndefined()
    })
  })

  describe('addMap()', () => {
    it('should map field names', () => {
      const builder = queryBuilder()
      builder.addMap('cat', 'category.name').where('cat', '=', 'electronics')
      const result = builder.build()
      expect(result.filter).toBe('category.name=electronics')
    })
  })

  describe('buildFilterString()', () => {
    it('should return only filter string', () => {
      const builder = queryBuilder()
      builder.where('name', '=', 'test').orderBy('price')
      const filterStr = builder.buildFilterString()
      expect(filterStr).toBe('name=test')
    })
  })
})