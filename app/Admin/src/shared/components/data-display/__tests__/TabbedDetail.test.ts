import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import TabbedDetail from '../TabbedDetail.vue'
import { h, defineComponent } from 'vue'

describe('TabbedDetail', () => {
  const DummyPanel = defineComponent({
    setup: () => () => h('div', { class: 'dummy-panel' }, 'Details content'),
  })

  const tabs = [
    { label: 'Details', value: 'details', panel: DummyPanel },
    { label: 'History', value: 'history', panel: DummyPanel },
  ]

  it('renders tab labels', () => {
    const wrapper = mount(TabbedDetail, {
      props: { tabs, activeTab: 'details' },
      global: {
        stubs: {
          Tabs: { template: '<div class="tabs-stub"><slot /></div>' },
          TabList: { template: '<div class="tablist-stub"><slot /></div>' },
          Tab: { template: '<div class="tab-stub"><slot /></div>' },
          TabPanels: { template: '<div class="tabpanels-stub"><slot /></div>' },
          TabPanel: { template: '<div class="tabpanel-stub"><slot /></div>' },
        },
      },
    })
    expect(wrapper.text()).toContain('Details')
    expect(wrapper.text()).toContain('History')
  })

  it('renders icon next to tab label when provided', () => {
    const tabsWithIcons = [
      { label: 'Info', value: 'info', icon: 'pi pi-info-circle', panel: DummyPanel },
    ]
    const wrapper = mount(TabbedDetail, {
      props: { tabs: tabsWithIcons, activeTab: 'info' },
      global: {
        stubs: {
          Tabs: { template: '<div class="tabs-stub"><slot /></div>' },
          TabList: { template: '<div class="tablist-stub"><slot /></div>' },
          Tab: { template: '<div class="tab-stub"><slot /></div>' },
          TabPanels: { template: '<div class="tabpanels-stub"><slot /></div>' },
          TabPanel: { template: '<div class="tabpanel-stub"><slot /></div>' },
        },
      },
    })
    expect(wrapper.find('i.pi-info-circle').exists()).toBe(true)
  })

  it('renders panel component content', () => {
    const wrapper = mount(TabbedDetail, {
      props: { tabs, activeTab: 'details' },
      global: {
        stubs: {
          Tabs: { template: '<div class="tabs-stub"><slot /></div>' },
          TabList: { template: '<div class="tablist-stub"><slot /></div>' },
          Tab: { template: '<div class="tab-stub"><slot /></div>' },
          TabPanels: { template: '<div class="tabpanels-stub"><slot /></div>' },
          TabPanel: { template: '<div class="tabpanel-stub"><slot /></div>' },
        },
      },
    })
    expect(wrapper.find('.dummy-panel').exists()).toBe(true)
  })
})
