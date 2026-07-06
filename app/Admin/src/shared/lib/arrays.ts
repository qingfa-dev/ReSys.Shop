export function groupBy<T, K extends string | number>(items: T[], key: (item: T) => K): Record<K, T[]> {
  return items.reduce(
    (acc, item) => {
      const k = key(item)
      ;(acc[k] ||= []).push(item)
      return acc
    },
    {} as Record<K, T[]>,
  )
}

export function sortBy<T>(items: T[], key: (item: T) => number | string, direction: 'asc' | 'desc' = 'asc'): T[] {
  return [...items].sort((a, b) => {
    const va = key(a)
    const vb = key(b)
    if (va < vb) return direction === 'asc' ? -1 : 1
    if (va > vb) return direction === 'asc' ? 1 : -1
    return 0
  })
}

export function uniqueBy<T>(items: T[], key: (item: T) => unknown): T[] {
  const seen = new Set<unknown>()
  return items.filter((item) => {
    const k = key(item)
    if (seen.has(k)) return false
    seen.add(k)
    return true
  })
}
