import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ResetPasswordView from '../ResetPasswordView.vue'
import { useAuthStore } from '../../stores/authStore'

// Router: Memory-history router carrying the emailed reset token as a query.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/reset-password', component: ResetPasswordView },
      { path: '/login', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + stubbed pinia so the submit flow stays client-side.
async function mountView() {
  const router = createTestRouter()
  await router.push('/reset-password?token=tok-1')
  await router.isReady()
  const wrapper = mount(ResetPasswordView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
    },
  })
  await flushPromises()
  return { wrapper, router }
}

describe('ResetPasswordView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders the new and confirm password fields', async () => {
    const { wrapper } = await mountView()

    expect(wrapper.text()).toContain('New password')
    expect(wrapper.text()).toContain('Confirm password')
    expect(wrapper.findAll('input')).toHaveLength(2)
    expect(wrapper.findAll('button')).toHaveLength(1)
  })

  it('rejects a mismatched confirmation password without calling the store', async () => {
    const { wrapper } = await mountView()

    await wrapper.find('#newPassword').setValue('NewSecret123!')
    await wrapper.find('#confirmPassword').setValue('Different456!')
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    const auth = useAuthStore()
    expect(auth.resetPassword).not.toHaveBeenCalled()
    expect(wrapper.text()).toContain('Passwords do not match')
  })

  it('shows validation messages and skips the store on invalid submit', async () => {
    const { wrapper } = await mountView()

    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    const auth = useAuthStore()
    expect(auth.resetPassword).not.toHaveBeenCalled()
    expect(wrapper.findAll('[data-pc-name="message"]').length).toBeGreaterThan(0)
  })

  it('calls authStore.resetPassword and shows the success state on valid submit', async () => {
    const { wrapper } = await mountView()
    const auth = useAuthStore()
    vi.mocked(auth.resetPassword).mockResolvedValue(true)

    await wrapper.find('#newPassword').setValue('NewSecret123!')
    await wrapper.find('#confirmPassword').setValue('NewSecret123!')
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    expect(auth.resetPassword).toHaveBeenCalledWith('tok-1', 'NewSecret123!')
    expect(wrapper.text()).toContain('Password reset successfully.')
    expect(wrapper.text()).toContain('Back to Sign In')
  })

  it('shows the API error message when the store rejects the reset', async () => {
    const { wrapper } = await mountView()
    const auth = useAuthStore()
    vi.mocked(auth.resetPassword).mockResolvedValue(false)
    auth.error = 'Invalid or expired reset token'

    await wrapper.find('#newPassword').setValue('NewSecret123!')
    await wrapper.find('#confirmPassword').setValue('NewSecret123!')
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Invalid or expired reset token')
    expect(wrapper.text()).not.toContain('Password reset successfully.')
  })
})
