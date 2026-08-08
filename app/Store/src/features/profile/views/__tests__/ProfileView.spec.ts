import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ProfileView from '../ProfileView.vue'
import { useProfileStore } from '../../stores/profileStore'
import type { ProfileDetail } from '../../types'

// Router: Memory-history router with the profile routes.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/account/profile', component: ProfileView },
      { path: '/account/preferences', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + ToastService + stubbed pinia so mounted fetches are no-ops.
async function mountView() {
  const router = createTestRouter()
  await router.push('/account/profile')
  await router.isReady()
  const wrapper = mount(ProfileView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
    },
  })
  await flushPromises()
  return wrapper
}

// Fixture: Profile entity matching the ProfileDetail contract.
const profile: ProfileDetail = {
  id: 'p-1',
  userId: 'u-1',
  fullName: 'Alice Nguyen',
  firstName: 'Alice',
  lastName: 'Nguyen',
  email: 'alice@example.com',
  phoneNumber: null,
  dateOfBirth: null,
  preferences: null,
  notifications: null,
  emailConfirmed: true,
  phoneNumberConfirmed: false,
  createdAtUtc: '2026-01-01T00:00:00Z',
  modifiedAtUtc: null,
}

// Seed: Load the profile into the store and wait for the draft watcher.
async function seedProfile(wrapper: ReturnType<typeof mountView> extends Promise<infer T> ? T : never) {
  const profileStore = useProfileStore()
  profileStore.profile = profile
  await wrapper.vm.$nextTick()
  return profileStore
}

describe('ProfileView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the personal information rows and the email address', async () => {
    const wrapper = await mountView()
    await seedProfile(wrapper)

    expect(wrapper.text()).toContain('Personal Information')
    expect(wrapper.text()).toContain('First name')
    expect(wrapper.text()).toContain('Alice')
    expect(wrapper.text()).toContain('Nguyen')
    expect(wrapper.text()).toContain('alice@example.com')
  })

  it('edits a name inplace and saves it through profileStore.updateProfile', async () => {
    const wrapper = await mountView()
    const profileStore = await seedProfile(wrapper)

    const display = wrapper.findAll('[role="button"]').find(b => b.text() === 'Alice')
    await display!.trigger('click')
    await wrapper.vm.$nextTick()

    const input = wrapper.find('#profile-first-name')
    expect(input.exists()).toBe(true)
    await input.setValue('Alicia')
    await wrapper.findAll('button').find(b => b.text() === 'Save')!.trigger('click')
    await flushPromises()

    expect(profileStore.updateProfile).toHaveBeenCalledWith({
      firstName: 'Alicia',
      lastName: 'Nguyen',
      email: 'alice@example.com',
    })
  })

  it('shows the preferences summary and links to the preferences page', async () => {
    const wrapper = await mountView()
    await seedProfile(wrapper)

    expect(wrapper.text()).toContain('Preferences')
    expect(wrapper.text()).toContain('USD')
    const link = wrapper.findAll('a').find(a => a.text().includes('Edit preferences'))
    expect(link!.attributes('href')).toBe('/account/preferences')
  })
})
