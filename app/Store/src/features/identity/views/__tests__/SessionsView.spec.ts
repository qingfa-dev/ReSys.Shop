import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import { createTestingPinia } from '@pinia/testing'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import SessionsView from '../SessionsView.vue'
import { SessionApi } from '../../services/sessionApi'
import { ok } from '@/shared/types/result'
import type { SessionInfo } from '../../types'

// Confirm: Stub the service so revoke clicks can be observed and accepted inline.
type RequireOptions = { accept: () => void; close?: () => void }
const { requireMock } = vi.hoisted(() => ({
  requireMock: vi.fn<(options: RequireOptions) => void>(),
}))
vi.mock('primevue/useconfirm', () => ({
  useConfirm: () => ({ require: requireMock, close: vi.fn<() => void>() }),
}))

vi.mock('../../services/sessionApi', () => ({
  SessionApi: {
    getSessions: vi.fn<() => void>(),
    revokeAll: vi.fn<() => void>(),
    revokeCurrentDevice: vi.fn<() => void>(),
  },
}))

const mockedApi = vi.mocked(SessionApi)

const currentSession: SessionInfo = {
  id: 's1',
  deviceName: 'Chrome on Windows',
  ipAddress: '10.0.0.1',
  lastActivityAt: new Date().toISOString(),
  isCurrent: true,
}

const otherSession: SessionInfo = {
  id: 's2',
  deviceName: 'iPhone Safari',
  ipAddress: '10.0.0.2',
  lastActivityAt: new Date(Date.now() - 3_600_000).toISOString(),
  isCurrent: false,
}

// Router: Memory-history router with the sessions target route.
function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/account/sessions', component: SessionsView }],
  })
}

// Mount: PrimeVue + stubbed pinia; the confirm service and API are module-mocked.
async function mountView(sessions: SessionInfo[]) {
  mockedApi.getSessions.mockResolvedValue(ok(sessions))
  const router = createTestRouter()
  await router.push('/account/sessions')
  await router.isReady()
  const wrapper = mount(SessionsView, {
    global: {
      plugins: [PrimeVue, ToastService, createTestingPinia({ stubActions: true }), router],
      stubs: { ConfirmPopup: true },
    },
  })
  await flushPromises()
  return { wrapper, router }
}

describe('SessionsView', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders every session row from the API', async () => {
    const { wrapper } = await mountView([currentSession, otherSession])

    expect(wrapper.text()).toContain('Chrome on Windows')
    expect(wrapper.text()).toContain('iPhone Safari')
    expect(wrapper.text()).toContain('10.0.0.1')
    expect(wrapper.text()).toContain('10.0.0.2')
    expect(mockedApi.getSessions).toHaveBeenCalledTimes(1)
  })

  it('marks the current session with a "This device" tag', async () => {
    const { wrapper } = await mountView([currentSession, otherSession])

    expect(wrapper.find('[data-pc-name="tag"]').text()).toContain('This device')
  })

  it('disables revoke for the current session and enables it for others', async () => {
    const { wrapper } = await mountView([currentSession, otherSession])

    const buttons = wrapper.findAll('button')
    const currentRowButton = buttons.find(b => b.attributes('disabled') != null)
    expect(currentRowButton?.attributes('disabled')).toBeDefined()
    expect(buttons.some(b => b.attributes('disabled') == null)).toBe(true)
  })

  it('shows the confirm popup and revokes all other sessions on accept', async () => {
    mockedApi.revokeAll.mockResolvedValue(ok(undefined))
    const { wrapper } = await mountView([currentSession, otherSession])

    const revokeButtons = wrapper.findAll('button')
    const revokeButton = revokeButtons.find(b => b.attributes('disabled') == null)
    expect(revokeButton).toBeDefined()
    await revokeButton!.trigger('click')

    expect(requireMock).toHaveBeenCalledWith(
      expect.objectContaining({
        header: 'End session?',
        message: 'End all other active sessions, including iPhone Safari?',
      }),
    )

    const options = requireMock.mock.calls[0]![0]
    options.accept()
    await flushPromises()

    expect(mockedApi.revokeAll).toHaveBeenCalledTimes(1)
    expect(wrapper.text()).toContain('Chrome on Windows')
    expect(wrapper.text()).not.toContain('iPhone Safari')
  })

  it('shows the empty message when no sessions exist', async () => {
    const { wrapper } = await mountView([])

    expect(wrapper.find('[data-pc-name="message"]').text()).toContain('No active sessions found.')
  })

  it('adds no native interactive elements of its own', async () => {
    const { wrapper } = await mountView([currentSession, otherSession])

    expect(wrapper.findAll('input')).toHaveLength(0)
    expect(wrapper.findAll('select')).toHaveLength(0)
    expect(wrapper.findAll('textarea')).toHaveLength(0)
    expect(wrapper.findAll('button').length).toBeGreaterThan(0)
  })
})
