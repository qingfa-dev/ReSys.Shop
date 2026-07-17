<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';

const props = defineProps<{
    title?: string;
    description?: string;
    emptyMessage?: string;
}>();

const modelValue = defineModel<Record<string, any>>({ default: () => ({}) });

interface MetadataEntry {
    id: number;
    key: string;
    value: any;
    isNew?: boolean;
}

const entries = ref<MetadataEntry[]>([]);
let nextId = 0;

const initialize = () => {
    const current = modelValue.value || {};
    entries.value = Object.entries(current).map(([key, value]) => ({
        id: nextId++,
        key,
        value: typeof value === 'object' ? JSON.stringify(value) : value,
        isNew: false
    }));
};

const sync = () => {
    const result: Record<string, any> = {};
    entries.value.forEach(entry => {
        if (entry.key.trim()) {
            let val = entry.value;
            if (typeof val === 'string') {
                const trimmed = val.trim();
                if ((trimmed.startsWith('{') && trimmed.endsWith('}')) || (trimmed.startsWith('[') && trimmed.endsWith(']'))) {
                    try {
                        val = JSON.parse(trimmed);
                    } catch (e) {
                    }
                }
            }
            result[entry.key.trim()] = val;
        }
    });
    modelValue.value = result;
};

const addEntry = () => {
    entries.value.push({ id: nextId++, key: '', value: '', isNew: true });
};

const removeEntry = (id: number) => {
    entries.value = entries.value.filter(e => e.id !== id);
    sync();
};

const onBlur = () => {
    sync();
};

onMounted(initialize);

watch(() => modelValue.value, (newVal, oldVal) => {
    const newKeys = Object.keys(newVal || {}).sort().join(',');
    const oldKeys = Object.keys(oldVal || {}).sort().join(',');
    if (newKeys !== oldKeys && !loadingExternal.value) {
        initialize();
    }
}, { deep: true });

const loadingExternal = ref(false);
</script>

<template>
    <div class="metadata-manager">
        <div class="flex items-center justify-between mb-4">
            <div>
                <h3 v-if="title" class="text-lg font-bold text-surface-800 dark:text-surface-50">{{ title }}</h3>
                <p v-if="description" class="text-sm text-surface-500">{{ description }}</p>
            </div>
            <Button icon="pi pi-plus" label="Add Metadata" size="small" text @click="addEntry" />
        </div>

        <div v-if="entries.length === 0" class="p-8 border-2 border-dashed border-surface-200 dark:border-surface-800 rounded-xl text-center">
            <p class="text-surface-400 italic">{{ emptyMessage || 'No metadata defined' }}</p>
        </div>

        <div v-else class="flex flex-col gap-3">
            <div v-for="entry in entries" :key="entry.id" class="flex items-start gap-2 group animate-fadein">
                <div class="flex-1 grid grid-cols-2 gap-2">
                    <InputText 
                        v-model="entry.key" 
                        placeholder="Key" 
                        class="w-full font-mono text-sm" 
                        @blur="onBlur"
                        :class="{'border-primary': entry.isNew}"
                    />
                    <InputText 
                        v-model="entry.value" 
                        placeholder="Value (string, number or JSON)" 
                        class="w-full text-sm" 
                        @blur="onBlur" 
                    />
                </div>
                <Button 
                    icon="pi pi-times" 
                    severity="danger" 
                    text 
                    rounded 
                    @click="removeEntry(entry.id)" 
                    class="opacity-0 group-hover:opacity-100 transition-opacity" 
                />
            </div>
        </div>
        
        <small class="block mt-2 text-surface-400 italic">
            * Keys must be unique. Objects can be entered as JSON.
        </small>
    </div>
</template>

<style scoped>
.metadata-manager :deep(.p-inputtext) {
    padding: 0.5rem 0.75rem;
}
</style>
