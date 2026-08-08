import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createTestingPinia } from '@pinia/testing'
import { setActivePinia } from 'pinia'
import { ok, failure } from '@/shared/types/result'
import { useCheckoutStore } from '../checkoutStore'
import * as checkoutApi from '../../services/checkoutApi'

const mockedCheckoutApi = vi.mocked(checkoutApi) as any

const mockCartStore = {
  id: 'cart-1',
  items: [],
  isEmpty: false,
  fetchCart: vi.fn<() => Promise<void>>(),
}

vi.mock('@/features/ordering/stores/cartStore', () => ({
  useCartStore: vi.fn<() => typeof mockCartStore>(() => mockCartStore),
}))

vi.mock('vue-router', () => ({
  useRouter: vi.fn<() => { push: () => void }>(() => ({ push: vi.fn<() => void>() })),
}))

vi.mock('@/features/ordering/services/checkoutApi', () => ({
  CheckoutApi: {
    updateCheckout: vi.fn<() => Promise<void>>(),
    selectShippingRate: vi.fn<() => Promise<void>>(),
    validateCheckout: vi.fn<() => Promise<void>>(),
    createPaymentIntent: vi.fn<() => Promise<void>>(),
    placeOrder: vi.fn<() => Promise<void>>(),
  },
}))

describe('checkoutStore', () => {
  beforeEach(() => {
    setActivePinia(createTestingPinia({ stubActions: false, createSpy: vi.fn }))
    vi.clearAllMocks()
    mockCartStore.id = 'cart-1'
    mockCartStore.items = []
    mockCartStore.isEmpty = false
  })

  describe('orchestration', () => {
    it('runs saveAddress -> selectShippingRate -> createPaymentIntent -> placeOrder', async () => {
      const store = useCheckoutStore()
      mockedCheckoutApi.CheckoutApi.updateCheckout.mockResolvedValue(ok(undefined))
      mockedCheckoutApi.CheckoutApi.selectShippingRate.mockResolvedValue(ok(undefined))
      mockedCheckoutApi.CheckoutApi.createPaymentIntent.mockResolvedValue(
        ok({ id: 'pi-1', clientSecret: 'cs_secret', responseCode: null }),
      )
      mockedCheckoutApi.CheckoutApi.placeOrder.mockResolvedValue(ok({ id: 'order-1' }))

      const addressOk = await store.saveAddress('addr-1', 'u1@example.com')
      expect(addressOk).toBe(true)
      expect(store.shipAddressId).toBe('addr-1')
      expect(store.email).toBe('u1@example.com')
      expect(mockedCheckoutApi.CheckoutApi.updateCheckout).toHaveBeenCalledWith({
        shipAddressId: 'addr-1',
        billAddressId: 'addr-1',
        email: 'u1@example.com',
      })

      const shippingOk = await store.selectShippingRate('method-1')
      expect(shippingOk).toBe(true)
      expect(mockedCheckoutApi.CheckoutApi.selectShippingRate).toHaveBeenCalledWith({ shippingMethodId: 'method-1' })

      const paymentOk = await store.createPaymentIntent('pm-1')
      expect(paymentOk).toBe(true)
      expect(store.paymentIntentId).toBe('pi-1')
      expect(mockedCheckoutApi.CheckoutApi.createPaymentIntent).toHaveBeenCalledWith({
        orderId: 'cart-1',
        paymentMethodId: 'pm-1',
      })

      const orderOk = await store.placeOrder()
      expect(orderOk).toBe(true)
      expect(store.orderId).toBe('order-1')
      expect(store.currentStep).toBe(5)
      expect(mockedCheckoutApi.CheckoutApi.placeOrder).toHaveBeenCalledWith({ paymentIntentId: 'pi-1' })
    })

    it('saveAddress sets error and returns false on failure', async () => {
      const store = useCheckoutStore()
      mockedCheckoutApi.CheckoutApi.updateCheckout.mockResolvedValue(
        failure({ code: 'Checkout.UpdateFailed', message: 'Address invalid', type: 400 }),
      )

      const okResult = await store.saveAddress('addr-1', 'u1@example.com')

      expect(okResult).toBe(false)
      expect(store.error).toBe('Address invalid')
    })

    it('saveAddress sets error, clears loading, and returns false when the request throws', async () => {
      const store = useCheckoutStore()
      mockedCheckoutApi.CheckoutApi.updateCheckout.mockRejectedValue(new Error('network down'))

      const okResult = await store.saveAddress('addr-1', 'u1@example.com')

      expect(okResult).toBe(false)
      expect(store.error).toBe('Failed to save address')
      expect(store.loading).toBe(false)
    })

    it('selectShippingRate sets error and returns false on failure', async () => {
      const store = useCheckoutStore()
      mockedCheckoutApi.CheckoutApi.selectShippingRate.mockResolvedValue(
        failure({ code: 'Checkout.ShippingFailed', message: 'No shipping method', type: 400 }),
      )

      const okResult = await store.selectShippingRate('method-1')

      expect(okResult).toBe(false)
      expect(store.error).toBe('No shipping method')
    })

    it('createPaymentIntent returns false on failure', async () => {
      const store = useCheckoutStore()
      mockedCheckoutApi.CheckoutApi.createPaymentIntent.mockResolvedValue(
        failure({ code: 'Payment.IntentFailed', message: 'Stripe error', type: 400 }),
      )

      const result = await store.createPaymentIntent('pm-1')

      expect(result).toBe(false)
      expect(store.error).toBe('Stripe error')
    })

    it('createPaymentIntent prefers responseCode over id', async () => {
      const store = useCheckoutStore()
      mockedCheckoutApi.CheckoutApi.createPaymentIntent.mockResolvedValue(
        ok({ id: 'pi-1', clientSecret: 'cs_secret', responseCode: 'pi-rc-1' }),
      )

      await store.createPaymentIntent('pm-1')

      expect(store.paymentIntentId).toBe('pi-rc-1')
    })

    it('placeOrder sets error and returns false on failure', async () => {
      const store = useCheckoutStore()
      store.paymentIntentId = 'pi-1'
      mockedCheckoutApi.CheckoutApi.placeOrder.mockResolvedValue(
        failure({ code: 'Order.PlaceFailed', message: 'Could not place order', type: 400 }),
      )

      const okResult = await store.placeOrder()

      expect(okResult).toBe(false)
      expect(store.error).toBe('Could not place order')
      expect(store.currentStep).not.toBe(5)
    })

    it('placeOrder sets error, clears loading, and returns false when the request throws', async () => {
      const store = useCheckoutStore()
      store.paymentIntentId = 'pi-1'
      mockedCheckoutApi.CheckoutApi.placeOrder.mockRejectedValue(new Error('network down'))

      const okResult = await store.placeOrder()

      expect(okResult).toBe(false)
      expect(store.error).toBe('Failed to place order')
      expect(store.loading).toBe(false)
      expect(store.currentStep).not.toBe(5)
    })

    it('placeOrder returns false without a payment intent', async () => {
      const store = useCheckoutStore()

      const okResult = await store.placeOrder()

      expect(okResult).toBe(false)
      expect(mockedCheckoutApi.CheckoutApi.placeOrder).not.toHaveBeenCalled()
    })
  })
})
