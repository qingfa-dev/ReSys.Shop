<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { searchService, type GlobalSearchResult } from '@/shared/services/search.service';

const router = useRouter();
const query = ref('');
const results = ref<GlobalSearchResult[]>([]);
const loading = ref(false);

const onSearch = async (event: { query: string }) => {
    if (!event.query.trim()) return;
    loading.value = true;
    try {
        const res = await searchService.search(event.query);
        if (res.success && res.data) {
            results.value = res.data.results;
        }
    } finally {
        loading.value = false;
    }
};

const onSelect = (event: { value: GlobalSearchResult }) => {
    const item = event.value;
    router.push({ name: item.route_name, params: { id: item.id } });
    query.value = '';
};

const getIcon = (type: string) => {
    switch (type) {
        case 'Product': return 'pi pi-shopping-bag';
        case 'Order': return 'pi pi-shopping-cart';
        case 'User': return 'pi pi-user';
        default: return 'pi pi-search';
    }
};

const getTypeColor = (type: string) => {
    switch (type) {
        case 'Product': return 'text-blue-500 bg-blue-500/10';
        case 'Order': return 'text-purple-500 bg-purple-500/10';
        case 'User': return 'text-green-500 bg-green-500/10';
        default: return 'text-surface-500 bg-surface-500/10';
    }
};
</script>

<template>
    <div class="global-search w-full max-w-md mx-4">
        <AutoComplete
            v-model="query"
            :suggestions="results"
            @complete="onSearch"
            @item-select="onSelect"
            placeholder="Search SKUs, Orders, Users..."
            optionLabel="title"
            class="w-full"
            inputClass="w-full bg-surface-100 dark:bg-surface-800 border-none rounded-2xl h-10 px-4 focus:ring-2 focus:ring-primary shadow-sm"
            panelClass="rounded-2xl shadow-xl mt-2 overflow-hidden border border-surface-100 dark:border-surface-800"
        >
            <template #option="slotProps">
                <div class="flex items-center gap-3 p-1">
                    <div :class="['w-8 h-8 rounded-lg flex items-center justify-center shrink-0', getTypeColor(slotProps.option.type)]">
                        <i :class="getIcon(slotProps.option.type)" class="text-sm"></i>
                    </div>
                    <div class="flex flex-col overflow-hidden">
                        <span class="font-bold text-sm truncate">{{ slotProps.option.title }}</span>
                        <small class="text-surface-500 text-[10px] uppercase font-black tracking-widest">{{ slotProps.option.subtitle }}</small>
                    </div>
                </div>
            </template>
            <template #empty v-if="query && !loading">
                <div class="p-4 text-center text-surface-500">
                    No results found for "{{ query }}"
                </div>
            </template>
        </AutoComplete>
    </div>
</template>

<style scoped>
:deep(.p-autocomplete-panel) {
    background: var(--p-surface-0);
}
.dark :deep(.p-autocomplete-panel) {
    background: var(--p-surface-900);
}
</style>
