import { describe, it, expect } from 'vitest'
import {
  defaultPageBounds,
  normalizePage,
  normalizePageSize,
  skip,
  totalPages,
  createPageModel,
} from '../pagination.model'
import type { PageBounds } from '../pagination.model'

describe('normalizePage', () => {
  it('returns defaultPage (1) when page is undefined', () => {
    expect(normalizePage(undefined, defaultPageBounds)).toBe(1)
  })

  it('clamps 0 to 1 (minimum page)', () => {
    expect(normalizePage(0, defaultPageBounds)).toBe(1)
  })

  it('clamps negative values to 1', () => {
    expect(normalizePage(-5, defaultPageBounds)).toBe(1)
  })

  it('returns the page as-is when within valid range', () => {
    expect(normalizePage(5, defaultPageBounds)).toBe(5)
  })

  it('returns a large page number unchanged', () => {
    expect(normalizePage(999999, defaultPageBounds)).toBe(999999)
  })
})

describe('normalizePageSize', () => {
  it('returns defaultPageSize (10) when pageSize is undefined', () => {
    expect(normalizePageSize(undefined, defaultPageBounds)).toBe(10)
  })

  it('clamps 0 to 1 (minimum page size)', () => {
    expect(normalizePageSize(0, defaultPageBounds)).toBe(1)
  })

  it('clamps values exceeding maxPageSize to 100', () => {
    expect(normalizePageSize(999, defaultPageBounds)).toBe(100)
  })

  it('returns the pageSize as-is when within valid range', () => {
    expect(normalizePageSize(25, defaultPageBounds)).toBe(25)
  })
})

describe('skip', () => {
  it('returns 0 for page 1 with pageSize 10', () => {
    expect(skip(1, 10)).toBe(0)
  })

  it('returns 40 for page 3 with pageSize 20', () => {
    expect(skip(3, 20)).toBe(40)
  })

  it('returns -10 for page 0 with pageSize 10 (no input validation in skip)', () => {
    expect(skip(0, 10)).toBe(-10)
  })
})

describe('totalPages', () => {
  it('returns 10 when 100 items with pageSize 10', () => {
    expect(totalPages(100, 10)).toBe(10)
  })

  it('returns 3 when 25 items with pageSize 10 (ceil)', () => {
    expect(totalPages(25, 10)).toBe(3)
  })

  it('returns 0 when pageSize is 0', () => {
    expect(totalPages(100, 0)).toBe(0)
  })

  it('returns 0 when totalCount is 0', () => {
    expect(totalPages(0, 10)).toBe(0)
  })
})

describe('createPageModel', () => {
  it('returns default model when called with no arguments', () => {
    expect(createPageModel()).toEqual({
      page: 1,
      pageSize: 10,
      bounds: defaultPageBounds,
    })
  })

  it('returns model with provided page and pageSize, using default bounds', () => {
    expect(createPageModel(3, 20)).toEqual({
      page: 3,
      pageSize: 20,
      bounds: defaultPageBounds,
    })
  })

  it('uses custom bounds defaults when page and pageSize are undefined', () => {
    const customBounds: PageBounds = {
      defaultPage: 5,
      defaultPageSize: 50,
      maxPageSize: 200,
    }

    expect(createPageModel(undefined, undefined, customBounds)).toEqual({
      page: 5,
      pageSize: 50,
      bounds: customBounds,
    })
  })

  it('clamps pageSize to maxPageSize when both page and pageSize are out of range', () => {
    expect(createPageModel(9999, 9999)).toEqual({
      page: 9999,
      pageSize: 100,
      bounds: defaultPageBounds,
    })
  })
})
