<script setup lang="ts">
defineProps<{ currentStep: number }>()

const steps = [
  { label: 'Address' },
  { label: 'Delivery' },
  { label: 'Payment' },
  { label: 'Confirm' },
  { label: 'Complete' },
]
</script>
<template>
  <!-- Section: Checkout Steps — visual progress indicator -->
  <Steps :model="steps" :active-index="currentStep - 1" class="mb-8" />
</template>
