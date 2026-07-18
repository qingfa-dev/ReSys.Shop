<script setup lang="ts">
import { onMounted, computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useTaxonomyStore } from '../stores/taxonomy.store'
import { storeToRefs } from 'pinia'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import { useToast } from '@/shared/composables/toast.use'
import { useConfirm } from 'primevue/useconfirm'
import type { TaxonomyListItem } from '../types/taxonomy.response.type'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const store = useTaxonomyStore()
const { loading, taxonomies, totalRecords } = storeToRefs(store)
const { handleApiResult } = useApiErrorHandler()
const { showToast } = useToast()

const selectedId = computed(() => route.params.id as string)

onMounted(async () => {
  await store.fetchTaxonomies({ pageSize: 100 })
})

const openNew = () => {
  router.push({ name: 'catalog.taxonomies.create' })
}

const openEdit = (id: string) => {
  router.push({ name: 'catalog.taxonomies.edit', params: { id } })
}

const confirmDelete = (item: TaxonomyListItem) => {
  const messageStr = t('catalog.taxonomies.confirm.delete_message').replace('{name}', item.name);
  
  confirm.require({
    message: messageStr,
    header: t('catalog.taxonomies.confirm.delete_header'),
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t('catalog.taxonomies.actions.cancel'),
    acceptLabel: t('catalog.taxonomies.actions.delete'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      const result = await store.deleteTaxonomy(item.id)
      if (result.isSuccess) {
        showToast('success', t('common.deleted'), t('catalog.taxonomies.messages.delete_success'))
        if (selectedId.value === item.id) {
            router.push({ name: 'catalog.taxonomies.list' })
        }
      } else {
        handleApiResult(result)
      }
    }
  })
}

const goBack = () => router.push({ name: 'catalog.dashboard' })
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="p-6 pb-0 max-w-full">
        <div class="flex items-center justify-between mt-4 mb-6">
            <div class="flex items-center gap-4">
                <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="goBack" class="bg-surface-100 dark:bg-surface-800" />
                <div>
                    <h2 class="text-3xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                        {{ t('catalog.taxonomies.titles.list') }}
                    </h2>
                    <p class="text-sm text-surface-500 m-0">{{ t('catalog.taxonomies.descriptions.list') }}</p>
                </div>
            </div>
            <div class="flex items-center gap-2">
                <Button :label="t('catalog.taxonomies.actions.create')" icon="pi pi-plus" size="small" class="rounded-xl shadow-lg" @click="openNew()" />
                <Button icon="pi pi-refresh" severity="secondary" text rounded @click="store.fetchTaxonomies({ pageSize: 100 })" :loading="loading" />
            </div>
        </div>
    </div>

    <div class="flex flex-1 gap-6 p-6 pt-0 overflow-hidden min-h-[600px]">
        <div class="w-1/3 min-w-[320px] flex flex-col">
            <Card class="flex-1 border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden flex flex-col">
                <template #content>
                    <div class="flex flex-col h-full">
                        <div class="p-4 border-b border-surface-100 dark:border-surface-800 flex items-center justify-between">
                            <span class="font-bold text-xs uppercase tracking-widest text-surface-400">Available Hierarchies</span>
                            <Badge :value="taxonomies.length" severity="secondary" />
                        </div>
                        
                        <div class="flex-1 overflow-y-auto p-2 scrollbar-thin">
                            <div v-if="loading && taxonomies.length === 0" class="flex flex-col items-center justify-center py-20">
                                <ProgressSpinner style="width: 40px; height: 40px" />
                            </div>
                            
                            <div v-else class="flex flex-col gap-1">
                                <div 
                                    v-for="tax in taxonomies" 
                                    :key="tax.id"
                                    class="flex items-center justify-between p-3 rounded-2xl group cursor-pointer transition-all duration-200"
                                    :class="{ 'bg-primary/10 text-primary border border-primary/20 shadow-sm': selectedId === tax.id, 'hover:bg-surface-100 dark:hover:bg-surface-800 border border-transparent': selectedId !== tax.id }"
                                    @click="openEdit(tax.id)"
                                >
                                    <div class="flex items-center gap-4 overflow-hidden">
                                        <div class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0" :class="selectedId === tax.id ? 'bg-primary text-white' : 'bg-surface-100 dark:bg-surface-800 text-surface-500'">
                                            <i class="pi pi-sitemap text-sm"></i>
                                        </div>
                                        <div class="flex flex-col overflow-hidden">
                                            <span class="truncate font-bold text-sm leading-tight">{{ tax.presentation || tax.name }}</span>
                                            <span class="text-[10px] uppercase font-black opacity-50 tracking-wider mt-0.5">{{ tax.taxonsCount }} Categories</span>
                                        </div>
                                    </div>
                                    <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
                                        <Button icon="pi pi-list" text rounded size="small" severity="info" @click.stop="router.push({ name: 'catalog.taxa.manager', params: { taxonomyId: tax.id } })" v-tooltip.top="'Manage Tree'" />
                                        <Button icon="pi pi-trash" text rounded size="small" severity="danger" @click.stop="confirmDelete(tax)" />
                                    </div>
                                </div>
                            </div>

                            <div v-if="taxonomies.length === 0 && !loading" class="flex flex-col items-center justify-center py-20 text-center px-4">
                                <i class="pi pi-sitemap text-4xl text-surface-200 mb-4"></i>
                                <p class="text-surface-400 text-sm italic">No root taxonomies defined yet.</p>
                                <Button label="Create your first hierarchy" text size="small" @click="openNew()" />
                            </div>
                        </div>
                    </div>
                </template>
            </Card>
        </div>

        <div class="flex-1 overflow-hidden flex flex-col">
            <RouterView :key="route.fullPath" />
        </div>
    </div>
  </div>
</template>

<style scoped>
.scrollbar-thin::-webkit-scrollbar {
    width: 4px;
}
.scrollbar-thin::-webkit-scrollbar-thumb {
    background: var(--p-surface-200);
    border-radius: 4px;
}
</style>
