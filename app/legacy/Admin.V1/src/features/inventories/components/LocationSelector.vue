<script setup lang="ts">
import { onMounted } from 'vue';
import { useInventoryStore } from '../store/inventory.store';
import { storeToRefs } from 'pinia';

const props = defineProps<{
    modelValue?: string | null;
    placeholder?: string;
    disabled?: boolean;
}>();

const emit = defineEmits(['update:modelValue', 'change']);

const store = useInventoryStore();
const { locations, loading } = storeToRefs(store);

onMounted(async () => {
    if (locations.value.length === 0) {
        await store.fetchLocations();
    }
});
</script>

<template>
    <Select 
        :modelValue="modelValue" 
        @update:modelValue="emit('update:modelValue', $event)"
        @change="emit('change', $event)"
        :options="locations" 
        optionLabel="name" 
        optionValue="id" 
        :placeholder="placeholder || 'Select Location'" 
        :loading="loading"
        :disabled="disabled"
        filter
        class="w-full"
    >
        <template #option="slotProps">
            <div class="flex items-center gap-2">
                <i class="pi pi-building text-surface-400"></i>
                <div class="flex flex-col">
                    <span class="font-bold text-sm">{{ slotProps.option.name }}</span>
                    <small class="text-[10px] uppercase font-mono">{{ slotProps.option.code }}</small>
                </div>
            </div>
        </template>
    </Select>
</template>
