import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import SearchInput from '../form/SearchInput.vue'

vi.useFakeTimers()

describe('SearchInput', () => {
  it('renders with default placeholder', () => {
    const wrapper = mount(SearchInput, {
      global: { stubs: { InputText: true, IconField: { template: '<div><slot /></div>' }, InputIcon: true } },
    })
    expect(wrapper.findComponent({ name: 'InputText' }).exists()).toBe(true)
  })

  it('renders with custom placeholder', () => {
    const wrapper = mount(SearchInput, {
      props: { placeholder: 'Find products...' },
      global: { stubs: { InputText: true, IconField: { template: '<div><slot /></div>' }, InputIcon: true } },
    })
    expect(wrapper.findComponent({ name: 'InputText' }).attributes('placeholder')).toBe('Find products...')
  })

  it('emits search after debounce', async () => {
    const wrapper = mount(SearchInput, {
      props: { debounce: 300 },
      global: { stubs: { InputText: { template: '<input @input="$emit(\'update:modelValue\', $event.target.value)" />', props: ['modelValue'], emits: ['update:modelValue'] }, IconField: { template: '<div><slot /></div>' } } },
    })

    await wrapper.find('input').setValue('test')

    vi.advanceTimersByTime(300)

    expect(wrapper.emitted('search')).toBeTruthy()
    expect(wrapper.emitted('search')![0]).toEqual(['test'])
  })

  it('does not emit search before debounce', async () => {
    const wrapper = mount(SearchInput, {
      props: { debounce: 300 },
      global: { stubs: { InputText: { template: '<input @input="$emit(\'update:modelValue\', $event.target.value)" />', props: ['modelValue'], emits: ['update:modelValue'] }, IconField: { template: '<div><slot /></div>' } } },
    })

    await wrapper.find('input').setValue('test')
    vi.advanceTimersByTime(100)

    expect(wrapper.emitted('search')).toBeFalsy()
  })
})
