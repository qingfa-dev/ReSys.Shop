<script setup lang="ts">
import { onMounted, computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useInventoryStore } from '../../stores/inventory.store'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'
import { useApiErrorHandler } from '@/common/composables/api-error-handler.use'
import { useToast } from '@/common/composables/toast.use'
import { useConfirm } from 'primevue/useconfirm'
import type { TreeNode } from 'primevue/treenode'

const { t } = useI18n()

const route = useRoute()
const router = useRouter()
const confirm = useConfirm()
const store = useInventoryStore()
const { loading, locationTree, locations } = storeToRefs(store)
const { handleApiResult } = useApiErrorHandler()
const { showToast } = useToast()

const selectedId = computed(() => route.params.id as string)

onMounted(async () => {
  await store.fetchLocationTree()
  if (locations.value.length === 0) {
    await store.fetchLocations()
  }
})

interface LocationTreeNode {
  id: string; name: string; parentId?: string | null; children?: LocationTreeNode[]
}

const openNew = (parent?: LocationTreeNode) => {
  router.push({
    name: 'inventory.locations.create',
    query: parent ? { parentId: parent.id } : {}
  })
}

const openEdit = (node: LocationTreeNode) => {
  router.push({ name: 'inventory.locations.edit', params: { id: node.id } })
}

const confirmDelete = (node: LocationTreeNode) => {
  const messageStr = t('inventory.confirm.delete_message', { name: node.name });

  confirm.require({
    message: messageStr,
    header: t('inventory.confirm.delete_header'),
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: t('inventory.actions.cancel'),
    acceptLabel: t('inventory.actions.delete'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      const result = await store.inventoryService.deleteLocation(node.id)
      if (result.isSuccess) {
        showToast('success', t('common.deleted'), t('inventory.messages.delete_location_success'))
        await store.fetchLocationTree()
        await store.fetchLocations()
        if (selectedId.value === node.id) {
          router.push({ name: 'inventory.locations.list' })
        }
      } else {
        handleApiResult(result)
      }
    }
  })
}

const goBack = () => router.push({ name: 'inventory.stocks.list' })
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Header -->
    <div class="p-6 pb-0 max-w-full">
      <div class="flex items-center justify-between mt-4 mb-6">
        <div class="flex items-center gap-4">
          <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="goBack"
            class="bg-surface-100 dark:bg-surface-800" />
          <div>
            <h2 class="text-3xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
              {{ t('inventory.titles.locations') }}
            </h2>
            <p class="text-sm text-surface-500 m-0">{{ t('inventory.descriptions.manager') }}</p>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <Button :label="t('inventory.actions.new_location')" icon="pi pi-plus" size="small"
            class="rounded-xl shadow-lg" @click="openNew()" />
          <Button icon="pi pi-refresh" severity="secondary" text rounded @click="store.fetchLocationTree()"
            :loading="loading" />
        </div>
      </div>
    </div>

    <!-- Manager Layout -->
    <div class="flex flex-1 gap-6 p-6 pt-0 overflow-hidden min-h-[600px]">
      <!-- Sidebar (Tree) -->
      <div class="w-1/3 min-w-[320px] flex flex-col">
        <Card
          class="flex-1 border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden flex flex-col">
          <template #content>
            <div class="flex flex-col h-full">
              <div class="p-4 border-b border-surface-100 dark:border-surface-800 flex items-center justify-between">
                <span class="font-bold text-xs uppercase tracking-widest text-surface-400">{{
                  t('inventory.messages.hierarchy_view') }}</span>
                <Badge :value="locations.length" severity="secondary" />
              </div>

              <div class="flex-1 overflow-y-auto p-2 scrollbar-thin">
                <div v-if="loading && locationTree.length === 0"
                  class="flex flex-col items-center justify-center py-20">
                  <ProgressSpinner style="width: 40px; height: 40px" />
                </div>

                <Tree v-else :value="locationTree as unknown as TreeNode[]" selectionMode="single"
                  :pt="{ root: { class: 'bg-transparent border-none p-0' } }">
                  <template #default="{ node }: { node: any }">
                    <div
                      class="flex items-center justify-between w-full p-2 rounded-xl group cursor-pointer transition-colors"
                      :class="{ 'bg-primary/10 text-primary': selectedId === node.id, 'hover:bg-surface-100 dark:hover:bg-surface-800': selectedId !== node.id }"
                      @click="openEdit(node as any)">
                      <div class="flex items-center gap-3 overflow-hidden">
                        <i class="pi pi-building text-sm shrink-0"></i>
                        <span class="truncate font-medium text-sm">{{ node.name }}</span>
                      </div>
                      <div
                        class="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
                        <Button icon="pi pi-plus" text rounded size="small" severity="secondary"
                          @click.stop="openNew(node as any)" v-tooltip.top="t('inventory.actions.add_child')" />
                        <Button icon="pi pi-trash" text rounded size="small" severity="danger"
                          @click.stop="confirmDelete(node as any)" />
                      </div>
                    </div>
                  </template>
                </Tree>

                <div v-if="locationTree.length === 0 && !loading"
                  class="flex flex-col items-center justify-center py-20 text-center px-4">
                  <i class="pi pi-building text-4xl text-surface-200 mb-4"></i>
                  <p class="text-surface-400 text-sm italic">{{ t('inventory.messages.no_locations') }}</p>
                  <Button :label="t('inventory.actions.new_location')" text size="small" @click="openNew()" />
                </div>
              </div>
            </div>
          </template>
        </Card>
      </div>

      <!-- Main Content (Form/List) -->
      <div class="flex-1 overflow-hidden flex flex-col">
        <div v-if="route.name === 'inventory.locations.list'" class="flex-1">
          <RouterView />
        </div>
        <div
          v-else-if="route.name === 'inventory.locations.manager' || !route.params.id && route.name !== 'inventory.locations.create'"
          class="flex-1 flex flex-col items-center justify-center bg-surface-50/50 dark:bg-surface-950/20 rounded-3xl border-2 border-dashed border-surface-200 dark:border-surface-800">
          <div class="w-20 h-20 rounded-full bg-surface-100 dark:bg-surface-800 flex items-center justify-center mb-6">
            <i class="pi pi-sitemap text-4xl text-surface-300"></i>
          </div>
          <h3 class="text-xl font-bold text-surface-700 dark:text-surface-200">{{
            t('inventory.messages.select_location') }}
          </h3>
          <p class="text-surface-500 text-center max-w-xs px-4 mt-2">
            {{ t('inventory.messages.select_location_desc') }}
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
