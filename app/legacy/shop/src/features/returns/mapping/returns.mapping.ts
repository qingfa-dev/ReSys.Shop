import type { ReturnRequest, ReturnRequestSchemaType } from '../types'

export function toReturnRequest(schema: ReturnRequestSchemaType): ReturnRequest {
  return {
    id: schema.id,
    orderId: schema.orderId,
    status: schema.status,
    items: schema.items,
    refundAmount: schema.refundAmount,
    refundMethod: schema.refundMethod,
    trackingNumber: schema.trackingNumber,
    createdAt: schema.createdAt,
    updatedAt: schema.updatedAt,
  }
}

export function fromReturnRequest(returnRequest: ReturnRequest): ReturnRequestSchemaType {
  return ReturnRequestSchema.parse(returnRequest)
}

export function isReturnPending(returnRequest: ReturnRequest): boolean {
  return returnRequest.status === 'pending'
}

export function isReturnApproved(returnRequest: ReturnRequest): boolean {
  return returnRequest.status === 'approved'
}

export function isReturnRefunded(returnRequest: ReturnRequest): boolean {
  return returnRequest.status === 'refunded'
}

export function getReturnStatusLabel(status: ReturnRequest['status']): string {
  const labels: Record<ReturnRequest['status'], string> = {
    pending: 'Pending Review',
    approved: 'Approved',
    rejected: 'Rejected',
    received: 'Received',
    refunded: 'Refunded',
  }
  return labels[status]
}

import { ReturnRequestSchema } from '../types/schemas'