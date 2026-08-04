import { describe, it, expect, vi } from 'vitest'
import apiClient from '@/common/api/http/api.client'
import { orderRepository } from '../orders/api/order.api'
import { fulfillmentRepository } from '../fulfillment/api/fulfillment.api'

vi.mock('@/common/api/http/api.client', () => ({
  default: { get: vi.fn<() => void>().mockResolvedValue({ data: {} }), post: vi.fn<() => void>().mockResolvedValue({ data: {} }), put: vi.fn<() => void>().mockResolvedValue({ data: {} }), delete: vi.fn<() => void>().mockResolvedValue({ data: {} }) }
}))

describe('OrderRepository', () => {
  it('list calls correct route', async () => {
    await orderRepository.list({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('/ordering/orders', expect.any(Object))
  })
  it('getById calls correct route', async () => {
    await orderRepository.getById('ord-1')
    expect(apiClient.get).toHaveBeenCalledWith('/ordering/orders/ord-1')
  })
  it('create calls correct route', async () => {
    await orderRepository.create({ email: 'test@test.com', currency: 'USD', lineItems: [{ variantId: 'v-1', quantity: 1 }] })
    expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders', expect.any(Object))
  })
  it('update calls correct route', async () => {
    await orderRepository.update('ord-1', { email: 'new@test.com' })
    expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/ord-1', expect.any(Object))
  })
  it('delete calls correct route', async () => {
    await orderRepository.delete('ord-1')
    expect(apiClient.delete).toHaveBeenCalledWith('/ordering/orders/ord-1')
  })
  it('listLineItems calls correct route', async () => {
    await orderRepository.listLineItems('ord-1')
    expect(apiClient.get).toHaveBeenCalledWith('/ordering/orders/ord-1/line-items')
  })
  it('addLineItem calls correct route', async () => {
    await orderRepository.addLineItem('ord-1', { variantId: 'v-1', quantity: 2 })
    expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders/ord-1/line-items', { variantId: 'v-1', quantity: 2 })
  })
  it('updateLineItem calls correct route', async () => {
    await orderRepository.updateLineItem('ord-1', 'li-1', { quantity: 3 })
    expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/ord-1/line-items/li-1', { quantity: 3 })
  })
  it('removeLineItem calls correct route', async () => {
    await orderRepository.removeLineItem('ord-1', 'li-1')
    expect(apiClient.delete).toHaveBeenCalledWith('/ordering/orders/ord-1/line-items/li-1')
  })
  it('cancel calls correct route', async () => {
    await orderRepository.cancel('ord-1', { reason: 'out of stock' })
    expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders/ord-1/cancel', { reason: 'out of stock' })
  })
  it('complete calls correct route', async () => {
    await orderRepository.complete('ord-1')
    expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders/ord-1/complete')
  })
  it('approve calls correct route', async () => {
    await orderRepository.approve('ord-1')
    expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders/ord-1/approve')
  })
  it('resume calls correct route', async () => {
    await orderRepository.resume('ord-1')
    expect(apiClient.post).toHaveBeenCalledWith('/ordering/orders/ord-1/resume')
  })
  it('updateStatus calls correct route', async () => {
    await orderRepository.updateStatus('ord-1', { status: 'Processing' })
    expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/ord-1/status', { status: 'Processing' })
  })
  it('updateShipAddress calls correct route', async () => {
    await orderRepository.updateShipAddress('ord-1', { city: 'NYC' })
    expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/ord-1/ship-address', { city: 'NYC' })
  })
  it('updateBillAddress calls correct route', async () => {
    await orderRepository.updateBillAddress('ord-1', { city: 'LA' })
    expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/ord-1/bill-address', { city: 'LA' })
  })
  it('updateShippingMethod calls correct route', async () => {
    await orderRepository.updateShippingMethod('ord-1', { shippingMethodId: 'sm-1' })
    expect(apiClient.put).toHaveBeenCalledWith('/ordering/orders/ord-1/shipping-method', { shippingMethodId: 'sm-1' })
  })
})

describe('FulfillmentRepository', () => {
  it('getQueue calls correct route', async () => {
    await fulfillmentRepository.getQueue({ page: 1 })
    expect(apiClient.get).toHaveBeenCalledWith('/ordering/orders', { params: { page: 1, state: 'Processing' } })
  })
})
