<script setup lang="ts">
import { onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useCatalogDashboardStore } from '../stores/catalog-dashboard.store';
import { storeToRefs } from 'pinia';
import { useFormatter } from '@/common/composables/formatter.use';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import PageHeader from '@/shared/components/navigation/PageHeader.vue';

const { t } = useI18n();
const router = useRouter();
const store = useCatalogDashboardStore();
const { summary, loading } = storeToRefs(store);
const { formatDate } = useFormatter();

onMounted(async () => {
    await store.fetchSummary();
});

const navigateToProducts = () => router.push({ name: 'catalog.products.list' });
const navigateToTaxonomies = () => router.push({ name: 'catalog.taxonomies.list' });
const navigateToOptionTypes = () => router.push({ name: 'catalog.option-types.list' });
</script>

<template>
    <PageShell maxWidth="7xl">
        <PageHeader title="Catalog Dashboard" description="High-level overview of your product catalog and taxonomies." />

        <div v-if="loading && !summary" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            <Skeleton v-for="i in 3" :key="i" height="100px" class="rounded-2xl"></Skeleton>
        </div>

        <div v-else-if="summary" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            <!-- Products Card -->
            <div class="p-6 bg-surface-0 dark:bg-surface-900 border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm flex flex-col justify-between hover:border-primary transition-colors cursor-pointer" @click="navigateToProducts">
                <div class="flex items-center justify-between mb-4">
                    <div class="w-12 h-12 flex items-center justify-center rounded-xl bg-primary/10 text-primary">
                        <i class="pi pi-shopping-bag text-xl"></i>
                    </div>
                    <span class="text-sm font-bold text-green-500">{{ summary.activeProducts }} Active</span>
                </div>
                <div>
                    <span class="block text-surface-500 dark:text-surface-400 text-sm font-medium mb-1">Total Products</span>
                    <span class="text-3xl font-black text-surface-900 dark:text-surface-0 leading-none">{{ summary.totalProducts }}</span>
                </div>
            </div>

            <!-- Variants Card -->
            <div class="p-6 bg-surface-0 dark:bg-surface-900 border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm flex flex-col justify-between">
                <div class="flex items-center justify-between mb-4">
                    <div class="w-12 h-12 flex items-center justify-center rounded-xl bg-primary/10 text-primary">
                        <i class="pi pi-clone text-xl"></i>
                    </div>
                </div>
                <div>
                    <span class="block text-surface-500 dark:text-surface-400 text-sm font-medium mb-1">Total SKU Variants</span>
                    <span class="text-3xl font-black text-surface-900 dark:text-surface-0 leading-none">{{ summary.totalVariants }}</span>
                </div>
            </div>

            <!-- Categories Card -->
            <div class="p-6 bg-surface-0 dark:bg-surface-900 border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm flex flex-col justify-between hover:border-primary transition-colors cursor-pointer" @click="navigateToTaxonomies">
                <div class="flex items-center justify-between mb-4">
                    <div class="w-12 h-12 flex items-center justify-center rounded-xl bg-primary/10 text-primary">
                        <i class="pi pi-sitemap text-xl"></i>
                    </div>
                    <span class="text-sm font-bold text-surface-500">{{ summary.totalTaxonomies }} Roots</span>
                </div>
                <div>
                    <span class="block text-surface-500 dark:text-surface-400 text-sm font-medium mb-1">Categories (Taxons)</span>
                    <span class="text-3xl font-black text-surface-900 dark:text-surface-0 leading-none">{{ summary.totalTaxons }}</span>
                </div>
            </div>

        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
            <!-- Recently Added -->
            <div class="lg:col-span-2">
                <div class="bg-surface-0 dark:bg-surface-900 border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm overflow-hidden">
                    <div class="p-6 border-b border-surface-100 dark:border-surface-800 flex justify-between items-center">
                        <h3 class="text-xl font-bold m-0">Recently Added Products</h3>
                        <Button :label="t('catalog.actions.view_all')" text size="small" @click="navigateToProducts" />
                    </div>
                    <div class="p-0">
                            <DataTable :value="summary?.recentProducts" class="p-datatable-sm border-none">
                            <Column field="name" :header="t('catalog.products.table.name')">
                                <template #body="{ data }">
                                    <span class="font-bold text-surface-900 dark:text-surface-0">{{ data.name }}</span>
                                </template>
                            </Column>
                            <Column field="createdAtUtc" :header="t('catalog.products.table.added_on')">
                                <template #body="{ data }">
                                    <span class="text-sm text-surface-500">{{ formatDate(data.createdAtUtc) }}</span>
                                </template>
                            </Column>
                            <Column class="w-24 text-right">
                                <template #body="{ data }">
                                    <Button icon="pi pi-pencil" text rounded size="small" @click="router.push({ name: 'catalog.products.edit', params: { id: data.id } })" />
                                </template>
                            </Column>
                            <template #empty>
                                <div class="p-8 text-center text-surface-400 italic">No products added recently.</div>
                            </template>
                        </DataTable>
                    </div>
                </div>
            </div>

            <!-- Quick Actions -->
            <div class="lg:col-span-1">
                <div class="bg-surface-0 dark:bg-surface-900 border border-surface-100 dark:border-surface-800 rounded-2xl shadow-sm p-6 h-full">
                    <h3 class="text-xl font-bold mb-6">Catalog Actions</h3>
                    <div class="flex flex-col gap-3">
                        <Button :label="t('catalog.products.actions.new')" icon="pi pi-plus" class="w-full rounded-xl py-3 justify-start" @click="router.push({ name: 'catalog.products.create' })" />
                        <Button :label="t('catalog.taxonomies.actions.create')" icon="pi pi-plus" severity="secondary" outlined class="w-full rounded-xl py-3 justify-start" @click="router.push({ name: 'catalog.taxonomies.create' })" />
                        <Button :label="t('catalog.option_types.actions.manage')" icon="pi pi-list" severity="secondary" outlined class="w-full rounded-xl py-3 justify-start" @click="navigateToOptionTypes" />
                        
                        <Divider class="my-4" />
                        
                        <div class="bg-surface-50 dark:bg-surface-800 p-4 rounded-xl border border-dashed border-surface-200 dark:border-surface-700">
                            <h4 class="font-bold text-sm mb-2 uppercase tracking-wider text-surface-500">System Tip</h4>
                            <p class="text-xs leading-relaxed text-surface-600 dark:text-surface-400 m-0">
                                Use taxonomies to create hierarchical structures for navigation, and dynamic rules to auto-classify products based on attributes or pricing.
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </PageShell>
</template>
