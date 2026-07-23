import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import DashboardPage from '../DashboardPage.vue'

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'catalog.products.titles.list': 'Products',
        'catalog.products.descriptions.list': 'Manage your product catalog, pricing and stock.',
        'catalog.taxonomies.titles.list': 'Taxonomies',
        'catalog.option_types.titles.list': 'Option Types',
      }
      return map[key] ?? key
    },
  }),
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [{ path: '/', component: { template: '<div />' } }],
})

describe('Catalog DashboardPage', () => {
  it('renders page header with correct title', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('Products')
  })

  it('renders 4 stat cards', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    const statCards = wrapper.findAllComponents({ name: 'StatCard' })
    expect(statCards).toHaveLength(4)
  })

  it('contains expected KPI labels', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('Total Products')
    expect(wrapper.text()).toContain('Active Products')
    expect(wrapper.text()).toContain('Taxonomies')
    expect(wrapper.text()).toContain('Option Types')
  })
})
