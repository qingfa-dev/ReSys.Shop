import { describe, it, expect, vi, beforeEach } from 'vitest'
import { createTestingPinia } from '@pinia/testing'
import { setActivePinia } from 'pinia'
import { ok, failure } from '@/shared/types/result'
import { useCheckoutStore } from '../checkoutStore'
import { useCartStore } from '../cartStore'
import * as checkoutApi from '../../services/checkoutApi'

const mockedCheckoutApi = vi.mocked(checkoutApi)

vi.mock('@/features/ordering/services/checkoutApi', () => ({
  updateCheckout: vi.fn<(...args: unknown[]) => unknown>(),
  selectShippingRate: vi.fn<(...args: unknown[]) => unknown>(),
  validateCheckout: vi.fn<(...args: unknown[]) => unknown>(),
  createPaymentIntent: vi.fn<(...args: unknown[]) => unknown>(),
  placeOrder: vi.fn<(...args: unknown[]) => unknown>(),
}))

describe('checkoutStore', () => {
  beforeEach(() => {
    setActivePinia(createTestingPinia({ stubActions: false, createSpy: vi.fn }))
    vi.clearAllMocks()
  })

  describe('goToStep', () => {
    it('advances 1 -> 2 -> 3 without running the /cart/validate gate', async () => {
      const store = useCheckoutStore()

      await store.goToStep(2)
      expect(store.currentStep).toBe(2)

      await store.goToStep(3)
      expect(store.currentStep).toBe(3)

      expect(mockedCheckoutApi.validateCheckout).not.toHaveBeenCalled()
    })

    it('validates before advancing 3 -> 4 and advances on success', async () => {
      const store = useCheckoutStore()
      store.currentStep = 3
      mockedCheckoutApi.validateCheckout.mockResolvedValue(ok(null))

      await store.goToStep(4)

      expect(mockedCheckoutApi.validateCheckout).toHaveBeenCalledTimes(1)
      expect(store.currentStep).toBe(4)
      expect(store.error).toBeNull()
    })

    it('stays on the current step and clears loading when validation throws on 3 -> 4', async () => {
      const store = useCheckoutStore()
      store.currentStep = 3
      mockedCheckoutApi.validateCheckout.mockRejectedValue(new Error('network down'))

      await store.goToStep(4)

      expect(store.currentStep).toBe(3)
      expect(store.error).toBe('Please complete the current step first.')
      expect(store.loading).toBe(false)
    })

    it('stays on the current step when validation fails on 3 -> 4', async () => {
      const store = useCheckoutStore()
      store.currentStep = 3
      mockedCheckoutApi.validateCheckout.mockResolvedValue(
        failure({ code: 'Cart.Invalid', message: 'Checkout is incomplete', type: 422 }),
      )

      await store.goToStep(4)

      expect(store.currentStep).toBe(3)
      expect(store.error).toBe('Checkout is incomplete')
    })

    it('validates before advancing 4 -> 5', async () => {
      const store = useCheckoutStore()
      store.currentStep = 4
      mockedCheckoutApi.validateCheckout.mockResolvedValue(ok(null))

      await store.goToStep(5)

      expect(mockedCheckoutApi.validateCheckout).toHaveBeenCalledTimes(1)
      expect(store.currentStep).toBe(5)
    })

    it('does not validate on backward navigation', async () => {
      const store = useCheckoutStore()
      store.currentStep = 4

      await store.goToStep(2)

      expect(store.currentStep).toBe(2)
      expect(mockedCheckoutApi.validateCheckout).not.toHaveBeenCalled()
    })
  })

  describe('orchestration', () => {
    it('runs saveAddress -> calculateShipping -> createPaymentIntent -> placeOrder', async () => {
      const store = useCheckoutStore()
      const cart = useCartStore()
      cart.id = 'cart-1'
      mockedCheckoutApi.updateCheckout.mockResolvedValue(ok(null))
      mockedCheckoutApi.selectShippingRate.mockResolvedValue(ok(null))
      mockedCheckoutApi.createPaymentIntent.mockResolvedValue(
        ok({ id: 'pi-1', clientSecret: 'cs_secret', responseCode: null }),
      )
      mockedCheckoutApi.placeOrder.mockResolvedValue(ok({ id: 'order-1' }))

      const addressOk = await store.saveAddress('addr-1', 'u1@example.com')
      expect(addressOk).toBe(true)
      expect(store.shipAddressId).toBe('addr-1')
      expect(store.email).toBe('u1@example.com')
      expect(mockedCheckoutApi.updateCheckout).toHaveBeenCalledWith({
        shipAddressId: 'addr-1',
        billAddressId: 'addr-1',
        currency: 'VND',
        email: 'u1@example.com',
      })

      const shippingOk = await store.calculateShipping('method-1')
      expect(shippingOk).toBe(true)
      expect(store.shippingMethodId).toBe('method-1')
      expect(mockedCheckoutApi.selectShippingRate).toHaveBeenCalledWith({ shippingMethodId: 'method-1' })

      const clientSecret = await store.createPaymentIntent('pm-1', 100000)
      expect(clientSecret).toBe('cs_secret')
      expect(store.paymentIntentId).toBe('pi-1')
      expect(mockedCheckoutApi.createPaymentIntent).toHaveBeenCalledWith({
        orderId: 'cart-1',
        amount: 100000,
        currency: 'VND',
        paymentMethodId: 'pm-1',
      })

      const orderOk = await store.placeOrder()
      expect(orderOk).toBe(true)
      expect(store.orderId).toBe('order-1')
      expect(store.currentStep).toBe(5)
      expect(mockedCheckoutApi.placeOrder).toHaveBeenCalledWith({ paymentIntentId: 'pi-1' })
    })

    it('saveAddress sets error, clears loading, and returns false when the request throws', async () => {
      const store = useCheckoutStore()
      mockedCheckoutApi.updateCheckout.mockRejectedValue(new Error('network down'))

      const okResult = await store.saveAddress('addr-1', 'u1@example.com')

      expect(okResult).toBe(false)
      expect(store.error).toBe('Failed to save address')
      expect(store.loading).toBe(false)
    })

    it('placeOrder sets error, clears loading, and returns false when the request throws', async () => {
      const store = useCheckoutStore()
      store.paymentIntentId = 'pi-1'
      mockedCheckoutApi.placeOrder.mockRejectedValue(new Error('network down'))

      const okResult = await store.placeOrder()

      expect(okResult).toBe(false)
      expect(store.error).toBe('Failed to place order')
      expect(store.loading).toBe(false)
      expect(store.currentStep).not.toBe(5)
    })

    it('saveAddress sets error and returns false on failure', async () => {
      const store = useCheckoutStore()
      mockedCheckoutApi.updateCheckout.mockResolvedValue(
        failure({ code: 'Checkout.UpdateFailed', message: 'Address invalid', type: 400 }),
      )

      const okResult = await store.saveAddress('addr-1', 'u1@example.com')

      expect(okResult).toBe(false)
      expect(store.error).toBe('Address invalid')
    })

    it('calculateShipping sets error and returns false on failure', async () => {
      const store = useCheckoutStore()
      mockedCheckoutApi.selectShippingRate.mockResolvedValue(
        failure({ code: 'Checkout.ShippingFailed', message: 'No shipping method', type: 400 }),
      )

      const okResult = await store.calculateShipping('method-1')

      expect(okResult).toBe(false)
      expect(store.error).toBe('No shipping method')
    })

    it('createPaymentIntent returns null and errors when the cart is not loaded', async () => {
      const store = useCheckoutStore()

      const clientSecret = await store.createPaymentIntent('pm-1', 100000)

      expect(clientSecret).toBeNull()
      expect(store.error).toBe('Cart is not loaded.')
      expect(mockedCheckoutApi.createPaymentIntent).not.toHaveBeenCalled()
    })

    it('createPaymentIntent prefers responseCode over id', async () => {
      const store = useCheckoutStore()
      const cart = useCartStore()
      cart.id = 'cart-1'
      mockedCheckoutApi.createPaymentIntent.mockResolvedValue(
        ok({ id: 'pi-1', clientSecret: 'cs_secret', responseCode: 'pi-rc-1' }),
      )

      await store.createPaymentIntent('pm-1', 100000)

      expect(store.paymentIntentId).toBe('pi-rc-1')
    })

    it('createPaymentIntent sets error and returns null on failure', async () => {
      const store = useCheckoutStore()
      const cart = useCartStore()
      cart.id = 'cart-1'
      mockedCheckoutApi.createPaymentIntent.mockResolvedValue(
        failure({ code: 'Payment.IntentFailed', message: 'Stripe error', type: 400 }),
      )

      const clientSecret = await store.createPaymentIntent('pm-1', 100000)

      expect(clientSecret).toBeNull()
      expect(store.error).toBe('Stripe error')
    })

    it('placeOrder returns false without a payment intent', async () => {
      const store = useCheckoutStore()

      const okResult = await store.placeOrder()

      expect(okResult).toBe(false)
      expect(mockedCheckoutApi.placeOrder).not.toHaveBeenCalled()
    })

    it('placeOrder sets error and returns false on failure', async () => {
      const store = useCheckoutStore()
      store.paymentIntentId = 'pi-1'
      mockedCheckoutApi.placeOrder.mockResolvedValue(
        failure({ code: 'Order.PlaceFailed', message: 'Could not place order', type: 400 }),
      )

      const okResult = await store.placeOrder()

      expect(okResult).toBe(false)
      expect(store.error).toBe('Could not place order')
      expect(store.currentStep).not.toBe(5)
    })
  })
})
