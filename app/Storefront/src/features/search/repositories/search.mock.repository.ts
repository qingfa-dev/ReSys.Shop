import type { Result, PagedResult } from '@/core/models/result'
import { resultHelpers } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'
import type { ISearchRepository } from './search.repository.interface'

const mockProducts: Product[] = [
  {
    id: '550e8400-e29b-41d4-a716-446655440001',
    name: 'Classic T-Shirt',
    slug: 'classic-t-shirt',
    description: 'A comfortable classic t-shirt.',
    price: 29.99,
    images: ['/images/products/tshirt.jpg'],
    category: { id: 'cat-1', name: 'T-Shirts', slug: 't-shirts' },
    tags: ['cotton', 'casual'],
    inventory: { quantity: 100, trackQuantity: true, allowBackorder: false },
    createdAt: '2025-01-01T00:00:00Z',
    updatedAt: '2025-06-01T00:00:00Z',
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440002',
    name: 'Slim Fit Jeans',
    slug: 'slim-fit-jeans',
    description: 'Modern slim fit jeans.',
    price: 59.99,
    images: ['/images/products/jeans.jpg'],
    category: { id: 'cat-2', name: 'Jeans', slug: 'jeans' },
    tags: ['denim', 'bottoms'],
    inventory: { quantity: 50, trackQuantity: true, allowBackorder: true, lowStockThreshold: 10 },
    createdAt: '2025-02-01T00:00:00Z',
    updatedAt: '2025-06-15T00:00:00Z',
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440003',
    name: 'Leather Jacket',
    slug: 'leather-jacket',
    description: 'Premium leather jacket.',
    price: 199.99,
    compareAtPrice: 249.99,
    images: ['/images/products/jacket.jpg'],
    category: { id: 'cat-3', name: 'Jackets', slug: 'jackets' },
    tags: ['leather', 'outerwear'],
    inventory: { quantity: 20, trackQuantity: true, allowBackorder: false, lowStockThreshold: 5 },
    createdAt: '2025-03-01T00:00:00Z',
    updatedAt: '2025-07-01T00:00:00Z',
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440004',
    name: 'Running Shoes',
    slug: 'running-shoes',
    description: 'Lightweight running shoes.',
    price: 89.99,
    images: ['/images/products/shoes.jpg'],
    category: { id: 'cat-4', name: 'Shoes', slug: 'shoes' },
    tags: ['sports', 'footwear'],
    inventory: { quantity: 75, trackQuantity: true, allowBackorder: false },
    createdAt: '2025-04-01T00:00:00Z',
    updatedAt: '2025-07-10T00:00:00Z',
  },
  {
    id: '550e8400-e29b-41d4-a716-446655440005',
    name: 'Wool Beanie',
    slug: 'wool-beanie',
    description: 'Warm wool beanie hat.',
    price: 19.99,
    images: ['/images/products/beanie.jpg'],
    category: { id: 'cat-5', name: 'Accessories', slug: 'accessories' },
    tags: ['wool', 'winter', 'hats'],
    inventory: { quantity: 200, trackQuantity: true, allowBackorder: false, lowStockThreshold: 20 },
    createdAt: '2025-05-01T00:00:00Z',
    updatedAt: '2025-08-01T00:00:00Z',
  },
]

const mockSuggestions: string[] = [
  't-shirt',
  'jeans',
  'leather jacket',
  'running shoes',
  'wool beanie',
  'summer dress',
  'sneakers',
  'hoodie',
  'winter coat',
  'canvas backpack',
]

export class SearchMockRepository implements ISearchRepository {
  async search(query: string, filters?: Record<string, unknown>): Promise<PagedResult<Product>> {
    const q = query.toLowerCase()
    let filtered = mockProducts.filter(
      (p) =>
        p.name.toLowerCase().includes(q) ||
        p.description.toLowerCase().includes(q) ||
        p.tags.some((t) => t.toLowerCase().includes(q))
    )

    if (filters?.category) {
      filtered = filtered.filter((p) => p.category.slug === filters.category)
    }

    if (filters?.inStock === true) {
      filtered = filtered.filter((p) => p.inventory.quantity > 0)
    }

    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      items: filtered,
      page: 1,
      pageSize: filtered.length || 1,
      totalCount: filtered.length,
      totalPages: 1,
      hasNextPage: false,
      hasPreviousPage: false,
    }
  }

  async getSuggestions(query: string): Promise<Result<string[]>> {
    const q = query.toLowerCase()
    const matched = mockSuggestions.filter((s) => s.toLowerCase().includes(q))
    return resultHelpers.success(matched.slice(0, 5))
  }
}

export const searchMockRepository = new SearchMockRepository()
