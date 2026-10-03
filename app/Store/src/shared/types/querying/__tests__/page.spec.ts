import { describe, it, expect } from 'vitest'
import { defaultPageBounds, emptyPageModel } from '../page'

describe('defaultPageBounds', () => {
  it('has sensible defaults', () => {
    expect(defaultPageBounds.defaultPage).toBe(1)
    expect(defaultPageBounds.defaultPageSize).toBe(20)
    expect(defaultPageBounds.maxPageSize).toBe(100)
  })
})

describe('emptyPageModel', () => {
  it('is empty with defaults', () => {
    expect(emptyPageModel.isEmpty).toBe(true)
    expect(emptyPageModel.page).toBe(1)
    expect(emptyPageModel.pageSize).toBe(20)
  })
})
