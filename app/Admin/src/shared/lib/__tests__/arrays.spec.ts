import { describe, it, expect } from 'vitest'
import { groupBy, sortBy, uniqueBy } from '../arrays'

describe('arrays', () => {
  it('groupBy', () => {
    const r = groupBy([{ k: 'a', v: 1 }, { k: 'b', v: 2 }, { k: 'a', v: 3 }], (x) => x.k)
    expect(r.a).toEqual([{ k: 'a', v: 1 }, { k: 'a', v: 3 }])
    expect(r.b).toEqual([{ k: 'b', v: 2 }])
  })
  it('sortBy asc/desc', () => {
    expect(sortBy([{ n: 3 }, { n: 1 }, { n: 2 }], (x) => x.n, 'asc').map((x) => x.n)).toEqual([1, 2, 3])
    expect(sortBy([{ n: 3 }, { n: 1 }, { n: 2 }], (x) => x.n, 'desc').map((x) => x.n)).toEqual([3, 2, 1])
  })
  it('uniqueBy', () => {
    expect(uniqueBy([{ id: 1 }, { id: 2 }, { id: 1 }], (x) => x.id)).toEqual([{ id: 1 }, { id: 2 }])
  })
})
