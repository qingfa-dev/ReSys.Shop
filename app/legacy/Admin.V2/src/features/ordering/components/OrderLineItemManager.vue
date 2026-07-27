<script setup lang="ts">
import { ref } from 'vue'
import Column from 'primevue/column'
import DataTable from '@/shared/components/data/DataTable.vue'
import ActionMenu from '@/shared/components/layout/ActionMenu.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { OrderLineItemApi } from '../api'
import type { OrderLineItemResponse } from '../types'

const props = defineProps<{
  orderId: string
  lineItems: OrderLineItemResponse[]
  readonly?: boolean
}>()

const emit = defineEmits<{ refresh: [] }>()

const { confirmDelete } = useConfirm()
const toast = useToast()

const editingItem = ref<OrderLineItemResponse | null>(null)
const adding = ref(false)
const newVariantId = ref('')
const newQuantity = ref(1)
const newUnitPrice = ref(0)
const editQuantity = ref(1)
const editUnitPrice = ref(0)

function startAdd() {
  adding.value = true
  newVariantId.value = ''
  newQuantity.value = 1
  newUnitPrice.value = 0
}

function cancelAdd() {
  adding.value = false
}

async function addLineItem() {
  if (!newVariantId.value || newQuantity.value < 1) return
  const result = await OrderLineItemApi.create(props.orderId, {
    variantId: newVariantId.value,
    quantity: newQuantity.value,
    unitPrice: newUnitPrice.value,
  })
  if (result.isSuccess) {
    toast.success('Line item added')
    adding.value = false
    emit('refresh')
  } else {
    toast.error(result.message ?? 'Failed to add line item')
  }
}

function startEdit(item: OrderLineItemResponse) {
  editingItem.value = item
  editQuantity.value = item.quantity
  editUnitPrice.value = item.unitPrice
}

function cancelEdit() {
  editingItem.value = null
}

async function saveEdit() {
  if (!editingItem.value) return
  const result = await OrderLineItemApi.update(props.orderId, editingItem.value.id, {
    quantity: editQuantity.value,
    unitPrice: editUnitPrice.value,
  })
  if (result.isSuccess) {
    toast.success('Line item updated')
    editingItem.value = null
    emit('refresh')
  } else {
    toast.error(result.message ?? 'Failed to update line item')
  }
}

function confirmDeleteItem(item: OrderLineItemResponse) {
  confirmDelete({
    target: 'this line item',
    onAccept: async () => {
      const result = await OrderLineItemApi.delete(props.orderId, item.id)
      if (result.isSuccess) {
        toast.success('Line item deleted')
        emit('refresh')
      } else {
        toast.error(result.message ?? 'Failed to delete line item')
      }
    },
  })
}

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount)
}
</script>

<template>
  <div>
    <div class="flex align-items-center justify-content-between mb-3">
      <h3 class="text-lg font-semibold m-0">Line Items</h3>
      <button v-if="!readonly" class="p-button p-component p-button-sm" @click="startAdd">
        Add Item
      </button>
    </div>

    <DataTable :rows="[...props.lineItems]" :loading="false" :total-records="props.lineItems.length">
      <Column field="variantSku" header="SKU" />
      <Column field="variantName" header="Name" />
      <Column field="quantity" header="Qty" />
      <Column field="unitPrice" header="Unit Price">
        <template #body="{ data }">
          {{ formatCurrency(data.unitPrice) }}
        </template>
      </Column>
      <Column field="totalPrice" header="Total">
        <template #body="{ data }">
          {{ formatCurrency(data.totalPrice) }}
        </template>
      </Column>
      <template v-if="!readonly" #rowActions="{ data }">
        <ActionMenu
          :items="[
            { label: 'Edit', icon: 'pi pi-pencil', command: () => startEdit(data) },
            { label: 'Delete', icon: 'pi pi-trash', command: () => confirmDeleteItem(data) },
          ]"
        />
      </template>
    </DataTable>

    <div v-if="adding" class="card mt-3 p-3 surface-50 border-round">
      <h4 class="text-md font-semibold mb-2">New Line Item</h4>
      <div class="grid">
        <div class="col-4">
          <FormField label="Variant ID" required>
            <input v-model="newVariantId" type="text" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Quantity" required>
            <input v-model.number="newQuantity" type="number" min="1" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
        <div class="col-4">
          <FormField label="Unit Price" required>
            <input v-model.number="newUnitPrice" type="number" min="0" step="0.01" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
      </div>
      <div class="flex gap-2 mt-2">
        <button class="p-button p-component" @click="addLineItem">Add</button>
        <button class="p-button p-component p-button-secondary" @click="cancelAdd">Cancel</button>
      </div>
    </div>

    <div v-if="editingItem" class="card mt-3 p-3 surface-50 border-round">
      <h4 class="text-md font-semibold mb-2">Edit Line Item</h4>
      <div class="grid">
        <div class="col-6">
          <FormField label="Quantity" required>
            <input v-model.number="editQuantity" type="number" min="1" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Unit Price" required>
            <input v-model.number="editUnitPrice" type="number" min="0" step="0.01" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
      </div>
      <div class="flex gap-2 mt-2">
        <button class="p-button p-component" @click="saveEdit">Save</button>
        <button class="p-button p-component p-button-secondary" @click="cancelEdit">Cancel</button>
      </div>
    </div>
  </div>
</template>
