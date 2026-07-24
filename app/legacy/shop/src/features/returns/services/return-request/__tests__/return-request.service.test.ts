import { describe, it, expect } from 'vitest'
import { returnRequestService } from '../return-request.service'

import type { CreateReturnRequest } from '../../../types'

describe('ReturnRequestService', () => {
  describe('createReturnRequest', () => {
    it('should create return request', async () => {
      const result = await returnRequestService.createReturnRequest({ orderId: 'order-1', items: [], reason: 'damaged' } as CreateReturnRequest)
      expect(result).toBeDefined()
    })
  })

  describe('getReturnRequest', () => {
    it('should return return request', async () => {
      const result = await returnRequestService.getReturnRequest('ret-1')
      expect(result).toBeDefined()
    })
  })

  describe('getReturnRequestsByOrder', () => {
    it('should return return requests by order', async () => {
      const result = await returnRequestService.getReturnRequestsByOrder('order-1')
      expect(result).toBeDefined()
    })
  })

  describe('cancelReturnRequest', () => {
    it('should cancel return request', async () => {
      const result = await returnRequestService.cancelReturnRequest('ret-1')
      expect(result).toBeDefined()
    })
  })

  describe('getReturnLabels', () => {
    it('should return return labels', async () => {
      const result = await returnRequestService.getReturnLabels('ret-1')
      expect(result).toBeDefined()
    })
  })
})