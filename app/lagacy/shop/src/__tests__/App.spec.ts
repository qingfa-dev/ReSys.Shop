import { describe, it, expect, beforeEach, vi } from 'vitest'

import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import App from '../App.vue'

vi.stubGlobal('matchMedia', vi.fn((query) => ({
  matches: false,
  media: query,
  onchange: null,
  addListener: vi.fn(),
  removeListener: vi.fn(),
  addEventListener: vi.fn(),
  removeEventListener: vi.fn(),
  dispatchEvent: vi.fn(),
})))

describe('App', () => {
  let router: ReturnType<typeof createRouter>

  beforeEach(() => {
    const pinia = createPinia()
    setActivePinia(pinia)

    router = createRouter({
      history: createWebHistory(),
      routes: [
        { path: '/', name: 'home', component: { template: '<div>Home</div>' } },
        { path: '/shop', name: 'shop', component: { template: '<div>Shop</div>' } },
        { path: '/collections', name: 'collections', component: { template: '<div>Collections</div>' } },
        { path: '/about', name: 'about', component: { template: '<div>About</div>' } },
        { path: '/cart', name: 'cart', component: { template: '<div>Cart</div>' } },
        { path: '/account', name: 'account', component: { template: '<div>Account</div>' } },
      ],
    })
  })

  it('mounts renders properly', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [router, PrimeVue],
      },
    })

    expect(wrapper.text()).toContain('ReSys.Shop')
  })
})