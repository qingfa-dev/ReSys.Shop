import { describe, it, expect } from 'vitest'

import { mount } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import App from '../App.vue'

describe('App', () => {
  it('renders with router', async () => {
    const router = createRouter({
      history: createWebHistory(),
      routes: [{ path: '/', component: { template: '<div>Dashboard</div>' } }],
    })
    router.push('/')
    await router.isReady()

    const wrapper = mount(App, {
      global: {
        plugins: [router],
        stubs: {
          Toast: { template: '<div />' },
          ConfirmDialog: { template: '<div />' },
        },
      },
    })
    expect(wrapper.html()).toBeTruthy()
  })
})
