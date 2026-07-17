<script setup lang="ts">
import { onMounted, computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useOptionTypeStore } from '../stores/option-type.store'
import { storeToRefs } from 'pinia'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue'
import { useToast } from '@/shared/composables/toast.use'
import { useConfirm } from 'primevue/useconfirm'
import type { OptionTypeListItem } from '../types/OptionType.Response.Type'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const store = useOptionTypeStore()
const { loading, items, totalRecords } = storeToRefs(store)
const { handleApiResult } = useApiErrorHandler()
const { showToast } = useToast()

const selectedId = computed(() => route.params.id as string)

onMounted(async () => {
  await store.fetchList({ pageSize: 100 })
})

const openNew = () => {
  router.push({ name: 'catalog.option-types.create' })
}

const openEdit = (id: string) => {
  router.push({ name: 'catalog.option-types.edit', params: { id } })
}

const confirmDelete = (item: OptionTypeListItem) => {
  confirm.require({
    message: `Are you sure you want to delete "${item.name}"?`,
    header: t('common.warning'),
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t('catalog.option_types.actions.cancel'),
    acceptLabel: t('catalog.option_types.actions.delete'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      const result = await store.remove(item.id)
      if (result.isSuccess) {
        showToast('success', t('common.deleted'), t('catalog.option_types.messages.delete_success'))
        if (selectedId.value === item.id) {
            router.push({ name: 'catalog.option-types.list' })
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
        <AppBreadcrumb />
        <div class="flex items-center justify-between mt-4 mb-6">
            <div class="flex items-center gap-4">
                <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="goBack" class="bg-surface-100 dark:bg-surface-800" />
                <div>
                    <h2 class="text-3xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                        {{ t('catalog.option_types.titles.list') }}
                    </h2>
                    <p class="text-sm text-surface-500 m-0">{{ t('catalog.option_types.descriptions.list') }}</p>
                </div>
            </div>
            <div class="flex items-center gap-2">
                <Button :label="t('catalog.option_types.actions.create')" icon="pi pi-plus" size="small" class="rounded-xl shadow-lg" @click="openNew()" />
                <Button icon="pi pi-refresh" severity="secondary" text rounded @click="store.fetchList({ pageSize: 100 })" :loading="loading" />
            </div>
        </div>
    </div>

    <div class="flex flex-1 gap-6 p-6 pt-0 overflow-hidden min-h-[600px]">
        <div class="w-1/3 min-w-[320px] flex flex-col">
            <Card class="flex-1 border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden flex flex-col">
                <template #content>
                    <div class="flex flex-col h-full">
                        <div class="p-4 border-b border-surface-100 dark:border-surface-800 flex items-center justify-between">
                            <span class="font-bold text-xs uppercase tracking-widest text-surface-400">Available Options</span>
                            <Badge :value="items.length" severity="secondary" />
                        </div>
                        
                        <div class="flex-1 overflow-y-auto p-2 scrollbar-thin">
                            <div v-if="loading && items.length === 0" class="flex flex-col items-center justify-center py-20">
                                <ProgressSpinner style="width: 40px; height: 40px" />
                            </div>
                            
                            <div v-else class="flex flex-col gap-1">
                                <div 
                                    v-for="item in items" 
                                    :key="item.id"
                                    class="flex items-center justify-between p-3 rounded-2xl group cursor-pointer transition-all duration-200"
                                    :class="{ 'bg-primary/10 text-primary border border-primary/20 shadow-sm': selectedId === item.id, 'hover:bg-surface-100 dark:hover:bg-surface-800 border border-transparent': selectedId !== item.id }"
                                    @click="openEdit(item.id)"
                                >
                                    <div class="flex items-center gap-4 overflow-hidden">
                                        <div class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0" :class="selectedId === item.id ? 'bg-primary text-white' : 'bg-surface-100 dark:bg-surface-800 text-surface-500'">
                                            <i class="pi pi-list text-sm"></i>
                                        </div>
                                        <div class="flex flex-col overflow-hidden">
                                            <span class="truncate font-bold text-sm leading-tight">{{ item.presentation || item.name }}</span>
                                            <span class="text-[10px] uppercase font-black opacity-50 tracking-wider mt-0.5">Position {{ item.position }}</span>
                                        </div>
                                    </div>
                                    <div class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
                                        <Button icon="pi pi-trash" text rounded size="small" severity="danger" @click.stop="confirmDelete(item)" />
                                    </div>
                                </div>
                            </div>

                            <div v-if="items.length === 0 && !loading" class="flex flex-col items-center justify-center py-20 text-center px-4">
                                <i class="pi pi-box text-4xl text-surface-200 mb-4"></i>
                                <p class="text-surface-400 text-sm italic">No option types defined yet.</p>
                                <Button label="Create your first option" text size="small" @click="openNew()" />
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
.dark .scrollbar-thin::-webkit-scrollbar-thumb {
    background: var(--p-surface-700);
}
</style>
