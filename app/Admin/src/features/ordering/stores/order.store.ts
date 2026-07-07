import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { usePagedList } from '@/shared/composables/paged-list.use';
import { orderService } from '../services/order.service';
import type { 
  OrderListItem, 
  OrderDetail, 
  OrderSearchParams, 
  CreateOrderRequest, 
  AddOrderItemRequest, 
  UpdateAddressesRequest,
  RefundPaymentRequest
} from '../types/order.types';

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
      if (result.success && result.data) {
        current_order.value = result.data;
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
      if (result.success) {
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
      if (result.success) {
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
      const result = await orderService.updateAddresses(id, data);
      if (result.success) {
        showToast('success', 'Success', 'Addresses updated');
        await fetchOrderById(id);
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function advanceOrderState(id: string) {
    submitting.value = true;
    try {
      const result = await orderService.updateState(id);
      if (result.success) {
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
      const result = await orderService.cancelOrder(id, reason);
      if (result.success) {
        showToast('success', 'Success', 'Order canceled');
        await fetchOrderById(id);
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function cancelShipment(orderId: string, shipmentId: string) {
    submitting.value = true;
    try {
      const result = await orderService.cancelShipment(orderId, shipmentId);
      if (result.success) {
        showToast('success', 'Success', 'Shipment canceled');
        await fetchOrderById(orderId);
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function refundPayment(orderId: string, paymentId: string, data: RefundPaymentRequest) {
    submitting.value = true;
    try {
      const result = await orderService.refundPayment(orderId, paymentId, data);
      if (result.success) {
        showToast('success', 'Success', 'Payment refunded');
        await fetchOrderById(orderId);
      }
      return result;
    } finally {
      submitting.value = false;
    }
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
    cancelShipment,
    refundPayment
  };
});
