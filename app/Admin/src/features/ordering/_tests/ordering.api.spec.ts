import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/shared/api/http/api.client'
import { orderingApi } from '../services/ordering.api'

vi.mock('@/shared/api/http/api.client', () => ({
  default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() }
}))

describe('orderingApi', () => {
  it('list calls correct route', async () => {
    await orderingApi.orders.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/ordering/orders', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await orderingApi.orders.getById('ord-1')
    expect(apiClient.get).toHaveBeenCalledWith('api/ordering/orders/ord-1')
  })
  it('create calls correct route', async () => {
    await orderingApi.orders.create({ email: 'test@test.com', lineItems: [] })
    expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders', expect.any(Object))
  })
  it('update calls correct route', async () => {
    await orderingApi.orders.update('ord-1', { email: 'new@test.com' })
    expect(apiClient.put).toHaveBeenCalledWith('api/ordering/orders/ord-1', expect.any(Object))
  })
  it('delete calls correct route', async () => {
    await orderingApi.orders.delete('ord-1')
    expect(apiClient.delete).toHaveBeenCalledWith('api/ordering/orders/ord-1')
  })
  it('addLineItem calls correct route', async () => {
    await orderingApi.orders.addLineItem('ord-1', { variantId: 'v-1', quantity: 2 })
    expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/ord-1/line-items', { variantId: 'v-1', quantity: 2 })
  })
  it('updateLineItem calls correct route', async () => {
    await orderingApi.orders.updateLineItem('ord-1', 'li-1', { quantity: 3 })
    expect(apiClient.put).toHaveBeenCalledWith('api/ordering/orders/ord-1/line-items/li-1', { quantity: 3 })
  })
  it('removeLineItem calls correct route', async () => {
    await orderingApi.orders.removeLineItem('ord-1', 'li-1')
    expect(apiClient.delete).toHaveBeenCalledWith('api/ordering/orders/ord-1/line-items/li-1')
  })
  it('listLineItems calls correct route', async () => {
    await orderingApi.orders.listLineItems('ord-1')
    expect(apiClient.get).toHaveBeenCalledWith('api/ordering/orders/ord-1/line-items')
  })
  it('cancel calls correct route', async () => {
    await orderingApi.orders.cancel('ord-1', 'out of stock')
    expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/ord-1/cancel', { reason: 'out of stock' })
  })
  it('complete calls correct route', async () => {
    await orderingApi.orders.complete('ord-1')
    expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/ord-1/complete')
  })
  it('approve calls correct route', async () => {
    await orderingApi.orders.approve('ord-1')
    expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/ord-1/approve')
  })
  it('resume calls correct route', async () => {
    await orderingApi.orders.resume('ord-1')
    expect(apiClient.post).toHaveBeenCalledWith('api/ordering/orders/ord-1/resume')
  })
  it('updateStatus calls correct route', async () => {
    await orderingApi.orders.updateStatus('ord-1', 'Processing')
    expect(apiClient.put).toHaveBeenCalledWith('api/ordering/orders/ord-1/status', { status: 'Processing' })
  })
  it('updateShipAddress calls correct route', async () => {
    await orderingApi.orders.updateShipAddress('ord-1', { city: 'NYC' })
    expect(apiClient.put).toHaveBeenCalledWith('api/ordering/orders/ord-1/ship-address', { city: 'NYC' })
  })
  it('updateBillAddress calls correct route', async () => {
    await orderingApi.orders.updateBillAddress('ord-1', { city: 'LA' })
    expect(apiClient.put).toHaveBeenCalledWith('api/ordering/orders/ord-1/bill-address', { city: 'LA' })
  })
  it('updateShippingMethod calls correct route', async () => {
    await orderingApi.orders.updateShippingMethod('ord-1', 'sm-1')
    expect(apiClient.put).toHaveBeenCalledWith('api/ordering/orders/ord-1/shipping-method', { shippingMethodId: 'sm-1' })
  })
  it('fulfillments.getQueue calls correct route', async () => {
    await orderingApi.fulfillments.getQueue({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('api/ordering/orders', { params: { page: 1, state: 'Processing' } })
  })
})
