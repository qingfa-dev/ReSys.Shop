import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import ProductCard from '../ProductCard.vue'
import type { StoreProductListItemResponse } from '../../types/product'

function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/products/:slug', component: { template: '<div />' } },
    ],
  })
}

function makeProduct(overrides: Partial<StoreProductListItemResponse> = {}): StoreProductListItemResponse {
  return {
    id: 'p1',
    masterVariantId: 'v1',
    name: 'Slim Fit Oxford Shirt',
    status: 'active',
    description: 'A classic oxford shirt.',
    slug: 'slim-fit-oxford-shirt',
    styleCode: null,
    seasonName: null,
    materialComposition: null,
    careInstructions: null,
    fitNotes: null,
    department: 'Oxford',
    genderTarget: 'male',
    variantsCount: 1,
    availableOn: null,
    masterVariant: {
      id: 'v1',
      sku: 'OXF-001',
      isMaster: true,
      price: 79.99,
      currency: 'USD',
      optionValues: [],
      images: [
        { id: 'img1', url: 'https://example.com/shirt.jpg', alt: null, position: 0 },
      ],
      prices: [],
      stock: { availableQuantity: 10, backorderable: false },
    },
    classifications: [],
    ...overrides,
  }
}

describe('ProductCard', () => {
  describe('smoke tests', () => {
    it('renders product name', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: { product: makeProduct() },
        global: { plugins: [router] },
      })

      expect(wrapper.text()).toContain('Slim Fit Oxford Shirt')
    })

    it('links to product detail page', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: { product: makeProduct() },
        global: { plugins: [router] },
      })

      const link = wrapper.find('a')
      expect(link.attributes('href')).toBe('/products/slim-fit-oxford-shirt')
    })

    it('shows brand from department', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: { product: makeProduct({ department: 'Nike' }) },
        global: { plugins: [router] },
      })

      expect(wrapper.text()).toContain('Nike')
    })

    it('hides brand when department is null', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: { product: makeProduct({ department: null }) },
        global: { plugins: [router] },
      })

      // The brand <p> should not render, so uppercase tracker should be absent
      const paragraphs = wrapper.findAll('p')
      const brandTexts = paragraphs.filter((p) =>
        p.classes().includes('tracking-wide'),
      )
      expect(brandTexts).toHaveLength(0)
    })

    it('shows formatted price', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: { product: makeProduct() },
        global: { plugins: [router] },
      })

      expect(wrapper.text()).toContain('$79.99')
    })

    it('renders image when available', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: { product: makeProduct() },
        global: { plugins: [router] },
      })

      expect(wrapper.find('img').exists()).toBe(true)
      expect(wrapper.find('img').attributes('src')).toBe('https://example.com/shirt.jpg')
    })

    it('shows icon fallback when no image', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const product = makeProduct({
        masterVariant: {
          id: 'v2',
          sku: null,
          isMaster: true,
          price: 50,
          currency: 'USD',
          optionValues: [],
          images: [],
          prices: [],
          stock: { availableQuantity: 0, backorderable: false },
        },
      })

      const wrapper = mount(ProductCard, {
        props: { product },
        global: { plugins: [router] },
      })

      expect(wrapper.find('img').exists()).toBe(false)
      expect(wrapper.find('svg').exists()).toBe(true)
    })

    it('shows similarity badge when enabled', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: {
          product: makeProduct(),
          showSimilarity: true,
          similarityScore: 0.8734,
        },
        global: { plugins: [router] },
      })

      expect(wrapper.text()).toContain('87.3%')
    })

    it('hides similarity badge when disabled', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: { product: makeProduct(), showSimilarity: false },
        global: { plugins: [router] },
      })

      expect(wrapper.text()).not.toContain('%')
    })

    it('uses custom aspect ratio', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: { product: makeProduct(), aspectRatio: 'aspect-square' },
        global: { plugins: [router] },
      })

      const imageDiv = wrapper.find('.bg-neutral-100')
      expect(imageDiv.classes()).toContain('aspect-square')
      expect(imageDiv.classes()).not.toContain('aspect-[3/4]')
    })

    it('uses default aspect ratio when not specified', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const wrapper = mount(ProductCard, {
        props: { product: makeProduct() },
        global: { plugins: [router] },
      })

      const imageDiv = wrapper.find('.bg-neutral-100')
      expect(imageDiv.classes()).toContain('aspect-[3/4]')
    })

    it('does not show price when masterVariant has no price', async () => {
      const router = createTestRouter()
      await router.push('/')
      await router.isReady()

      const product = makeProduct({
        masterVariant: {
          id: 'v3',
          sku: null,
          isMaster: true,
          price: null,
          currency: null,
          optionValues: [],
          images: [],
          prices: [],
          stock: { availableQuantity: 0, backorderable: false },
        },
      })

      const wrapper = mount(ProductCard, {
        props: { product },
        global: { plugins: [router] },
      })

      const priceParagraphs = wrapper.findAll('p').filter((p) =>
        p.element.style.fontFamily === "'JetBrains Mono', monospace",
      )
      expect(priceParagraphs).toHaveLength(0)
    })
  })
})
