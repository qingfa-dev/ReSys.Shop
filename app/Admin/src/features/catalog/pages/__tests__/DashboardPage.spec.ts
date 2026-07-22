import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import DashboardPage from '../DashboardPage.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [{ path: '/', component: { template: '<div />' } }],
})

describe('Catalog DashboardPage', () => {
  it('renders page header with correct title', () => {
    const wrapper = mount(DashboardPage, {
      global: { plugins: [router] },
    })
    expect(wrapper.text()).toContain('Catalog Dashboard')
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
