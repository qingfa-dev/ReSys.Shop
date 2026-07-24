import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';
import type { 
  OrderListItem, 
  OrderDetail, 
  OrderSearchParams, 
  CreateOrderRequest, 
  AddOrderItemRequest, 
  UpdateAddressesRequest,
  CancelOrderRequest,
  CreateShipmentRequest,
  RefundPaymentRequest
} from '../types/order.types';

export const orderService = {
  async list(params: OrderSearchParams): Promise<ApiResult<OrderListItem[]>> {
    return apiClient.get('/admin/orders', { params });
  },

  async getById(id: string): Promise<ApiResult<OrderDetail>> {
    return apiClient.get(`/admin/orders/${id}`);
  },

  async create(data: CreateOrderRequest): Promise<ApiResult<OrderDetail>> {
    return apiClient.post('/admin/orders', data);
  },

  async createShipment(orderId: string, data: CreateShipmentRequest): Promise<ApiResult<void>> {
    return apiClient.post(`/admin/orders/${orderId}/shipments`, data);
  },

  async cancelShipment(orderId: string, shipmentId: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/admin/orders/${orderId}/shipments/${shipmentId}`);
  },

  async addItem(id: string, data: AddOrderItemRequest): Promise<ApiResult<void>> {
    return apiClient.post(`/admin/orders/${id}/items`, data);
  },

  async updateAddresses(id: string, data: UpdateAddressesRequest): Promise<ApiResult<void>> {
    return apiClient.put(`/admin/orders/${id}/addresses`, data);
  },

  async updateState(id: string): Promise<ApiResult<void>> {
    return apiClient.post(`/admin/orders/${id}/advance`);
  },

  async cancelOrder(id: string, reason?: string): Promise<ApiResult<void>> {
    const data: CancelOrderRequest = { reason };
    return apiClient.post(`/admin/orders/${id}/cancel`, data);
  },

  async refundPayment(orderId: string, paymentId: string, data: RefundPaymentRequest): Promise<ApiResult<void>> {
    return apiClient.post(`/admin/orders/${orderId}/payments/${paymentId}/refund`, data);
  }
};