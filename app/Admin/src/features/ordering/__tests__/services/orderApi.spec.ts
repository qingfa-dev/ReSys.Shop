import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet, mockPut, mockDel, mockGetPaged } = vi.hoisted(() => ({
  mockPost: vi.fn<(...args: unknown[]) => unknown>(),
  mockGet: vi.fn<(...args: unknown[]) => unknown>(),
  mockPut: vi.fn<(...args: unknown[]) => unknown>(),
  mockDel: vi.fn<(...args: unknown[]) => unknown>(),
  mockGetPaged: vi.fn<(...args: unknown[]) => unknown>(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
  put: mockPut,
  del: mockDel,
}))

vi.mock('@/shared/api', () => ({
  getPaged: mockGetPaged,
}))

import { OrderApi } from '../../services/orderApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('OrderApi.getOrders', () => {
  it('calls getPaged with order query params and allowed fields', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await OrderApi.getOrders({
      filter: 'status=Placed,currency=USD',
      sort: ['-createdAtUtc'],
      pageNumber: 1,
      pageSize: 10,
    })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/ordering/orders',
      {
        filter: 'status=Placed,currency=USD',
        sort: ['-createdAtUtc'],
        pageNumber: 1,
        pageSize: 10,
      },
      expect.objectContaining({
        allowedFilterFields: ['status', 'checkoutState', 'currency', 'userId', 'isDeleted'],
        allowedSortFields: ['number', 'total', 'completedAtUtc', 'createdAtUtc', 'status'],
        allowedSearchFields: ['number', 'email'],
      }),
    )
  })
})

describe('OrderApi.getOrder', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.getOrder('o-1')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1')
  })
})

describe('OrderApi.createOrder', () => {
  it('calls POST with request body', async () => {
    const req = { currency: 'USD', email: 'a@b.com' }
    mockPost.mockResolvedValue({ value: { id: 'o-1', ...req }, isSuccess: true, statusCode: 201, message: null, errors: [], metadata: null })
    await OrderApi.createOrder(req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/ordering/orders', req)
  })
})


describe('OrderApi.updateOrder', () => {
  it('calls PUT with request body', async () => {
    const req = { currency: 'USD', specialInstructions: 'leave at door' }
    mockPut.mockResolvedValue({ value: { id: 'o-1', ...req }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.updateOrder('o-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1', req)
  })
})

describe('OrderApi.deleteOrder', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await OrderApi.deleteOrder('o-1')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1')
  })
})

describe('OrderApi.getLineItems', () => {
  it('calls getPaged with line-items URL and allowed fields', async () => {
    mockGetPaged.mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    await OrderApi.getLineItems('o-1', { sort: ['-total'] })

    expect(mockGetPaged).toHaveBeenCalledWith(
      '/api/admin/ordering/orders/o-1/line-items',
      { sort: ['-total'] },
      expect.objectContaining({
        allowedFilterFields: ['OrderId', 'VariantId'],
        allowedSortFields: ['Quantity', 'Price', 'Total', 'CreatedAtUtc'],
        allowedSearchFields: [],
      }),
    )
  })
})

describe('OrderApi.getLineItem', () => {
  it('calls GET with correct URL', async () => {
    mockGet.mockResolvedValue({ value: { id: 'li-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.getLineItem('o-1', 'li-1')
    expect(mockGet).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/line-items/li-1')
  })
})

describe('OrderApi.addLineItem', () => {
  it('calls POST with request body', async () => {
    const req = { variantId: 'v-1', quantity: 2, price: 19.99 }
    mockPost.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.addLineItem('o-1', req)
    expect(mockPost).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/line-items', req)
  })
})

describe('OrderApi.updateLineItem', () => {
  it('calls PUT with request body', async () => {
    const req = { quantity: 3 }
    mockPut.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.updateLineItem('o-1', 'li-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/line-items/li-1', req)
  })
})

describe('OrderApi.removeLineItem', () => {
  it('calls DELETE with correct URL', async () => {
    mockDel.mockResolvedValue({ value: null, isSuccess: true, statusCode: 204, message: null, errors: [], metadata: null })
    await OrderApi.removeLineItem('o-1', 'li-1')
    expect(mockDel).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/line-items/li-1')
  })
})

describe('OrderApi.cancelOrder', () => {
  it('calls POST with cancel URL and reason body', async () => {
    mockPost.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.cancelOrder('o-1', { reason: 'x' })
    expect(mockPost).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/cancel', { reason: 'x' })
  })

  it('calls POST with empty body when no reason given', async () => {
    mockPost.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.cancelOrder('o-1')
    expect(mockPost).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/cancel', {})
  })
})

describe('OrderApi.completeOrder', () => {
  it('calls POST with complete URL', async () => {
    mockPost.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.completeOrder('o-1')
    expect(mockPost).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/complete')
  })
})

describe('OrderApi.approveOrder', () => {
  it('calls POST with approve URL', async () => {
    mockPost.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.approveOrder('o-1')
    expect(mockPost).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/approve')
  })
})

describe('OrderApi.resumeOrder', () => {
  it('calls POST with resume URL', async () => {
    mockPost.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.resumeOrder('o-1')
    expect(mockPost).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/resume')
  })
})

describe('OrderApi.updateShipAddress', () => {
  it('calls PUT with ship-address URL and request body', async () => {
    const req = { addressId: 'a-1' }
    mockPut.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.updateShipAddress('o-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/ship-address', req)
  })
})

describe('OrderApi.updateBillAddress', () => {
  it('calls PUT with bill-address URL and request body', async () => {
    const req = { addressId: 'a-2' }
    mockPut.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.updateBillAddress('o-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/bill-address', req)
  })
})

describe('OrderApi.updateShippingMethod', () => {
  it('calls PUT with shipping-method URL and request body', async () => {
    const req = { shippingMethodId: 'sm-1' }
    mockPut.mockResolvedValue({ value: { id: 'o-1' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.updateShippingMethod('o-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/shipping-method', req)
  })
})

describe('OrderApi.updateStatus', () => {
  it('calls PUT with status URL and request body', async () => {
    const req = { status: 'Placed' }
    mockPut.mockResolvedValue({ value: null, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.updateStatus('o-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/ordering/orders/o-1/status', req)
  })
})

describe('OrderApi.updateShipmentStatus', () => {
  it('calls PUT with shipment status URL and request body', async () => {
    const req = { status: 'Shipped', trackingNumber: 'TRK-123' }
    mockPut.mockResolvedValue({ value: { id: 's-1', status: 'Shipped' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.updateShipmentStatus('s-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/shipping/shipments/s-1/status', req)
  })

  it('calls PUT with status only when tracking number is omitted', async () => {
    const req = { status: 'Ready' }
    mockPut.mockResolvedValue({ value: { id: 's-1', status: 'Ready' }, isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null })
    await OrderApi.updateShipmentStatus('s-1', req)
    expect(mockPut).toHaveBeenCalledWith('/api/admin/shipping/shipments/s-1/status', req)
  })
})
