<script setup lang="ts">
import { computed, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useTaxonStore } from '../store/taxon.store'
import { useTaxonomyStore } from '../../taxonomies/store/taxonomy.store'
import { storeToRefs } from 'pinia'
import { useApiErrorHandler } from '@/common/composables/api-error-handler.use'
import { useToast } from '@/common/composables/toast.use'
import ConfirmDialog from '@/shared/components/overlays/ConfirmDialog.vue'
import type { TaxonListItem } from '../models/taxon.response'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const taxonStore = useTaxonStore()
const taxonomyStore = useTaxonomyStore()
const { loading, taxonTree } = storeToRefs(taxonStore)
const { currentItem: taxonomy } = storeToRefs(taxonomyStore)
const { handleApiResult } = useApiErrorHandler()
const { showToast } = useToast()

const taxonomyId = computed(() => route.params.taxonomyId as string)
const selectedTaxonId = computed(() => route.params.id as string)

const loadHierarchy = async () => {
    if (!taxonomyId.value) return
    if (!taxonomy.value || taxonomy.value.id !== taxonomyId.value)
        await taxonomyStore.fetchById(taxonomyId.value)
    await taxonStore.fetchTaxons(taxonomyId.value)
}

watch(taxonomyId, () => {
    loadHierarchy()
}, { immediate: true })

const openNew = (parent?: TaxonListItem) => {
  router.push({
    name: 'catalog.taxa.create',
    params: { taxonomyId: taxonomyId.value },
    query: parent ? { parentId: parent.id } : {}
  })
}

const openEdit = (node: TaxonListItem) => {
  router.push({
    name: 'catalog.taxa.edit',
    params: { taxonomyId: taxonomyId.value, id: node.id }
  })
}

const deleteTaxon = async (node: TaxonListItem) => {
  const result = await taxonStore.deleteTaxon(taxonomyId.value, node.id)
  if (result.isSuccess) {
    showToast('success', t('common.deleted'), t('catalog.taxa.messages.delete_success'))
    if (selectedTaxonId.value === node.id) {
        router.push({ name: 'catalog.taxa.manager', params: { taxonomyId: taxonomyId.value } })
    }
  } else {
    handleApiResult(result)
  }
}

const goBack = () => router.push({ name: 'catalog.taxonomies.list' })
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="p-6 pb-0 max-w-full">
        <div class="flex items-center justify-between mt-4 mb-6">
            <div class="flex items-center gap-4">
                <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="goBack" class="bg-surface-100 dark:bg-surface-800" />
                <div>
                    <h2 class="text-3xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                        {{ taxonomy?.presentation }}
                    </h2>
                    <p class="text-sm text-surface-500 m-0">{{ t('catalog.taxa.descriptions.manager') }}</p>
                </div>
            </div>
            <div class="flex items-center gap-2">
                <Button :label="t('catalog.taxa.actions.create_root')" icon="pi pi-plus" size="small" class="rounded-xl shadow-lg" @click="openNew()" />
                <Button icon="pi pi-refresh" severity="secondary" text rounded @click="taxonStore.fetchTaxons(taxonomyId)" :loading="loading" />
            </div>
        </div>
    </div>

    <div class="flex flex-1 gap-6 p-6 pt-0 overflow-hidden min-h-[600px]">
        <div class="w-1/3 min-w-[320px] flex flex-col">
            <Card class="flex-1 border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden flex flex-col">
                <template #content>
                    <div class="flex flex-col h-full">
                        <div class="p-4 border-b border-surface-100 dark:border-surface-800 flex items-center justify-between">
                            <span class="font-bold text-xs uppercase tracking-widest text-surface-400">{{ t('catalog.taxa.messages.hierarchy_view') }}</span>
                            <Badge :value="taxonStore.currentTaxons.length" severity="secondary" />
                        </div>

                        <div class="flex-1 overflow-y-auto p-2 scrollbar-thin">
                            <div v-if="loading && taxonTree.length === 0" class="flex flex-col items-center justify-center py-20">
                                <ProgressSpinner style="width: 40px; height: 40px" />
                            </div>

                            <Tree
                                v-else
                                :value="taxonTree"
                                selectionMode="single"
                                :pt="{ root: { class: 'bg-transparent border-none p-0' } }"
                            >
                                <template #default="{ node }: { node: any }">
                                    <div
                                        class="flex items-center justify-between w-full p-2 rounded-xl group cursor-pointer transition-colors"
                                        :class="{ 'bg-primary/10 text-primary': selectedTaxonId === node.id, 'hover:bg-surface-100 dark:hover:bg-surface-800': selectedTaxonId !== node.id }"
                                        @click="openEdit(node as any)"
                                    >
                                        <div class="flex items-center gap-3 overflow-hidden">
                                            <i class="pi pi-folder text-sm shrink-0"></i>
                                            <span class="truncate font-medium text-sm">{{ node.presentation }}</span>
                                        </div>
                                        <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
                                            <Button icon="pi pi-plus" text rounded size="small" severity="secondary" @click.stop="openNew(node as any)" v-tooltip.top="t('catalog.taxa.actions.add_taxon')" />
                                            <ConfirmDialog
                                              :header="t('catalog.taxa.confirm.delete_header')"
                                              :message="t('catalog.taxa.confirm.delete_message').replace('{name}', (node as any).presentation)"
                                              :accept-label="t('catalog.taxa.actions.delete_taxon')"
                                              :reject-label="t('catalog.taxa.actions.cancel')"
                                              @confirm="deleteTaxon(node as any)" />
                                        </div>
                                    </div>
                                </template>
                            </Tree>

                            <div v-if="taxonTree.length === 0 && !loading" class="flex flex-col items-center justify-center py-20 text-center px-4">
                                <i class="pi pi-folder-open text-4xl text-surface-200 mb-4"></i>
                                <p class="text-surface-400 text-sm italic">{{ t('catalog.taxa.messages.no_categories') }}</p>
                                <Button :label="t('catalog.taxa.actions.create_root')" text size="small" @click="openNew()" />
                            </div>
                        </div>
                    </div>
                </template>
            </Card>
        </div>

        <div class="flex-1 overflow-hidden flex flex-col">
            <div v-if="route.name === 'catalog.taxa.manager'" class="flex-1 flex flex-col items-center justify-center bg-surface-50/50 dark:bg-surface-950/20 rounded-3xl border-2 border-dashed border-surface-200 dark:border-surface-800">
                <div class="w-20 h-20 rounded-full bg-surface-100 dark:bg-surface-800 flex items-center justify-center mb-6">
                    <i class="pi pi-sitemap text-4xl text-surface-300"></i>
                </div>
                <h3 class="text-xl font-bold text-surface-700 dark:text-surface-200">{{ t('catalog.taxa.messages.select_category') }}</h3>
                <p class="text-surface-500 text-center max-w-xs px-4 mt-2">
                    {{ t('catalog.taxa.messages.select_category_desc') }}
                </p>
            </div>
            <RouterView v-else :key="route.fullPath" />
        </div>
    </div>
  </div>
</template>

<style scoped>
:deep(.p-tree-node-content) {
    padding: 0 !important;
    background: transparent !important;
    box-shadow: none !important;
}
:deep(.p-tree-node-children) {
    padding-left: 1.5rem;
    border-left: 1px dashed var(--p-surface-200);
    margin-left: 0.75rem;
}
.scrollbar-thin::-webkit-scrollbar {
    width: 4px;
}
.scrollbar-thin::-webkit-scrollbar-thumb {
    background: var(--p-surface-200);
    border-radius: 4px;
}
</style>
