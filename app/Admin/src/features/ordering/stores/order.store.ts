import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { usePagedList } from '@/shared/composables/paged-list.use';
import { orderService } from '../services/order.service';
import type { ServerResult } from '@/shared/api/types/result.types';
import type { OrderListItem, OrderDetail } from '../types/order.domain.types';
import type { OrderSearchParams, CreateOrderRequest, AddOrderItemRequest, UpdateAddressesRequest } from '../types/order.request.types';

export const useOrderStore = defineStore('order', () => {
  const { showToast } = useToast();

  const current_order = ref<OrderDetail | null>(null);
  const submitting = ref(false);

  const { items: orders, totalRecords, params: query, fetch: fetchOrders, loading, error } = usePagedList<OrderListItem, OrderSearchParams>(
    (p) => orderService.list(p),
    { page: 1, pageSize: 10, search: '', state: '', sort: ['-createdAtUtc'] },
  );

  async function fetchOrderById(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const result = await orderService.getById(id);
      if (result.isSuccess) {
        current_order.value = result.value;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function createOrder(data: CreateOrderRequest) {
    submitting.value = true;
    try {
      const result = await orderService.create(data);
      if (result.isSuccess) {
        showToast('success', 'Success', 'Order created successfully');
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function addOrderItem(id: string, data: AddOrderItemRequest) {
    submitting.value = true;
    try {
      const result = await orderService.addItem(id, data);
      if (result.isSuccess) {
        showToast('success', 'Success', 'Item added to order');
        await fetchOrderById(id);
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function updateOrderAddresses(id: string, data: UpdateAddressesRequest) {
    submitting.value = true;
    try {
      if (data.shippingAddress) {
        const shipResult = await orderService.updateShipAddress(id, data.shippingAddress);
        if (!shipResult.isSuccess) return shipResult;
      }
      if (data.billingAddress) {
        const billResult = await orderService.updateBillAddress(id, data.billingAddress);
        if (!billResult.isSuccess) return billResult;
      }
      showToast('success', 'Success', 'Addresses updated');
      await fetchOrderById(id);
      return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null };
    } finally {
      submitting.value = false;
    }
  }

  async function advanceOrderState(id: string, status?: string) {
    submitting.value = true;
    try {
      const result = await orderService.updateStatus(id, status || 'next');
      if (result.isSuccess) {
        showToast('success', 'Success', 'Order state advanced');
        await fetchOrderById(id);
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function cancelOrder(id: string, reason?: string) {
    submitting.value = true;
    try {
      const result = await orderService.cancel(id, reason);
      if (result.isSuccess) {
        showToast('success', 'Success', 'Order canceled');
        await fetchOrderById(id);
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function refundPayment(_orderId: string, _paymentId: string, _data: Record<string, unknown>): Promise<ServerResult<void>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined };
  }

  async function cancelShipment(_orderId: string, _shipmentId: string): Promise<ServerResult<void>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined };
  }

  return {
    orders,
    current_order,
    loading,
    submitting,
    error,
    query,
    totalRecords,
    fetchOrders,
    fetchOrderById,
    createOrder,
    addOrderItem,
    updateOrderAddresses,
    advanceOrderState,
    cancelOrder,
    refundPayment,
    cancelShipment,
  };

});
