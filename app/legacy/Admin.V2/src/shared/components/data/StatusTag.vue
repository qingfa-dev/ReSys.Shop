<template>
  <Tag :value="label ?? status" :severity="severity" :icon="icon" rounded />
</template>

<script setup lang="ts">
import { computed } from 'vue';
import Tag from 'primevue/tag';

type Severity = 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast';

/**
 * Central status → severity/icon mapping.
 * Extend `statusMap` as new domain statuses appear instead of
 * hardcoding severities inline at each call site.
 */
const statusMap: Record<string, { severity: Severity; icon?: string; label?: string }> = {
  active: { severity: 'success', icon: 'pi pi-check' },
  inactive: { severity: 'secondary', icon: 'pi pi-minus' },
  draft: { severity: 'secondary', icon: 'pi pi-file-edit' },
  pending: { severity: 'warn', icon: 'pi pi-clock' },
  processing: { severity: 'info', icon: 'pi pi-spin pi-spinner' },
  completed: { severity: 'success', icon: 'pi pi-check-circle' },
  cancelled: { severity: 'danger', icon: 'pi pi-times-circle' },
  failed: { severity: 'danger', icon: 'pi pi-exclamation-triangle' },
  out_of_stock: { severity: 'danger', label: 'Out of stock' },
  low_stock: { severity: 'warn', label: 'Low stock' },
  in_stock: { severity: 'success', label: 'In stock' },
  authorized: { severity: 'info', icon: 'pi pi-check-circle' },
  captured: { severity: 'success', icon: 'pi pi-credit-card' },
  voided: { severity: 'warn', icon: 'pi pi-ban' },
  approved: { severity: 'info', icon: 'pi pi-thumbs-up' },
  shipped: { severity: 'info', icon: 'pi pi-truck' },
  delivered: { severity: 'success', icon: 'pi pi-box' },
  on_hold: { severity: 'warn', icon: 'pi pi-pause' },
  returned: { severity: 'warn', icon: 'pi pi-history' },
  refunded: { severity: 'info', icon: 'pi pi-money-bill' },
};

const props = defineProps<{ status: string }>();

const key = computed(() => props.status?.toLowerCase().replace(/\s+/g, '_'));
const severity = computed<Severity>(() => statusMap[key.value]?.severity ?? 'secondary');
const icon = computed(() => statusMap[key.value]?.icon);
const label = computed(() => statusMap[key.value]?.label);
</script>
