<script setup lang="ts">
import { computed } from 'vue'
import { useConfirm } from 'primevue/useconfirm'
import type { Address, AddressType } from '../types/address'

const props = defineProps<{ address: Address; busy?: boolean }>()
const emit = defineEmits<{
  edit: [address: Address]
  delete: [id: string]
  setDefault: [id: string]
}>()
const confirm = useConfirm()

// The backend enum is Shipping/Billing/Other (NOT Home/Office); display the wire value.
const typeLabels: Record<AddressType, string> = {
  Shipping: 'Shipping',
  Billing: 'Billing',
  Other: 'Other',
}

// Computed once per render instead of recomputing cityLine() on every interpolation.
const fullName = computed(() =>
  [props.address.firstName, props.address.lastName].filter(Boolean).join(' '),
)

const cityLine = computed(() =>
  [props.address.city, props.address.stateProvince, props.address.zipCode].filter(Boolean).join(', '),
)

function requestDelete(): void {
  confirm.require({
    message: 'Delete this address? This cannot be undone.',
    header: 'Delete Address',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: () => emit('delete', props.address.id),
  })
}
</script>

<template>
  <div class="bg-white rounded-xl border border-gray-200 p-6">
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div class="flex flex-wrap items-center gap-2">
        <span
          class="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-600"
        >
          {{ typeLabels[address.addressType] }}
        </span>
        <span
          v-if="address.isDefault"
          class="inline-flex items-center rounded-full bg-blue-50 px-2.5 py-0.5 text-xs font-medium text-blue-600"
        >
          <i class="pi pi-check text-[10px] mr-1" />
          Default
        </span>
      </div>
      <div class="flex items-center gap-2">
        <Button
          v-if="!address.isDefault"
          label="Set Default"
          severity="secondary"
          outlined
          size="small"
          icon="pi pi-star"
          :disabled="busy"
          @click="emit('setDefault', address.id)"
        />
        <Button
          label="Edit"
          severity="secondary"
          outlined
          size="small"
          icon="pi pi-pencil"
          :disabled="busy"
          @click="emit('edit', address)"
        />
        <Button
          label="Delete"
          severity="danger"
          outlined
          size="small"
          icon="pi pi-trash"
          :disabled="busy"
          @click="requestDelete"
        />
      </div>
    </div>

    <div class="mt-4">
      <p class="font-semibold text-gray-900">{{ fullName }}</p>
      <p v-if="address.phone" class="text-sm text-gray-500 mt-0.5">{{ address.phone }}</p>
      <p class="text-sm text-gray-600 mt-1">{{ address.address1 }}</p>
      <p v-if="address.address2" class="text-sm text-gray-600">{{ address.address2 }}</p>
      <p class="text-sm text-gray-600">
        {{ cityLine }}<template v-if="cityLine">, </template>{{ address.countryName }}
      </p>
      <p v-if="address.label" class="text-xs text-gray-400 mt-1">{{ address.label }}</p>
    </div>
  </div>
</template>
