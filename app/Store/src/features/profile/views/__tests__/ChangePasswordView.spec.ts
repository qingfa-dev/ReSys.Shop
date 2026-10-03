import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ChangePasswordView from '../ChangePasswordView.vue'
import { useAuthStore } from '@/features/identity/stores/authStore'

// Router: Memory-history router with the account routes.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/account/change-password', component: ChangePasswordView },
      { path: '/account/profile', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + stubbed pinia so the submit flow stays client-side.
async function mountView() {
  const router = createTestRouter()
  await router.push('/account/change-password')
  await router.isReady()
  const wrapper = mount(ChangePasswordView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
    },
  })
  await flushPromises()
  return { wrapper, router }
}

describe('ChangePasswordView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the three password fields and the submit control', async () => {
    const { wrapper } = await mountView()

    expect(wrapper.text()).toContain('Current password')
    expect(wrapper.text()).toContain('New password')
    expect(wrapper.text()).toContain('Confirm new password')
    expect(wrapper.findAll('input')).toHaveLength(3)
    expect(wrapper.findAll('button')).toHaveLength(1)
  })

  it('shows validation messages and skips the store on invalid submit', async () => {
    const { wrapper } = await mountView()

    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    const auth = useAuthStore()
    expect(auth.changePassword).not.toHaveBeenCalled()
    expect(wrapper.findAll('[data-pc-name="message"]').length).toBeGreaterThan(0)
  })

  it('rejects a mismatched confirmation password without calling the store', async () => {
    const { wrapper } = await mountView()

    await wrapper.find('#currentPassword').setValue('old-secret')
    await wrapper.find('#newPassword').setValue('NewSecret123!')
    await wrapper.find('#confirmPassword').setValue('Different456!')
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    const auth = useAuthStore()
    expect(auth.changePassword).not.toHaveBeenCalled()
    expect(wrapper.text()).toContain('Passwords do not match')
  })

  it('calls authStore.changePassword and shows the success state on valid submit', async () => {
    const { wrapper } = await mountView()
    const auth = useAuthStore()
    vi.mocked(auth.changePassword).mockResolvedValue(true)

    await wrapper.find('#currentPassword').setValue('old-secret')
    await wrapper.find('#newPassword').setValue('NewSecret123!')
    await wrapper.find('#confirmPassword').setValue('NewSecret123!')
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    expect(auth.changePassword).toHaveBeenCalledWith('old-secret', 'NewSecret123!')
    expect(wrapper.text()).toContain('Password changed successfully.')
    expect(wrapper.text()).toContain('Back to Profile')
  })

  it('shows the API error message when the store rejects the change', async () => {
    const { wrapper } = await mountView()
    const auth = useAuthStore()
    vi.mocked(auth.changePassword).mockResolvedValue(false)
    auth.error = 'Current password is incorrect'

    await wrapper.find('#currentPassword').setValue('wrong-secret')
    await wrapper.find('#newPassword').setValue('NewSecret123!')
    await wrapper.find('#confirmPassword').setValue('NewSecret123!')
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Current password is incorrect')
    expect(wrapper.text()).not.toContain('Password changed successfully.')
  })
})
