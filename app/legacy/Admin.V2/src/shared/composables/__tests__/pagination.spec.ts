import { describe, it, expect } from 'vitest'
import { usePagination } from '../usePagination'

describe('usePagination', () => {
  it('initial state with default pageSize 10', () => {
    const { page, pageSize, totalPages, isFirstPage, isLastPage } = usePagination()
    expect(page.value).toBe(1)
    expect(pageSize.value).toBe(10)
    expect(totalPages.value).toBe(1)
    expect(isFirstPage.value).toBe(true)
    expect(isLastPage.value).toBe(true)
  })

  it('custom pageSize', () => {
    const { pageSize } = usePagination(25)
    expect(pageSize.value).toBe(25)
  })

  it('goToPage clamps to valid range', () => {
    const p = usePagination()
    p.goToPage(0)
    expect(p.page.value).toBe(1)
    p.goToPage(999)
    expect(p.page.value).toBe(1)
  })

  it('nextPage advances when not on last page', () => {
    const p = usePagination(10)
    p.totalCount.value = 50
    p.goToPage(2)
    p.nextPage()
    expect(p.page.value).toBe(3)
  })

  it('nextPage does not advance past last page', () => {
    const p = usePagination(10)
    p.totalCount.value = 50
    p.goToPage(5)
    p.nextPage()
    expect(p.page.value).toBe(5)
  })

  it('prevPage goes back', () => {
    const p = usePagination(10)
    p.totalCount.value = 50
    p.goToPage(3)
    p.prevPage()
    expect(p.page.value).toBe(2)
  })

  it('prevPage does not go below 1', () => {
    const p = usePagination()
    p.prevPage()
    expect(p.page.value).toBe(1)
  })

  it('reset restores defaults', () => {
    const p = usePagination()
    p.totalCount.value = 100
    p.goToPage(5)
    p.reset()
    expect(p.page.value).toBe(1)
    expect(p.pageSize.value).toBe(10)
    expect(p.totalCount.value).toBe(0)
  })
})
