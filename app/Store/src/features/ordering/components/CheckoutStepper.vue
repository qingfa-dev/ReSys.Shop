<script setup lang="ts">
import type { CheckoutStep } from '../stores/checkoutStore'

defineProps<{ steps: Array<{ label: string; stepNumber: number }>; currentStep: CheckoutStep }>()
</script>
<template>
  <div class="flex items-center justify-center mb-8">
    <template v-for="(step, idx) in steps" :key="step.stepNumber">
      <div class="flex items-center">
        <div class="flex items-center gap-2" :class="currentStep >= step.stepNumber ? 'text-gray-900' : 'text-gray-400'">
          <span
            class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium border-2"
            :class="currentStep > step.stepNumber
              ? 'bg-gray-900 border-gray-900 text-white'
              : currentStep === step.stepNumber
                ? 'border-gray-900 text-gray-900'
                : 'border-gray-300 text-gray-400'"
          >
            <i v-if="currentStep > step.stepNumber" class="pi pi-check text-xs" />
            <span v-else>{{ step.stepNumber }}</span>
          </span>
          <span class="text-sm font-medium hidden sm:inline">{{ step.label }}</span>
        </div>
        <div v-if="idx < steps.length - 1" class="w-12 sm:w-24 h-px mx-2" :class="currentStep > step.stepNumber ? 'bg-gray-900' : 'bg-gray-300'" />
      </div>
    </template>
  </div>
</template>
