import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useToast } from '@/common/composables/toast.use';
import { usePagedList } from '@/common/composables/paged-list.use';
import { orderRepository } from '../api/order.api';
import type { ServerResult } from '@/common/api/types/result.types';
import type { OrderListItemModel, OrderDetailModel } from '../types/order.model';
import type { OrderQuery } from '../types/order.query';
import type { CreateOrderRequest, AddOrderItemRequest, UpdateAddressesRequest } from '../types/order.request';

export const useOrderStore = defineStore('order', () => {
  const { showToast } = useToast();
  const { t } = useI18n();

  const current_order = ref<OrderDetailModel | null>(null);
  const submitting = ref(false);

  const { items: orders, totalRecords, params: query, fetch: fetchOrders, loading, error } = usePagedList<OrderListItemModel, OrderQuery>(
    (p) => orderRepository.list(p),
    { page: 1, pageSize: 10, search: '', state: '', sort: ['-createdAtUtc'] },
  );

  async function fetchOrderById(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const result = await orderRepository.getById(id);
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
      const result = await orderRepository.create(data);
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('ordering.messages.order_created'));
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function addOrderItem(id: string, data: AddOrderItemRequest) {
    submitting.value = true;
    try {
      const result = await orderRepository.addLineItem(id, data);
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('ordering.messages.item_added'));
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
        const shipResult = await orderRepository.updateShipAddress(id, data.shippingAddress);
        if (!shipResult.isSuccess) return shipResult;
      }
      if (data.billingAddress) {
        const billResult = await orderRepository.updateBillAddress(id, data.billingAddress);
        if (!billResult.isSuccess) return billResult;
      }
      showToast('success', t('common.success'), t('ordering.messages.addresses_updated'));
      await fetchOrderById(id);
      return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null };
    } finally {
      submitting.value = false;
    }
  }

  async function advanceOrderState(id: string, status?: string) {
    submitting.value = true;
    try {
      const result = await orderRepository.updateStatus(id, { status: status || 'next' });
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('ordering.messages.state_advanced'));
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
      const result = await orderRepository.cancel(id, reason ? { reason } : undefined);
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('ordering.messages.order_canceled'));
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

  async function resumeOrder(id: string) {
    submitting.value = true;
    try {
      const result = await orderRepository.resume(id);
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('ordering.messages.order_resumed'));
        await fetchOrderById(id);
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function updateLineItem(orderId: string, _lineItemId: string, data: { quantity: number }) {
    submitting.value = true;
    try {
      const result = await orderRepository.updateLineItem(orderId, _lineItemId, data);
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('ordering.messages.line_item_updated'));
        await fetchOrderById(orderId);
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function removeLineItem(orderId: string, _lineItemId: string) {
    submitting.value = true;
    try {
      const result = await orderRepository.removeLineItem(orderId, _lineItemId);
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('ordering.messages.line_item_removed'));
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
    refundPayment,
    cancelShipment,
    resumeOrder,
    updateLineItem,
    removeLineItem,
  };

});
