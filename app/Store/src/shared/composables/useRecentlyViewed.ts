import { ref } from 'vue'

export interface RecentlyViewedItem {
  productId: string
  productName: string
  slug: string
  thumbnailUrl: string | null
  minPrice: number | null
  viewedAt: number
}

const STORAGE_KEY = 'recentlyViewed'
const DEFAULT_MAX = 10

let sharedItems: RecentlyViewedItem[] = []

function loadItems(): RecentlyViewedItem[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw)
    if (!Array.isArray(parsed)) return []
    return parsed.filter(
      (item: unknown): item is RecentlyViewedItem =>
        typeof item === 'object' &&
        item !== null &&
        typeof (item as RecentlyViewedItem).productId === 'string',
    )
  } catch {
    return []
  }
}

function saveItems(items: RecentlyViewedItem[]): void {
  try { localStorage.setItem(STORAGE_KEY, JSON.stringify(items)) } catch { /* ignore */ }
}

export function useRecentlyViewed(maxItems = DEFAULT_MAX) {
  if (sharedItems.length === 0) {
    sharedItems = loadItems()
  }

  const items = ref<RecentlyViewedItem[]>(sharedItems)

  function add(product: RecentlyViewedItem): void {
    const idx = sharedItems.findIndex(i => i.productId === product.productId)
    if (idx >= 0) sharedItems.splice(idx, 1)
    sharedItems.push(product)
    if (sharedItems.length > maxItems) sharedItems.shift()
    items.value = [...sharedItems]
    saveItems(sharedItems)
  }

  function clear(): void {
    sharedItems = []
    items.value = []
    try { localStorage.removeItem(STORAGE_KEY) } catch { /* ignore */ }
  }

  return { items, add, clear }
}