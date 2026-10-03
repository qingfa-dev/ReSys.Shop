import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import ui from '@nuxt/ui/vue-plugin'
import App from '../App.vue'

describe('App', () => {
  it('mounts without error', () => {
    const router = createRouter({
      history: createWebHistory(),
      routes: [
        {
          path: '/',
          name: 'Home',
          component: { template: '<div>Home</div>' },
        },
      ],
    })

    const wrapper = mount(App, {
      global: {
        plugins: [createPinia(), router, ui],
      },
    })

    expect(wrapper.exists()).toBe(true)
  })
})
