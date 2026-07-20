import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import PageContainer from '../layout/PageContainer.vue'

describe('PageContainer', () => {
  it('renders slot content', () => {
    const wrapper = mount(PageContainer, {
      slots: { default: '<p class="content">Hello</p>' },
    })
    expect(wrapper.find('.content').exists()).toBe(true)
    expect(wrapper.find('.content').text()).toBe('Hello')
  })

  it('applies default max-width', () => {
    const wrapper = mount(PageContainer, {
      slots: { default: '<p>test</p>' },
    })
    const container = wrapper.find('.page-container')
    expect(container.attributes('style')).toContain('max-width: 1504px')
  })

  it('applies custom maxWidth', () => {
    const wrapper = mount(PageContainer, {
      props: { maxWidth: '800px' },
      slots: { default: '<p>test</p>' },
    })
    const container = wrapper.find('.page-container')
    expect(container.attributes('style')).toContain('max-width: 800px')
  })

  it('wraps content in card when card is true', () => {
    const wrapper = mount(PageContainer, {
      props: { card: true },
      slots: { default: '<p>test</p>' },
    })
    expect(wrapper.find('.card').exists()).toBe(true)
  })

  it('does not wrap in card when card is false', () => {
    const wrapper = mount(PageContainer, {
      props: { card: false },
      slots: { default: '<p>test</p>' },
    })
    expect(wrapper.find('.card').exists()).toBe(false)
  })

  it('card defaults to true', () => {
    const wrapper = mount(PageContainer, {
      slots: { default: '<p>test</p>' },
    })
    expect(wrapper.find('.card').exists()).toBe(true)
  })
})
