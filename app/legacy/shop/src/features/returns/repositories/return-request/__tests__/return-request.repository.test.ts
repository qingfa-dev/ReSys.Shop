import { describe, it, expect, beforeEach } from 'vitest'
import { mockReturnRequestRepository, MockReturnRequestRepository } from '../return-request.mock.repository'

describe('ReturnRequestRepository', () => {
  beforeEach(() => {
    MockReturnRequestRepository.reset()
  })

  describe('getAll', () => {
    it('should return return requests for order', async () => {
      const result = await mockReturnRequestRepository.getAll('order-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(1)
    })

    it('should return empty array for order with no returns', async () => {
      const result = await mockReturnRequestRepository.getAll('order-nonexistent')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toHaveLength(0)
    })
  })

  describe('getById', () => {
    it('should return return request by id', async () => {
      const result = await mockReturnRequestRepository.getById('return-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBe('return-1')
    })

    it('should return error for non-existent id', async () => {
      const result = await mockReturnRequestRepository.getById('invalid-id')
      expect(result.isFailure).toBe(true)
      expect(result.statusCode).toBe(404)
    })

    it('should support generic type', async () => {
      const result = await mockReturnRequestRepository.getById('return-1')
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('create', () => {
    it('should create new return request', async () => {
      const request = { orderId: 'order-3', items: [{ orderItemId: 'item-3', quantity: 1, reason: 'not-as-described' }] }
      const result = await mockReturnRequestRepository.create(request)
      expect(result.isSuccess).toBe(true)
      expect(result.data?.id).toBeDefined()
      expect(result.data?.status).toBe('pending')
    })
  })

  describe('cancel', () => {
    it('should cancel return request', async () => {
      const result = await mockReturnRequestRepository.cancel('return-1')
      expect(result.isSuccess).toBe(true)
    })

    it('should return error for non-existent return', async () => {
      const result = await mockReturnRequestRepository.cancel('invalid-id')
      expect(result.isFailure).toBe(true)
    })
  })

  describe('getLabels', () => {
    it('should return label URL', async () => {
      const result = await mockReturnRequestRepository.getLabels('return-1')
      expect(result.isSuccess).toBe(true)
      expect(result.data).toContain('return-1')
    })

    it('should return error for non-existent return', async () => {
      const result = await mockReturnRequestRepository.getLabels('invalid-id')
      expect(result.isFailure).toBe(true)
    })
  })
})