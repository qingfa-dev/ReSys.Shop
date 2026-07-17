import { defineStore } from 'pinia';
import { ref } from 'vue';
import { inventoryService } from '../services/inventory.service';
import type {
  StockItem,
  StockLocation,
  StockTransfer,
  InventoryUnit,
  InventorySearchParams,
  InventoryUnitSearchParams
} from '../types/inventory.types';

export const useInventoryStore = defineStore('inventory', () => {
  // --- STATE ---
  const stocks = ref<StockItem[]>([]);
  const locations = ref<StockLocation[]>([]);
  const transfers = ref<StockTransfer[]>([]);
  const units = ref<InventoryUnit[]>([]);

  const loading = ref(false);
  const totalStocks = ref(0);
  const totalLocations = ref(0);
  const totalTransfers = ref(0);
  const totalUnits = ref(0);

  const stockQuery = ref<InventorySearchParams>({
    page: 1,
    pageSize: 10,
    search: '',
    sort: ['-countOnHand']
  });

  const locationQuery = ref<InventorySearchParams>({
    page: 1,
    pageSize: 20,
    sort: ['name']
  });

  const transferQuery = ref<InventorySearchParams>({
    page: 1,
    pageSize: 10,
    sort: ['-createdAtUtc']
  });

  const unitQuery = ref<InventoryUnitSearchParams>({
    page: 1,
    pageSize: 20,
    sort: ['-createdAtUtc']
  });

  // --- ACTIONS ---

  async function fetchStocks(params: InventorySearchParams = {}) {
    loading.value = true;
    stockQuery.value = { ...stockQuery.value, ...params };
    try {
      const result = await inventoryService.listStocks(stockQuery.value);
      if (result.success && result.data) {
        stocks.value = result.data;
        totalStocks.value = result.meta?.totalCount || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchLocations(params: InventorySearchParams = {}) {
    loading.value = true;
    locationQuery.value = { ...locationQuery.value, ...params };
    try {
      const result = await inventoryService.listLocations(locationQuery.value);
      if (result.success && result.data) {
        locations.value = result.data;
        totalLocations.value = result.meta?.totalCount || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchTransfers(params: InventorySearchParams = {}) {
    loading.value = true;
    transferQuery.value = { ...transferQuery.value, ...params };
    try {
      const result = await inventoryService.listTransfers(transferQuery.value);
      if (result.success && result.data) {
        transfers.value = result.data;
        totalTransfers.value = result.meta?.totalCount || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchUnits(params: InventoryUnitSearchParams = {}) {
    loading.value = true;
    unitQuery.value = { ...unitQuery.value, ...params };
    try {
      const result = await inventoryService.listReservations(unitQuery.value);
      if (result.success && result.data) {
        units.value = result.data;
        totalUnits.value = result.meta?.totalCount || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  // Stub methods for components — implement when backend API adds these endpoints
  const locationTree = ref<StockLocation[]>([]);

  async function fetchLocationTree(): Promise<void> {
    locationTree.value = locations.value;
  }

  async function toggleLocationStatus(id: string): Promise<void> {
    const loc = locations.value.find(l => l.id === id);
    if (loc) loc.active = !loc.active;
  }

  async function adjustStock(_data: Record<string, unknown>): Promise<{ success: boolean }> {
    return { success: true };
  }

  async function addTransferItem(_transferId: string, _data: Record<string, unknown>): Promise<{ success: boolean }> {
    return { success: true };
  }

  async function shipTransfer(_id: string): Promise<{ success: boolean }> {
    return { success: true };
  }

  return {
    stocks,
    locations,
    transfers,
    units,
    loading,
    totalStocks,
    totalLocations,
    totalTransfers,
    totalUnits,
    stockQuery,
    locationQuery,
    transferQuery,
    unitQuery,
    inventoryService,
    fetchStocks,
    fetchLocations,
    fetchTransfers,
    fetchUnits,
    locationTree,
    fetchLocationTree,
    toggleLocationStatus,
    adjustStock,
    addTransferItem,
    shipTransfer,
  };
});
