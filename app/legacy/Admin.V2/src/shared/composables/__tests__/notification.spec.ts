import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useNotification } from '../useNotification'

const mockStartPolling = vi.fn()
const mockStopPolling = vi.fn()
const mockMarkRead = vi.fn()
const mockMarkAllRead = vi.fn()

vi.mock('@/stores/useNotificationStore', () => ({
  useNotificationStore: vi.fn(() => ({
    startPolling: mockStartPolling,
    stopPolling: mockStopPolling,
    markRead: mockMarkRead,
    markAllRead: mockMarkAllRead,
    fetch: vi.fn(),
    unreadCount: 5,
    recentItems: [],
    items: [],
  })),
}))

let mountedCb: (() => void) | null = null
let unmountedCb: (() => void) | null = null

vi.mock('vue', async (importOriginal) => {
  const original = (await importOriginal()) as Record<string, unknown>
  return {
    ...original,
    onMounted: vi.fn((cb: () => void) => {
      mountedCb = cb
    }),
    onUnmounted: vi.fn((cb: () => void) => {
      unmountedCb = cb
    }),
  }
})

describe('useNotification', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mountedCb = null
    unmountedCb = null
  })

  it('calls store.startPolling(30000) on mount', () => {
    useNotification()
    expect(mountedCb).not.toBeNull()
    mountedCb!()
    expect(mockStartPolling).toHaveBeenCalledWith(30000)
  })

  it('calls store.stopPolling() on unmount', () => {
    useNotification()
    expect(unmountedCb).not.toBeNull()
    unmountedCb!()
    expect(mockStopPolling).toHaveBeenCalled()
  })

  it('returns store with markRead and markAllRead', () => {
    const store = useNotification()
    store.markRead('n1')
    expect(mockMarkRead).toHaveBeenCalledWith('n1')
    store.markAllRead()
    expect(mockMarkAllRead).toHaveBeenCalled()
  })

})
