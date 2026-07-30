<script setup lang="ts">
import Card from 'primevue/card'
import { Form } from '@primevue/forms'
import type { FormResolverOptions, FormSubmitEvent } from '@primevue/forms'

interface Props {
  title: string
  description?: string
  resolver?: (options: FormResolverOptions) => any
  initialValues?: Record<string, any>
}

withDefaults(defineProps<Props>(), {
  description: '',
})

const emit = defineEmits<{
  (e: 'submit', event: FormSubmitEvent): void
}>()
</script>

<template>
  <Card>
    <template #content>
      <div class="flex flex-col gap-6">
        <div>
          <div class="font-semibold text-xl">{{ title }}</div>
          <p v-if="description" class="text-muted-color mt-1">{{ description }}</p>
        </div>
        <Form
          :resolver="resolver"
          :initial-values="initialValues"
          class="flex flex-col gap-4"
          @submit="emit('submit', $event)"
        >
          <slot />
        </Form>
      </div>
    </template>
  </Card>
</template>
