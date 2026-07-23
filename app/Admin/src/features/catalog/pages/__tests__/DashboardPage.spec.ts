import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import DashboardPage from '../DashboardPage.vue'

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'catalog.dashboard.title': 'Catalog',
        'catalog.dashboard.description': 'Overview of your product catalog',
        'catalog.dashboard.total_products': 'Total Products',
        'catalog.dashboard.active': 'active',
        'catalog.dashboard.inactive': 'inactive',
        'catalog.dashboard.this_month': 'this month',
        'catalog.dashboard.catalog_coverage': 'Catalog Coverage',
        'catalog.dashboard.needs_attention': 'Needs Attention',
        'catalog.dashboard.quick_actions': 'Quick Actions',
        'catalog.dashboard.add_product': 'Add Product',
        'catalog.dashboard.import_csv': 'Import CSV',
        'catalog.dashboard.manage_categories': 'Manage Categories',
        'catalog.dashboard.recently_updated': 'Recently Updated',
        'catalog.dashboard.recent_empty': 'No products added yet.',
        'catalog.dashboard.attention_empty': 'Everything looks good',
        'catalog.taxonomies.titles.list': 'Taxonomies',
        'catalog.option_types.titles.list': 'Option Types',
      }
      return map[key] ?? key
    },
  }),
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { name: 'catalog.products.create', path: '/catalog/products/new', component: { template: '<div />' } },
    { name: 'catalog.taxonomies.list', path: '/catalog/taxonomies', component: { template: '<div />' } },
  ],
})

describe('Catalog DashboardPage', () => {
  it('renders page header with correct title', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('Catalog')
  })

  it('renders hero section with total product count', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('1,247')
    expect(wrapper.text()).toContain('total products')
  })

  it('renders 4 stat cards with metrics', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    const statCards = wrapper.findAllComponents({ name: 'StatCard' })
    expect(statCards).toHaveLength(4)
    expect(wrapper.text()).toContain('Taxonomies')
    expect(wrapper.text()).toContain('Option Types')
    expect(wrapper.text()).toContain('Catalog Coverage')
    expect(wrapper.text()).toContain('Needs Attention')
  })

  it('renders quick action buttons', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('Add Product')
    expect(wrapper.text()).toContain('Import CSV')
    expect(wrapper.text()).toContain('Manage Categories')
  })

  it('renders recently updated product list', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('Recently Updated')
    expect(wrapper.text()).toContain('Vintage Denim Jacket')
    expect(wrapper.text()).toContain('Merino Wool Sweater')
  })

  it('renders needs attention section', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('No primary image')
    expect(wrapper.text()).toContain('Missing category')
    expect(wrapper.text()).toContain('Out of stock')
    expect(wrapper.text()).toContain('No price set')
  })
})
