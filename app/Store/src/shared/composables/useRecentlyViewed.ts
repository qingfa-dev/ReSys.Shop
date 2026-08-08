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

// Cache: Module-level shared state — singleton across all component instances
let sharedItems: RecentlyViewedItem[] = []

function loadItems(): RecentlyViewedItem[] {
  try {
    // Cache: Load from localStorage with defensive parsing — corrupted data returns empty
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return []
    const parsed = JSON.parse(raw)
    if (!Array.isArray(parsed)) return []
    // Validate: Filter items missing required fields — protects against schema drift
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
  // Cache: Persist to localStorage — catch quota errors silently (best-effort)
  try { localStorage.setItem(STORAGE_KEY, JSON.stringify(items)) } catch { /* ignore */ }
}

export function useRecentlyViewed(maxItems = DEFAULT_MAX) {
  if (sharedItems.length === 0) {
    // Cache: Lazy-load from localStorage on first access
    sharedItems = loadItems()
  }

  const items = ref<RecentlyViewedItem[]>(sharedItems)

  function add(product: RecentlyViewedItem): void {
    // Filter: Remove existing entry to avoid duplicates before re-adding
    const idx = sharedItems.findIndex(i => i.productId === product.productId)
    if (idx >= 0) sharedItems.splice(idx, 1)
    sharedItems.push(product)
    // Guard: Cap list size — drop oldest when exceeding max
    if (sharedItems.length > maxItems) sharedItems.shift()
    items.value = [...sharedItems]
    saveItems(sharedItems)
  }

  function clear(): void {
    // Reset: Clear all recently viewed items from state and localStorage
    sharedItems = []
    items.value = []
    try { localStorage.removeItem(STORAGE_KEY) } catch { /* ignore */ }
  }

  return { items, add, clear }
}