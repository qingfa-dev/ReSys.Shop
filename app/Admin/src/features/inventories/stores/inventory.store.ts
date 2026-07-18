import { defineStore } from 'pinia';
import { ref } from 'vue';
import { inventoryService } from '../services/inventory.service';
import type { InventoryUnit } from '../inventory-units/types/inventory-unit.response.type';
import type { StockItem } from '../stock-items/types/stock-item.response.type';
import type { StockLocation } from '../stock-locations/types/stock-location.response.type';
import type { StockTransfer } from '../stock-transfers/types/stock-transfer.response.type';
import type { StockItemQuery } from '../stock-items/types/stock-item.query.type';
import type { InventoryUnitQuery } from '../inventory-units/types/inventory-unit.query.type';
import type { ServerQueryingParameters } from '@/shared/api/types/query.types';

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

  const stockQuery = ref<StockItemQuery>({
    page: 1,
    pageSize: 10,
    search: '',
    sort: ['-countOnHand']
  });

  const locationQuery = ref<ServerQueryingParameters>({
    page: 1,
    pageSize: 20,
    sort: ['name']
  });

  const transferQuery = ref<ServerQueryingParameters>({
    page: 1,
    pageSize: 10,
    sort: ['-createdAtUtc']
  });

  const unitQuery = ref<InventoryUnitQuery>({
    page: 1,
    pageSize: 20,
    sort: ['-createdAtUtc']
  });

  // --- ACTIONS ---

  async function fetchStocks(params: StockItemQuery = {}) {
    loading.value = true;
    stockQuery.value = { ...stockQuery.value, ...params };
    try {
      const result = await inventoryService.listStocks(stockQuery.value);
      if (result.isSuccess) {
        stocks.value = result.items;
        totalStocks.value = result.totalCount || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchLocations(params: ServerQueryingParameters = {}) {
    loading.value = true;
    locationQuery.value = { ...locationQuery.value, ...params };
    try {
      const result = await inventoryService.listLocations(locationQuery.value);
      if (result.isSuccess) {
        locations.value = result.items;
        totalLocations.value = result.totalCount || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchTransfers(params: ServerQueryingParameters = {}) {
    loading.value = true;
    transferQuery.value = { ...transferQuery.value, ...params };
    try {
      const result = await inventoryService.listTransfers(transferQuery.value);
      if (result.isSuccess) {
        transfers.value = result.items;
        totalTransfers.value = result.totalCount || 0;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchUnits(params: InventoryUnitQuery = {}) {
    loading.value = true;
    unitQuery.value = { ...unitQuery.value, ...params };
    try {
      const result = await inventoryService.listReservations(unitQuery.value);
      if (result.isSuccess) {
        units.value = result.items;
        totalUnits.value = result.totalCount || 0;
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
    if (loc) loc.isActive = !loc.isActive;
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
