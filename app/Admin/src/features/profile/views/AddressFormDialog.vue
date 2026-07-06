<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  address: any | null
}>()

const emit = defineEmits<{
  save: [data: any]
  cancel: []
}>()

const form = ref({
  label: '',
  street: '',
  city: '',
  state: '',
  zip: '',
  isDefault: false,
})

watch(() => props.address, (val) => {
  if (val) {
    form.value = { ...val }
  } else {
    form.value = { label: '', street: '', city: '', state: '', zip: '', isDefault: false }
  }
}, { immediate: true })

function onSubmit() {
  emit('save', { ...form.value })
}
</script>

<template>
  <div class="flex flex-col gap-4 pt-2">
    <div>
      <label class="block text-sm font-medium mb-1">Label</label>
      <InputText v-model="form.label" placeholder="e.g. Warehouse A" class="w-full" />
    </div>
    <div>
      <label class="block text-sm font-medium mb-1">Street Address</label>
      <InputText v-model="form.street" placeholder="123 Commerce St" class="w-full" />
    </div>
    <div class="grid grid-cols-3 gap-3">
      <div class="col-span-1">
        <label class="block text-sm font-medium mb-1">City</label>
        <InputText v-model="form.city" placeholder="Portland" class="w-full" />
      </div>
      <div>
        <label class="block text-sm font-medium mb-1">State</label>
        <InputText v-model="form.state" placeholder="OR" class="w-full" maxlength="2" />
      </div>
      <div>
        <label class="block text-sm font-medium mb-1">ZIP</label>
        <InputText v-model="form.zip" placeholder="97201" class="w-full" />
      </div>
    </div>
    <div class="flex items-center gap-2 mt-1">
      <Checkbox v-model="form.isDefault" :binary="true" inputId="isDefault" />
      <label for="isDefault" class="text-sm">Set as default address</label>
    </div>
    <div class="flex justify-end gap-2 mt-4 pt-4 border-t border-surface-200">
      <Button label="Cancel" severity="secondary" text @click="emit('cancel')" />
      <Button label="Save" icon="pi pi-check" @click="onSubmit" />
    </div>
  </div>
</template>
