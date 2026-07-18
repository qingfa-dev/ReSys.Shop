<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { usePaymentStore } from '../stores/payment.store'
import { storeToRefs } from 'pinia'
import Button from 'primevue/button'

const route = useRoute()
const store = usePaymentStore()
const { current, loading } = storeToRefs(store)

onMounted(() => store.fetchById(route.params.id as string))
</script>

<template>
  <div v-if="loading">Loading...</div>
  <div v-else-if="current">
    <h2>Payment {{ current.id }}</h2>
    <p>Order: {{ current.orderId }}</p>
    <p>Amount: {{ current.amount }} {{ current.currency }}</p>
    <p>Status: {{ current.status }}</p>
    <p>Method: {{ current.methodName }}</p>
    <div class="actions">
      <Button label="Capture" @click="store.capture(current.id)" />
      <Button label="Void" @click="store.void(current.id)" class="p-button-warning" />
      <Button label="Refund" @click="store.refund(current.id, current.amount)" class="p-button-danger" />
    </div>
  </div>
</template>
