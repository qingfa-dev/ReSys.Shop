<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useInventoryStore } from '../../store/inventory.store';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import type { TreeNode } from 'primevue/treenode'

const { t } = useI18n();

const store = useInventoryStore();
const { locations, locationTree, loading } = storeToRefs(store);
const router = useRouter();

const viewMode = ref<'grid' | 'tree'>('grid');

onMounted(async () => {
    await store.fetchLocations();
    await store.fetchLocationTree();
});

const onToggleStatus = async (id: string, current: boolean) => {
    await store.toggleLocationStatus(id);
    await store.fetchLocations();
    await store.fetchLocationTree();
};
</script>

<template>
    <div class="h-full flex flex-col">
        <div class="flex items-center justify-between mb-6">
            <h3 class="text-xl font-bold text-surface-700 dark:text-surface-200 m-0">{{ t('inventory.titles.overview') }}</h3>
            <div class="flex items-center gap-3">
                <SelectButton v-model="viewMode" :options="['grid', 'tree']" aria-labelledby="basic">
                    <template #option="slotProps">
                        <i :class="slotProps.option === 'grid' ? 'pi pi-th-large' : 'pi pi-sitemap'"></i>
                    </template>
                </SelectButton>
            </div>
        </div>

        <!-- Grid View -->
        <div v-if="viewMode === 'grid'" class="grid grid-cols-1 xl:grid-cols-2 gap-6 overflow-y-auto pr-2 scrollbar-thin">
            <div v-for="loc in locations" :key="loc.id" 
                 class="bg-surface-0 dark:bg-surface-900 p-6 rounded-3xl border border-surface-100 dark:border-surface-800 shadow-sm flex flex-col gap-4 hover:border-primary/50 transition-colors">
                <div class="flex justify-between items-start">
                    <div class="flex flex-col">
                        <span class="text-[10px] font-black uppercase tracking-widest text-surface-400">{{ loc.code }}</span>
                        <h3 class="text-xl font-black m-0 tracking-tight">{{ loc.name }}</h3>
                    </div>
                    <Tag :value="loc.isActive ? 'Active' : 'Inactive'" 
                         :severity="loc.isActive ? 'success' : 'secondary'" rounded class="px-3 text-[10px] font-black" />
                </div>

                <div class="flex items-center gap-2 text-surface-500 text-sm">
                    <i class="pi pi-map-marker text-xs"></i>
                    <span>{{ loc.city }}, {{ loc.country }}</span>
                </div>

                <div class="flex items-center gap-2">
                    <Badge v-if="loc.isDefault" value="Primary" severity="info" class="text-[10px] font-black" />
                    <Tag :value="loc.code" severity="secondary" class="text-[10px] font-black" />
                </div>

                <div class="flex gap-2 mt-4 pt-4 border-t border-surface-50 dark:border-surface-800">
                    <Button icon="pi pi-pencil" text rounded severity="secondary" v-tooltip.top="'Edit Details'" @click="router.push({ name: 'inventory.locations.edit', params: { id: loc.id } })" />
                    <Button icon="pi pi-power-off" text rounded :severity="loc.isActive ? 'danger' : 'success'" @click="onToggleStatus(loc.id, loc.isActive)" />
                    <div class="flex-grow"></div>
                    <Button :label="t('inventory.titles.inventory')" icon="pi pi-box" text size="small" class="font-bold" />
                </div>
            </div>
        </div>

        <!-- Tree View -->
        <div v-else class="flex-1 overflow-hidden border shadow-sm bg-surface-0 dark:bg-surface-900 rounded-3xl border-surface-100 dark:border-surface-800">
            <TreeTable :value="locationTree as unknown as TreeNode[]" class="p-treetable-sm h-full" scrollable scrollHeight="flex">
                <Column field="name" :header="t('inventory.table.location_name')" expander></Column>
                <Column field="code" :header="t('inventory.table.code')">
                    <template #body="{ node }">
                        <span class="font-mono text-xs uppercase">{{ node.data.code }}</span>
                    </template>
                </Column>
                <Column field="city" header="City"></Column>
                <Column field="isActive" :header="t('inventory.table.status')" class="text-center">
                    <template #body="{ node }">
                        <Tag :value="node.data.isActive ? 'Active' : 'Inactive'" 
                             :severity="node.data.isActive ? 'success' : 'secondary'" rounded class="text-[10px] font-black" />
                    </template>
                </Column>
                <Column :header="t('inventory.table.actions')" class="w-24">
                    <template #body="{ node }">
                        <Button icon="pi pi-pencil" text rounded severity="secondary" @click="router.push({ name: 'inventory.locations.edit', params: { id: node.data.id } })" />
                    </template>
                </Column>
            </TreeTable>
        </div>

        <div v-if="!loading && locations.length === 0" class="flex-1 flex flex-col items-center justify-center p-20 text-surface-400">
            <i class="mb-4 text-6xl pi pi-building opacity-20"></i>
            <p class="text-xl font-medium">No locations registered.</p>
        </div>
    </div>
</template>
