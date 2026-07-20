import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import CopyButton from '../data-display/CopyButton.vue'

describe('CopyButton', () => {
  const writeText = vi.fn().mockResolvedValue(undefined)

  beforeAll(() => {
    Object.defineProperty(navigator, 'clipboard', {
      value: { writeText },
      writable: true,
    })
  })

  beforeEach(() => {
    writeText.mockClear()
  })

  it('renders link variant by default', () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'copy-me' },
    })
    expect(wrapper.find('button').exists()).toBe(true)
  })

  it('copies value to clipboard on click', async () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'SKU-12345' },
    })
    await wrapper.find('button').trigger('click')
    expect(writeText).toHaveBeenCalledWith('SKU-12345')
  })

  it('shows tooltip with label', () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'test', label: 'Copy ID' },
    })
    expect(wrapper.find('button').attributes('title')).toBe('Copy ID')
  })

  it('renders default icon pi-copy', () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'test' },
    })
    expect(wrapper.find('i').classes()).toContain('pi-copy')
  })

  it('renders custom icon', () => {
    const wrapper = mount(CopyButton, {
      props: { value: 'test', icon: 'pi pi-link' },
    })
    expect(wrapper.find('i').classes()).toContain('pi-link')
  })
})
