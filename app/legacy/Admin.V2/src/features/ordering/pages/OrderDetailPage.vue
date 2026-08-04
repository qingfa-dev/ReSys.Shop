<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { DetailLayout, AppCard } from '@/shared/components'
import OrderForm from '../components/OrderForm.vue'
import FulfillmentWorkflow from '../components/FulfillmentWorkflow.vue'
import { useOrder } from '../composables/useOrder'
import { OrderApi } from '../api'

const { id, mode } = useOrder()
const orderStatus = ref<string | null>(null)
const formKey = ref(0)

const showWorkflow = computed(() => mode.value === 'view' && !!id.value)

async function loadOrderStatus() {
  if (!id.value) return
  try {
    const result = await OrderApi.get(id.value)
    if (result.isSuccess) {
      orderStatus.value = result.value.status
    }
  } catch (err) {
    console.error(err)
  }
}

function onStatusChanged() {
  loadOrderStatus()
  formKey.value++
}

onMounted(() => { loadOrderStatus() })
</script>

<template>
  <DetailLayout>
    <OrderForm :key="formKey" />
    <template v-if="showWorkflow" #sub-entities>
      <AppCard>
        <FulfillmentWorkflow
          v-if="orderStatus"
          :order-id="id!"
          :status="orderStatus"
          @status-changed="onStatusChanged"
        />
      </AppCard>
    </template>
  </DetailLayout>
</template>
