import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import SkeletonLoader from '../feedback/SkeletonLoader.vue'

describe('SkeletonLoader', () => {
  it('renders table variant skeleton rows', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'table', rows: 3 },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders card variant', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'card' },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders form variant', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'form' },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders detail variant', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'detail' },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('renders list variant', () => {
    const wrapper = mount(SkeletonLoader, {
      props: { variant: 'list' },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBeGreaterThan(0)
  })

  it('defaults variant to table', () => {
    const wrapper = mount(SkeletonLoader, {
      props: {},
      global: { stubs: { Skeleton: true } },
    })
    expect(wrapper.find('.card').exists()).toBe(true)
  })
})
