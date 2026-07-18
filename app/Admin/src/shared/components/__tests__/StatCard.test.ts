import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import StatCard from '../StatCard.Component.vue'

describe('StatCard', () => {
  it('renders title and value', () => {
    const wrapper = mount(StatCard, {
      props: { title: 'Revenue', value: '$12,450', icon: 'pi pi-dollar', iconBg: 'bg-green-100' },
      global: { stubs: { Skeleton: true } },
    })
    expect(wrapper.text()).toContain('Revenue')
    expect(wrapper.text()).toContain('$12,450')
  })

  it('renders icon in iconBg circle', () => {
    const wrapper = mount(StatCard, {
      props: { title: 'Orders', value: '42', icon: 'pi pi-shopping-cart', iconBg: 'bg-blue-100' },
      global: { stubs: { Skeleton: true } },
    })
    expect(wrapper.find('i.pi-shopping-cart').exists()).toBe(true)
  })

  it('shows positive trend arrow and percentage', () => {
    const wrapper = mount(StatCard, {
      props: {
        title: 'Users', value: '1,234', icon: 'pi pi-users', iconBg: 'bg-purple-100',
        trendValue: 12, trendPositive: true,
      },
      global: { stubs: { Skeleton: true } },
    })
    expect(wrapper.find('i.pi-arrow-up').exists()).toBe(true)
    expect(wrapper.text()).toContain('12%')
  })

  it('shows negative trend arrow and percentage', () => {
    const wrapper = mount(StatCard, {
      props: {
        title: 'Bounce', value: '5%', icon: 'pi pi-chart-line', iconBg: 'bg-red-100',
        trendValue: 3, trendPositive: false,
      },
      global: { stubs: { Skeleton: true } },
    })
    expect(wrapper.find('i.pi-arrow-down').exists()).toBe(true)
    expect(wrapper.text()).toContain('3%')
  })

  it('renders skeleton placeholders when skeleton is true', () => {
    const wrapper = mount(StatCard, {
      props: { title: 'Loading', value: '', icon: 'pi', iconBg: 'bg-gray-100', skeleton: true },
      global: { stubs: { Skeleton: true } },
    })
    const skeletons = wrapper.findAllComponents({ name: 'Skeleton' })
    expect(skeletons.length).toBe(3)
  })

  it('does not render trend section when trendValue is undefined', () => {
    const wrapper = mount(StatCard, {
      props: { title: 'Revenue', value: '$100', icon: 'pi pi-dollar', iconBg: 'bg-green-100' },
      global: { stubs: { Skeleton: true } },
    })
    expect(wrapper.find('i.pi-arrow-up').exists()).toBe(false)
    expect(wrapper.find('i.pi-arrow-down').exists()).toBe(false)
  })
})
