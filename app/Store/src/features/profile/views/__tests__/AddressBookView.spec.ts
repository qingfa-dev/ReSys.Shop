import { describe, it, expect, vi, beforeAll, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import AddressBookView from '../AddressBookView.vue'
import { useAddressStore } from '../../stores/addressStore'
import { getCountries } from '@/features/location/services/countryApi'
import { getStates } from '@/features/location/services/stateApi'
import type { Country } from '@/features/location/types/location'
import type { Address } from '../../types'
import ConfirmDialog from 'primevue/confirmdialog'

// Confirm: Stub the service so delete clicks can be observed and accepted inline.
type RequireOptions = { accept?: () => void }
const { confirmRequire } = vi.hoisted(() => ({
  confirmRequire: vi.fn<(options: RequireOptions) => void>(),
}))
vi.mock('primevue/useconfirm', () => ({
  useConfirm: () => ({ require: confirmRequire, close: vi.fn<() => void>() }),
}))

// Mock: Location catalog APIs resolve against local fixtures, no network.
vi.mock('@/features/location/services/countryApi', () => ({
  getCountries: vi.fn<() => void>(),
}))
vi.mock('@/features/location/services/stateApi', () => ({
  getStates: vi.fn<() => void>(),
}))

const mockedCountries = vi.mocked(getCountries)
const mockedStates = vi.mocked(getStates)

// Polyfill: Dialog calls matchMedia on mount; jsdom does not provide it.
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

// Fixture: A single country used by the cascade when the dialog opens.
const vietnam: Country = {
  id: 'vn',
  name: 'Vietnam',
  isoCode: 'VN',
  callingCode: '+84',
  statesRequired: false,
  isActive: true,
}

// Router: Memory-history router with the account route.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/account/addresses', component: AddressBookView }],
  })
}

// Mount: PrimeVue + ToastService + stubbed pinia so mounted fetches are no-ops.
async function mountView() {
  mockedCountries.mockResolvedValue({
    isSuccess: true,
    statusCode: 200,
    message: null,
    errors: [],
    items: [vietnam],
    page: 1,
    pageSize: 10,
    totalCount: 1,
    totalPages: 1,
  })
  mockedStates.mockResolvedValue({
    isSuccess: true,
    statusCode: 200,
    message: null,
    errors: [],
    items: [],
    page: 1,
    pageSize: 10,
    totalCount: 0,
    totalPages: 0,
  })
  const router = createTestRouter()
  await router.push('/account/addresses')
  await router.isReady()
  const wrapper = mount(AddressBookView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
      stubs: { ConfirmDialog: true, teleport: true },
    },
  })
  await flushPromises()
  return wrapper
}

// Fixture: Addresses matching the Address contract.
const defaultAddress: Address = {
  id: 'addr-1',
  userId: 'u-1',
  addressType: 'Shipping',
  firstName: 'Alice',
  lastName: 'Nguyen',
  address1: '12 Le Loi',
  address2: null,
  city: 'Ho Chi Minh City',
  zipCode: '70000',
  phone: null,
  label: 'Home',
  isDefault: true,
  countryName: 'Vietnam',
  stateProvince: null,
  countryCode: 'VN',
  stateCode: null,
}

const secondAddress: Address = {
  id: 'addr-2',
  userId: 'u-1',
  addressType: 'Billing',
  firstName: 'Alice',
  lastName: 'Nguyen',
  address1: '88 Tran Hung Dao',
  address2: 'Apt 4B',
  city: 'Hanoi',
  zipCode: '10000',
  phone: '0912345678',
  label: null,
  isDefault: false,
  countryName: 'Vietnam',
  stateProvince: 'Hanoi',
  countryCode: 'VN',
  stateCode: 'HN',
}

// Seed: Populate the address store with two rows.
function seedAddresses() {
  const addresses = useAddressStore()
  addresses.addresses = [defaultAddress, secondAddress]
  return addresses
}

describe('AddressBookView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders address rows with labels, lines and the default tag', async () => {
    const wrapper = await mountView()
    seedAddresses()
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Home')
    expect(wrapper.text()).toContain('Alice')
    expect(wrapper.text()).toContain('12 Le Loi, Ho Chi Minh City, Vietnam')
    expect(wrapper.text()).toContain('Default')
    expect(wrapper.text()).toContain('88 Tran Hung Dao, Apt 4B, Hanoi, Hanoi, Vietnam')
  })

  it('shows the empty message when no addresses exist', async () => {
    const wrapper = await mountView()

    expect(wrapper.text()).toContain('No addresses yet — add one to speed up checkout.')
    expect(wrapper.text()).toContain('Add Address')
  })

  it('opens the add dialog and validates before calling the store', async () => {
    const wrapper = await mountView()
    const addresses = seedAddresses()

    await wrapper.findAll('button').find(b => b.text() === 'Add Address')!.trigger('click')
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Add Address')
    await wrapper.findAll('button').find(b => b.text() === 'Save Address')!.trigger('click')
    await flushPromises()

    expect(addresses.createAddress).not.toHaveBeenCalled()
    expect(wrapper.text()).toContain('Complete the required fields')
  })

  it('opens the edit dialog prefilled with the selected address', async () => {
    const wrapper = await mountView()
    seedAddresses()
    await wrapper.vm.$nextTick()

    await wrapper.findAll('[aria-label="Edit address"]')[1]!.trigger('click')
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Edit Address')
    expect((wrapper.find('#address-label').element as HTMLInputElement).value).toBe('')
    expect((wrapper.find('#address-address1').element as HTMLInputElement).value).toBe('88 Tran Hung Dao')
  })

  it('deletes an address through the confirm service accept callback', async () => {
    const wrapper = await mountView()
    const addresses = seedAddresses()
    await wrapper.vm.$nextTick()

    await wrapper.findAll('[aria-label="Delete address"]')[0]!.trigger('click')
    expect(confirmRequire).toHaveBeenCalled()
    const options = confirmRequire.mock.calls[0]![0]
    options.accept?.()
    await flushPromises()

    expect(addresses.deleteAddress).toHaveBeenCalledWith('addr-1')
  })

  it('sets an address as default via updateAddress with the default flag', async () => {
    const wrapper = await mountView()
    const addresses = seedAddresses()
    await wrapper.vm.$nextTick()

    const setDefault = wrapper.findAll('button').find(b => b.text() === 'Set default')
    await setDefault!.trigger('click')
    await flushPromises()

    const updateSpy = vi.mocked(addresses.updateAddress)
    expect(updateSpy).toHaveBeenCalledTimes(1)
    const [id, input] = updateSpy.mock.calls[0]! as unknown as [string, { isDefault: boolean }]
    expect(id).toBe('addr-2')
    expect(input.isDefault).toBe(true)
  })

  it('adds no native interactive elements of its own', async () => {
    const wrapper = await mountView()
    seedAddresses()
    await wrapper.vm.$nextTick()

    expect(wrapper.findAll('input')).toHaveLength(0)
    expect(wrapper.findAll('textarea')).toHaveLength(0)
    expect(wrapper.findAll('select')).toHaveLength(0)
  })

  it('does not render its own ConfirmDialog — App.vue already mounts the global one', async () => {
    const wrapper = await mountView()
    seedAddresses()
    await wrapper.vm.$nextTick()

    expect(wrapper.findComponent(ConfirmDialog).exists()).toBe(false)
  })
})
