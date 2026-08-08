import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import TaxonTree from '../TaxonTree.vue'
import { useCatalogStore } from '@/features/catalog/stores/catalogStore'
import type { TaxonTreeNode } from '@/features/catalog/types'

// Polyfill: Overlay components call matchMedia on mount; jsdom does not provide it.
function createMatchMediaStub(query: string) {
  return {
    matches: false,
    media: query,
    onchange: null,
    addEventListener: vi.fn<() => void>(),
    removeEventListener: vi.fn<() => void>(),
    addListener: vi.fn<() => void>(),
    removeListener: vi.fn<() => void>(),
    dispatchEvent: vi.fn<() => void>(),
  }
}

beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn<typeof createMatchMediaStub>(createMatchMediaStub))
})

// Fixture: Root taxon with two leaf children for checkbox filtering.
const taxonomyTree: TaxonTreeNode[] = [
  {
    id: 'root-1',
    name: 'Clothing',
    presentation: 'Clothing',
    permalink: '/clothing',
    depth: 0,
    hasChildren: true,
    children: [
      {
        id: 'leaf-1',
        name: 'T-Shirts',
        presentation: null,
        permalink: '/clothing/t-shirts',
        depth: 1,
        hasChildren: false,
        children: [],
      },
      {
        id: 'leaf-2',
        name: 'Jeans',
        presentation: null,
        permalink: '/clothing/jeans',
        depth: 1,
        hasChildren: false,
        children: [],
      },
    ],
  },
]

// Mount: PrimeVue + stubbed pinia so store actions become spies.
function mountTree() {
  return mount(TaxonTree, {
    props: { nodes: taxonomyTree },
    global: {
      plugins: [PrimeVue, createTestingPinia()],
    },
  })
}

describe('TaxonTree', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders root and child labels with roots expanded by default', () => {
    const wrapper = mountTree()

    expect(wrapper.text()).toContain('Clothing')
    expect(wrapper.text()).toContain('T-Shirts')
    expect(wrapper.text()).toContain('Jeans')
    expect(wrapper.text()).toContain('2')
  })

  it('calls catalogStore.toggleTaxon with the leaf id when its checkbox is checked', async () => {
    const wrapper = mountTree()
    const catalog = useCatalogStore()

    const leafNode = wrapper.findAll('.p-tree-node').find(n => n.text().includes('T-Shirts'))
    expect(leafNode).toBeDefined()

    await leafNode!.find('.p-tree-node-checkbox').trigger('click')

    expect(catalog.toggleTaxon).toHaveBeenCalledWith('leaf-1')
  })

  it('reflects pre-selected taxon ids as checked checkboxes', async () => {
    const wrapper = mountTree()
    const catalog = useCatalogStore()
    catalog.selectedTaxonIds = ['leaf-2']
    await nextTick()

    const leafNode = wrapper.findAll('.p-tree-node').find(n => n.text().includes('Jeans'))
    expect(leafNode!.find('.p-tree-node-checkbox[data-p-checked="true"]').exists()).toBe(true)
  })
})
