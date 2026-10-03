import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { useNewsletter } from '../useNewsletter'

describe('useNewsletter', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    localStorage.getItem = vi.fn()
    localStorage.setItem = vi.fn()
    localStorage.removeItem = vi.fn()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('should initialize with default values', () => {
    const { email, isLoading, isSuccess, error } = useNewsletter()
    expect(email.value).toBe('')
    expect(isLoading.value).toBe(false)
    expect(isSuccess.value).toBe(false)
    expect(error.value).toBe('')
  })

  it('should return error when email is empty', async () => {
    const store = useNewsletter()
    store.email.value = ''
    
    await store.subscribe()
    
    expect(store.error.value).toBe('Please enter your email address')
    expect(store.isLoading.value).toBe(false)
  })

  it('should return error for invalid email', async () => {
    const store = useNewsletter()
    store.email.value = 'invalid'
    
    await store.subscribe()
    
    expect(store.error.value).toBe('Please enter a valid email address')
    expect(store.isLoading.value).toBe(false)
  })

  it('should succeed with valid email', async () => {
    const store = useNewsletter()
    store.email.value = 'test@example.com'
    
    const promise = store.subscribe()
    vi.advanceTimersByTime(800)
    await promise
    
    expect(store.isLoading.value).toBe(false)
    expect(store.isSuccess.value).toBe(true)
    expect(store.email.value).toBe('')
  })

  it('should reset all values', () => {
    const store = useNewsletter()
    store.email.value = 'test@example.com'
    store.isSuccess.value = true
    store.error.value = 'error'
    
    store.reset()
    
    expect(store.email.value).toBe('')
    expect(store.isSuccess.value).toBe(false)
    expect(store.error.value).toBe('')
  })
})
