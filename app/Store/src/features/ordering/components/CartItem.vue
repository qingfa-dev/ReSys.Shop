<script setup lang="ts">
import type { CartLineItem } from '../types/cart'
import { formatVnd } from '@/shared/utils/currency'

defineProps<{ item: CartLineItem }>()
const emit = defineEmits<{ updateQuantity: [lineItemId: string, qty: number]; remove: [lineItemId: string] }>()
</script>
<template>
  <div class="flex gap-4 py-4 border-b border-gray-200">
    <img
      v-if="item.thumbnailUrl"
      :src="item.thumbnailUrl"
      :alt="item.productName"
      class="w-20 h-20 rounded-lg object-cover bg-gray-100"
    />
    <div class="w-20 h-20 rounded-lg bg-gray-100 flex items-center justify-center text-gray-400" v-else>
      <i class="pi pi-image" />
    </div>
    <div class="flex-1 min-w-0">
      <router-link :to="`/products/${item.productSlug}`" class="text-sm font-medium text-gray-900 hover:text-gray-600">
        {{ item.productName }}
      </router-link>
      <p v-if="item.optionDescription" class="text-xs text-gray-500 mt-1">{{ item.optionDescription }}</p>
      <p class="text-sm font-semibold text-gray-900 mt-1">{{ formatVnd(item.unitPrice) }}</p>
    </div>
    <div class="flex flex-col items-end gap-2">
      <InputNumber :model-value="item.quantity" :min="1" :max="item.maxQuantity" class="w-20" @update:model-value="(v: number) => emit('updateQuantity', item.lineItemId, v)" />
      <Button icon="pi pi-trash" severity="danger" text size="small" @click="emit('remove', item.lineItemId)" />
      <p class="text-sm font-semibold">{{ formatVnd(item.unitPrice * item.quantity) }}</p>
    </div>
  </div>
</template>
