import { describe, it, expect } from 'vitest'
import type { PagedResult } from '../paged-result'

describe('PagedResult<T>', () => {
  it('matches backend PagedResult<T> shape', () => {
    const r: PagedResult<number> = {
      items: [1, 2, 3],
      totalCount: 3,
      page: 1,
      pageSize: 10,
    }
    expect(r.items).toHaveLength(3)
    expect(r.totalCount).toBe(3)
  })
})
