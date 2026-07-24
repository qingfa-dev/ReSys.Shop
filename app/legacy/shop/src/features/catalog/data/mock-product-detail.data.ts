import type { ProductDetail } from '../types'

export const mockProductDetails: ProductDetail[] = [
  {
    id: 'prod-1',
    name: 'Classic Cotton T-Shirt',
    slug: 'classic-cotton-tshirt',
    description: 'A comfortable everyday essential made from 100% organic cotton. Features a relaxed fit and reinforced collar for durability.',
    longDescription: 'Our Classic Cotton T-Shirt is the perfect everyday essential. Made from 100% organic cotton, it offers superior comfort and breathability. The reinforced collar ensures it maintains its shape wash after wash. Perfect for layering or wearing on its own.',
    price: 29.99,
    compareAtPrice: 39.99,
    images: [
      { url: 'https://picsum.photos/seed/prod1/600/800', alt: 'Classic Cotton T-Shirt front view' },
      { url: 'https://picsum.photos/seed/prod1b/600/800', alt: 'Classic Cotton T-Shirt back view' },
    ],
    category: { id: 'cat-1', name: 'Clothing', slug: 'clothing' },
    tags: ['cotton', 'casual', 'essential', 'summer'],
    inventory: { quantity: 100, trackQuantity: true, allowBackorder: false, lowStockThreshold: 10 },
    brand: 'ReSyShop Originals',
    rating: 4.5,
    reviews: 128,
    inStock: true,
    colors: [
      { id: 'col-1', name: 'Black', hex: '#1a1a1a' },
      { id: 'col-2', name: 'White', hex: '#ffffff' },
      { id: 'col-3', name: 'Navy', hex: '#1e3a5f' },
      { id: 'col-4', name: 'Gray', hex: '#6b7280' },
    ],
    sizes: [
      { id: 'sz-xs', name: 'XS', stock: 15 },
      { id: 'sz-s', name: 'S', stock: 25 },
      { id: 'sz-m', name: 'M', stock: 30 },
      { id: 'sz-l', name: 'L', stock: 20 },
      { id: 'sz-xl', name: 'XL', stock: 10 },
    ],
    sizeChart: [
      { size: 'XS', chest: '32-34"', length: '26"', sleeves: '7"' },
      { size: 'S', chest: '34-36"', length: '27"', sleeves: '7.5"' },
      { size: 'M', chest: '36-38"', length: '28"', sleeves: '8"' },
      { size: 'L', chest: '38-40"', length: '29"', sleeves: '8.5"' },
      { size: 'XL', chest: '40-42"', length: '30"', sleeves: '9"' },
    ],
    reviewsList: [
      { author: 'John D.', title: 'Great quality!', text: 'Perfect fit and the fabric is very comfortable.', rating: 5 },
      { author: 'Sarah M.', title: 'Good everyday tee', text: 'Nice basic tee, good value for the price.', rating: 4 },
    ],
    createdAt: '2026-01-15T10:00:00Z',
    updatedAt: '2026-04-01T10:00:00Z',
  },
  {
    id: 'prod-2',
    name: 'Slim Fit Denim Jeans',
    slug: 'slim-fit-denim-jeans',
    description: 'Modern slim fit jeans with stretch comfort. Made from premium denim with a contemporary silhouette.',
    longDescription: 'These Slim Fit Denim Jeans feature a modern slim leg silhouette crafted from premium stretch denim. The added elastane provides all-day comfort while maintaining their shape.',
    price: 79.99,
    compareAtPrice: 99.99,
    images: [
      { url: 'https://picsum.photos/seed/prod2/600/800', alt: 'Slim Fit Denim Jeans front' },
      { url: 'https://picsum.photos/seed/prod2b/600/800', alt: 'Slim Fit Denim Jeans back' },
    ],
    category: { id: 'cat-1', name: 'Clothing', slug: 'clothing' },
    tags: ['denim', 'slim-fit', 'modern', 'pants'],
    inventory: { quantity: 50, trackQuantity: true, allowBackorder: false, lowStockThreshold: 5 },
    brand: 'Urban Style',
    rating: 4.3,
    reviews: 89,
    inStock: true,
    colors: [
      { id: 'col-5', name: 'Indigo', hex: '#3f5a8c' },
      { id: 'col-6', name: 'Black', hex: '#1a1a1a' },
    ],
    sizes: [
      { id: 'sz-28', name: '28', stock: 8 },
      { id: 'sz-30', name: '30', stock: 15 },
      { id: 'sz-32', name: '32', stock: 12 },
      { id: 'sz-34', name: '34', stock: 10 },
      { id: 'sz-36', name: '36', stock: 5 },
    ],
    sizeChart: [
      { size: '28', chest: '28"', length: '30"', sleeves: '-' },
      { size: '30', chest: '30"', length: '30"', sleeves: '-' },
      { size: '32', chest: '32"', length: '32"', sleeves: '-' },
      { size: '34', chest: '34"', length: '32"', sleeves: '-' },
      { size: '36', chest: '36"', length: '32"', sleeves: '-' },
    ],
    reviewsList: [
      { author: 'Mike R.', title: 'Perfect fit', text: 'These jeans fit perfectly. Great stretch!', rating: 5 },
    ],
    createdAt: '2026-01-20T10:00:00Z',
    updatedAt: '2026-04-01T10:00:00Z',
  },
]

export function getProductDetailById(id: string): ProductDetail | undefined {
  return mockProductDetails.find(p => p.id === id)
}

export function getProductDetailBySlug(slug: string): ProductDetail | undefined {
  return mockProductDetails.find(p => p.slug === slug)
}
