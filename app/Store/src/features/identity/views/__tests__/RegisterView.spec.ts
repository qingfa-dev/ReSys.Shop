import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises, type VueWrapper } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import RegisterView from '../RegisterView.vue'
import { useAuthStore } from '../../stores/authStore'

// Router: Memory-history router with the register target and redirect destinations.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/login', component: { template: '<div />' } },
      { path: '/register', component: RegisterView },
      { path: '/terms', component: { template: '<div />' } },
    ],
  })
}

// Mount: PrimeVue + stubbed pinia so the submit flow stays client-side.
async function mountView() {
  const router = createTestRouter()
  await router.push('/register')
  await router.isReady()
  const wrapper = mount(RegisterView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
    },
  })
  await flushPromises()
  return { wrapper, router }
}

// Fill: Complete a valid registration form and agree to the terms.
async function fillValidForm(wrapper: VueWrapper) {
  await wrapper.find('#firstName').setValue('Alice')
  await wrapper.find('#lastName').setValue('Example')
  await wrapper.find('#email').setValue('alice@example.com')
  await wrapper.find('#password').setValue('Sup3rsecret!')
  await wrapper.find('#confirmPassword').setValue('Sup3rsecret!')
  await wrapper.find('[data-pc-name="checkbox"] input').trigger('change')
  // Settle: Flush the checkbox v-model before submit so the terms rule passes.
  await flushPromises()
}

describe('RegisterView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders all registration fields and the terms consent', async () => {
    const { wrapper } = await mountView()

    expect(wrapper.text()).toContain('First name')
    expect(wrapper.text()).toContain('Last name')
    expect(wrapper.text()).toContain('Email')
    expect(wrapper.text()).toContain('Password')
    expect(wrapper.text()).toContain('Confirm password')
    expect(wrapper.text()).toContain('Terms of Service')
    expect(wrapper.text()).toContain('Create Account')
    expect(wrapper.text()).toContain('Sign in')
    expect(wrapper.findAll('[data-pc-name="inputtext"]')).toHaveLength(3)
    expect(wrapper.findAll('[data-pc-name="pcinputtext"]')).toHaveLength(2)
    expect(wrapper.findAll('[data-pc-name="checkbox"]')).toHaveLength(1)
  })

  it('adds no native interactive elements of its own', async () => {
    const { wrapper } = await mountView()

    // Inputs and the submit control come only from PrimeVue: 5 text inputs + 1 checkbox input.
    expect(wrapper.findAll('input')).toHaveLength(6)
    expect(wrapper.findAll('button')).toHaveLength(1)
  })

  it('shows validation messages and skips the store on invalid submit', async () => {
    const { wrapper } = await mountView()

    await wrapper.find('form').trigger('submit')
    await flushPromises()

    const auth = useAuthStore()
    expect(auth.register).not.toHaveBeenCalled()
    expect(wrapper.findAll('[data-pc-name="message"]').length).toBeGreaterThan(2)
  })

  it('calls authStore.register and shows the success state on a valid submit', async () => {
    const { wrapper } = await mountView()
    const auth = useAuthStore()
    vi.mocked(auth.register).mockResolvedValue(true)

    await fillValidForm(wrapper)
    await wrapper.find('form').trigger('submit')
    await flushPromises()
    // Settle: Flush a macrotask so vee-validate resolves its async validation.
    await new Promise((r) => setTimeout(r, 0))
    await wrapper.vm.$nextTick()

    expect(auth.register).toHaveBeenCalledWith({
      email: 'alice@example.com',
      userName: 'alice',
      password: 'Sup3rsecret!',
      firstName: 'Alice',
      lastName: 'Example',
      acceptTerm: true,
    })
    expect(wrapper.find('form').exists()).toBe(false)
    expect(wrapper.text()).toContain('Account created. Please sign in.')
  })
})
