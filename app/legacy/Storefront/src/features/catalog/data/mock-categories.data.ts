import type { Category } from '../types/schemas/product.schema'

export const mockCategories: Category[] = [
  { id: 'cat-1', name: 'Clothing', slug: 'clothing', image: 'https://picsum.photos/seed/cat1/400/300' },
  { id: 'cat-2', name: 'Shoes', slug: 'shoes', image: 'https://picsum.photos/seed/cat2/400/300' },
  { id: 'cat-3', name: 'Accessories', slug: 'accessories', image: 'https://picsum.photos/seed/cat3/400/300' },
  { id: 'cat-4', name: 'Outerwear', slug: 'outerwear', image: 'https://picsum.photos/seed/cat4/400/300' },
  { id: 'cat-5', name: 'Activewear', slug: 'activewear', image: 'https://picsum.photos/seed/cat5/400/300' },
  { id: 'cat-6', name: 'Formal', slug: 'formal', image: 'https://picsum.photos/seed/cat6/400/300' },
]

export function getCategoryById(id: string): Category | undefined {
  return mockCategories.find(c => c.id === id)
}

export function getCategoryBySlug(slug: string): Category | undefined {
  return mockCategories.find(c => c.slug === slug)
}