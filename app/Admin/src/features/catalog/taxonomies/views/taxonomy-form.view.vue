<script setup lang="ts">
import { onMounted, computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { TaxonomySchema } from '../schemas/taxonomy.schema'
import { useTaxonomyStore } from '../stores/taxonomy.store'
import { taxonomyLocales } from '../locales/taxonomy.locales'
import { useToast } from '@/shared/composables/toast.use'
import type { FeatureLocales } from '@/shared/locales/locale.types'
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue'
import MetadataManager from '@/shared/components/metadata-manager.component.vue'

// --- LOCALES & ALIASES ---
const t = taxonomyLocales as any

// --- STORE & ROUTING ---
const route = useRoute()
const router = useRouter()
const store = useTaxonomyStore()
const { showToast } = useToast()

const isEdit = computed(() => !!route.params.id)
const itemId = computed(() => route.params.id as string)

const activeTab = ref(0)

// --- FORM SETUP ---
const { defineField, errors, handleSubmit: submitForm, setValues } = useForm({
  validationSchema: toTypedSchema(TaxonomySchema),
  initialValues: {
    name: '',
    presentation: '',
    position: 0,
  },
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')
const [position] = defineField('position')

const public_metadata = ref<Record<string, any>>({})
const private_metadata = ref<Record<string, any>>({})

onMounted(async () => {
  store.clearCurrent()
  if (isEdit.value) {
    const result = await store.fetchTaxonomyById(itemId.value)
    if (result.success && result.data) {
      setValues({
        name: result.data.name,
        presentation: result.data.presentation || '',
        position: result.data.position,
      })
      public_metadata.value = result.data.public_metadata || {}
      private_metadata.value = result.data.private_metadata || {}
    }
  }
})

// --- ACTIONS ---
const onFormSubmit = submitForm(async (values) => {
  const payload = {
    ...values,
    public_metadata: public_metadata.value,
    private_metadata: private_metadata.value,
  }

  const result = isEdit.value
    ? await store.updateTaxonomy(itemId.value, payload)
    : await store.createTaxonomy(payload)

  if (result.success) {
    showToast(
      'success',
      t.common?.success || 'Success',
      (isEdit.value ? t.messages?.update_success : t.messages?.create_success) || 'Success',
    )
    if (!isEdit.value && result.data) {
        router.push({ name: 'catalog.taxonomies.edit', params: { id: result.data.id } })
    }
    store.fetchTaxonomies({ page_size: 100 }) // Refresh sidebar list
  }
})
</script>

<template>
  <div class="flex flex-col h-full overflow-hidden">
    <!-- Compact Header for Split View -->
    <div class="flex items-center justify-between mb-4 bg-surface-0 dark:bg-surface-900 p-4 rounded-2xl border border-surface-100 dark:border-surface-800 shadow-sm">
        <div class="flex items-center gap-3 overflow-hidden">
            <div class="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary shrink-0">
                <i :class="isEdit ? 'pi pi-pencil' : 'pi pi-plus'"></i>
            </div>
            <div class="overflow-hidden">
                <h3 class="text-lg font-black tracking-tight m-0 truncate">{{ isEdit ? (presentation || 'Edit Taxonomy') : t.titles?.create }}</h3>
                <p class="text-xs text-surface-500 m-0 truncate">{{ isEdit ? t.descriptions?.edit : t.descriptions?.create }}</p>
            </div>
        </div>
        <div class="flex items-center gap-2 shrink-0">
            <Button 
                :label="isEdit ? t.actions?.save_edit : t.actions?.save_create" 
                icon="pi pi-check" 
                class="rounded-xl px-6 shadow-lg shadow-primary/20" 
                :loading="store.submitting"
                @click="onFormSubmit" 
            />
        </div>
    </div>

    <Card class="flex-1 border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden flex flex-col">
        <template #content>
          <div class="flex flex-col h-full">
            <Tabs v-model:value="activeTab" class="flex-1 flex flex-col overflow-hidden">
                <TabList class="shrink-0">
                    <Tab :value="0">{{ (t as any).tabs.general }}</Tab>
                    <Tab :value="1">{{ (t as any).tabs.metadata }}</Tab>
                </TabList>

                <TabPanels class="flex-1 overflow-y-auto p-6 scrollbar-thin">
                    <TabPanel :value="0">
                        <div class="flex flex-col gap-6">
                            <div class="grid grid-cols-1 gap-6">
                                <div class="flex flex-col gap-2">
                                    <label for="name" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels.name }}</label>
                                    <InputText id="name" v-model="name" :placeholder="t.placeholders?.name" :invalid="!!errors.name" class="rounded-xl h-11" />
                                    <small class="text-red-500 ml-1" v-if="errors.name">{{ errors.name }}</small>
                                </div>

                                <div class="flex flex-col gap-2">
                                    <label for="presentation" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels.presentation }}</label>
                                    <InputText id="presentation" v-model="presentation" :placeholder="t.placeholders?.presentation" :invalid="!!errors.presentation" class="rounded-xl h-11" />
                                    <small class="text-red-500 ml-1" v-if="errors.presentation">{{ errors.presentation }}</small>
                                </div>

                                <div class="flex flex-col gap-2">
                                    <label for="position" class="font-bold text-xs uppercase tracking-wider text-surface-500 ml-1">{{ t.labels.position }}</label>
                                    <InputNumber id="position" v-model="position" showButtons :min="0" class="rounded-xl overflow-hidden" inputClass="h-11" />
                                    <p class="text-xs text-surface-400 mt-1 ml-1 italic">Determine the display order in navigation menus.</p>
                                </div>
                            </div>
                        </div>
                    </TabPanel>

                    <TabPanel :value="1">
                        <div class="flex flex-col gap-8">
                            <MetadataManager v-model="public_metadata" :title="(t.labels as any).public_metadata" />
                            <Divider />
                            <MetadataManager v-model="private_metadata" :title="(t.labels as any).private_metadata" />
                        </div>
                    </TabPanel>
                </TabPanels>
            </Tabs>
          </div>
        </template>
    </Card>
  </div>
</template>

<style scoped>
:deep(.p-tabs-list) {
    border-bottom: 1px solid var(--p-surface-100);
}
.dark :deep(.p-tabs-list) {
    border-bottom-color: var(--p-surface-800);
}
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