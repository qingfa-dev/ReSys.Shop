<script setup lang="ts">
import { onMounted, computed, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useTaxonStore } from '../stores/taxon.store'
import { useTaxonomyStore } from '../../stores/taxonomy.store'
import { storeToRefs } from 'pinia'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { TaxonSchema } from '../schemas/taxon.schema'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import { useToast } from '@/shared/composables/toast.use'
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue'
import MetadataManager from '@/shared/components/MetadataManager.Component.vue'
import TaxonRulesManagerComponent from '../components/TaxonRulesManager.Component.vue'
import TaxonProductsPreviewComponent from '../components/TaxonProductsPreview.Component.vue'
import { taxonService } from '../services/taxon.service'
import type { TaxonDetail } from '../types/taxon.types'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const taxonStore = useTaxonStore()
const taxonomyStore = useTaxonomyStore()
const { loading: storeLoading, currentRules } = storeToRefs(taxonStore)
const { currentItem: taxonomy } = storeToRefs(taxonomyStore)
const { handleApiResult } = useApiErrorHandler()
const { showToast } = useToast()

const taxonomyId = computed(() => route.params.taxonomyId as string)
const taxonId = computed(() => route.params.id as string)
const isEdit = computed(() => !!taxonId.value)
const parentIdParam = computed(() => route.query.parentId as string)

const activeTab = ref(0)
const actionLoading = ref(false)
const initialLoading = ref(false)
const previewRef = ref<any>(null)

const { defineField, handleSubmit, errors, setValues, resetForm, values: formValues } = useForm({
  validationSchema: toTypedSchema(TaxonSchema),
  initialValues: {
    taxonomyId: taxonomyId.value,
    name: '',
    presentation: '',
    description: '',
    slug: '',
    position: 0,
    hideFromNav: false,
    parentId: parentIdParam.value || null,
    automatic: false,
    rulesMatchPolicy: 'all',
    sortOrder: 'manual',
    metaTitle: '',
    metaDescription: '',
    metaKeywords: '',
  },
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')
const [description] = defineField('description')
const [slug] = defineField('slug')
const [position] = defineField('position')
const [hideFromNav] = defineField('hideFromNav')
const [automatic] = defineField('automatic')
const [rulesMatchPolicy] = defineField('rulesMatchPolicy')
const [sortOrder] = defineField('sortOrder')
const [metaTitle] = defineField('metaTitle')
const [metaDescription] = defineField('metaDescription')
const [metaKeywords] = defineField('metaKeywords')

const public_metadata = ref<Record<string, any>>({})
const private_metadata = ref<Record<string, any>>({})

const generateSlug = () => {
  if (!name.value || (isEdit.value && slug.value)) return
  slug.value = name.value
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '')
}

const loadData = async () => {
  initialLoading.value = true
  
  if (!taxonomy.value || taxonomy.value.id !== taxonomyId.value) {
    await taxonomyStore.fetchById(taxonomyId.value)
  }

  if (isEdit.value) {
    const result = await taxonService.getById(taxonomyId.value, taxonId.value)
    if (result.success && result.data) {
      setValues({
        taxonomyId: result.data.taxonomyId,
        name: result.data.name,
        presentation: result.data.presentation,
        description: result.data.description || '',
        slug: result.data.slug,
        position: result.data.position,
        hideFromNav: result.data.hideFromNav,
        parentId: result.data.parentId as any,
        automatic: result.data.automatic,
        rulesMatchPolicy: result.data.rulesMatchPolicy as any,
        sortOrder: result.data.sortOrder,
        metaTitle: result.data.metaTitle || '',
        metaDescription: result.data.metaDescription || '',
        metaKeywords: result.data.metaKeywords || '',
      })
      
      if (result.data.automatic) {
        await taxonStore.fetchRules(taxonomyId.value, taxonId.value)
      }
    } else {
      handleApiResult(result)
      router.push({ name: 'catalog.taxa.manager', params: { taxonomyId: taxonomyId.value } })
    }
  } else {
      resetForm({
          values: {
              taxonomyId: taxonomyId.value,
              name: '',
              presentation: '',
              description: '',
              slug: '',
              position: 0,
              hideFromNav: false,
              parentId: parentIdParam.value || null,
              automatic: false,
              rulesMatchPolicy: 'all',
              sortOrder: 'manual',
              metaTitle: '',
              metaDescription: '',
              metaKeywords: '',
          }
      })
  }
  
  initialLoading.value = false
}

watch([taxonId, isEdit], () => {
    loadData()
}, { immediate: false })

onMounted(() => {
  loadData()
})

const onFormSubmit = handleSubmit(async (values: any) => {
  actionLoading.value = true
  const payload = {
    ...values,
  }

  const result = isEdit.value
    ? await taxonStore.updateTaxon(taxonomyId.value, taxonId.value, payload)
    : await taxonStore.addTaxon(taxonomyId.value, payload)

  if (result.success) {
    showToast(
      'success',
      t('common.success') || 'Success',
      (isEdit.value
        ? t('catalog.taxa.messages.update_success')
        : t('catalog.taxa.messages.create_success')) || 'Success',
    )
    if (!isEdit.value && result.data) {
        router.push({ name: 'catalog.taxa.edit', params: { taxonomyId: taxonomyId.value, id: result.data.id } })
    } else {
        taxonStore.fetchTaxons(taxonomyId.value)
    }
  } else {
    handleApiResult(result)
  }
  actionLoading.value = false
})

const handleRulesUpdated = () => {
    if (previewRef.value) {
        previewRef.value.refresh()
    }
}

const goBack = () => router.push({ name: 'catalog.taxa.manager', params: { taxonomyId: taxonomyId.value } })
</script>

<template>
  <div class="flex flex-col h-full overflow-hidden">
    <div v-if="initialLoading" class="flex flex-col items-center justify-center py-32">
        <ProgressSpinner />
    </div>

    <div v-else class="flex flex-col h-full">
        <div class="flex items-center justify-between mb-4 bg-surface-0 dark:bg-surface-900 p-4 rounded-2xl border border-surface-100 dark:border-surface-800 shadow-sm">
            <div class="flex items-center gap-3 overflow-hidden">
                <div v-if="!initialLoading" class="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary shrink-0">
                    <i :class="isEdit ? 'pi pi-pencil' : 'pi pi-plus'"></i>
                </div>
                <div v-if="!initialLoading" class="overflow-hidden">
                    <h3 class="text-lg font-black tracking-tight m-0 truncate">{{ isEdit ? presentation : t('catalog.taxa.titles.create') }}</h3>
                    <p class="text-xs text-surface-500 m-0 truncate">{{ isEdit ? t('catalog.taxa.descriptions.automation_edit') : t('catalog.taxa.descriptions.automation_create') }}</p>
                </div>
            </div>
            <div class="flex items-center gap-2 shrink-0">
                <Button 
                    :label="isEdit ? t('catalog.taxa.actions.save') : t('catalog.taxa.actions.create')" 
                    icon="pi pi-check" 
                    class="rounded-xl px-6 shadow-lg shadow-primary/20" 
                    :loading="actionLoading"
                    @click="onFormSubmit" 
                />
            </div>
        </div>

        <Card class="flex-1 border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden flex flex-col">
          <template #content>
            <div class="flex flex-col h-full">
                <Tabs v-model:value="activeTab" class="flex-1 flex flex-col overflow-hidden">
                    <TabList class="shrink-0">
                        <Tab :value="0">{{ t('catalog.taxa.tabs.general') }}</Tab>
                        <Tab :value="3">{{ t('catalog.taxa.tabs.seo') }}</Tab>
                        <Tab :value="4">{{ t('catalog.taxa.tabs.metadata') }}</Tab>
                    </TabList>

                    <TabPanels class="flex-1 overflow-y-auto p-6 scrollbar-thin">
                        <TabPanel :value="0">
                            <div class="flex flex-col gap-6">
                                <div class="grid grid-cols-1 gap-6">
                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500">{{ t('catalog.taxa.labels.name') }}</label>
                                        <InputText v-model="name" class="w-full rounded-xl h-11" :invalid="!!errors.name" @blur="generateSlug" />
                                        <small class="text-red-500" v-if="errors.name">{{ errors.name }}</small>
                                    </div>

                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500">{{ t('catalog.taxa.labels.presentation') }}</label>
                                        <InputText v-model="presentation" class="w-full rounded-xl h-11" :invalid="!!errors.presentation" />
                                        <small class="text-red-500" v-if="errors.presentation">{{ errors.presentation }}</small>
                                    </div>

                                    <div class="grid grid-cols-2 gap-4">
                                        <div class="flex flex-col gap-2">
                                            <label class="font-bold text-xs uppercase tracking-wider text-surface-500">{{ t('catalog.taxa.labels.slug') }}</label>
                                            <InputText v-model="slug" class="w-full font-mono text-sm rounded-xl h-11" :invalid="!!errors.slug" />
                                        </div>
                                        <div class="flex flex-col gap-2">
                                            <label class="font-bold text-xs uppercase tracking-wider text-surface-500">{{ t('catalog.taxa.labels.position') }}</label>
                                            <InputNumber v-model="position" showButtons :min="0" class="w-full rounded-xl overflow-hidden" inputClass="h-11" />
                                        </div>
                                    </div>

                                    <div class="flex flex-col gap-2">
                                        <label class="font-bold text-xs uppercase tracking-wider text-surface-500">{{ t('catalog.taxa.labels.description') }}</label>
                                        <Textarea v-model="description" rows="3" class="w-full rounded-xl" />
                                    </div>

                                    <div class="p-4 bg-surface-50 dark:bg-surface-800/50 rounded-2xl border border-surface-100 dark:border-surface-800 flex items-center justify-between mt-2">
                                        <span class="font-bold text-sm">{{ t('catalog.taxa.labels.hide_from_nav') }}</span>
                                        <ToggleSwitch v-model="hideFromNav" />
                                    </div>
                                </div>
                            </div>
                        </TabPanel>

                        <TabPanel :value="3">
                            <div class="flex flex-col gap-6">
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-xs uppercase text-surface-500">{{ t('catalog.taxa.labels.meta_title') }}</label>
                                    <InputText v-model="metaTitle" class="w-full rounded-xl" :placeholder="t('catalog.taxa.placeholders.meta_title')" />
                                </div>
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-xs uppercase text-surface-500">{{ t('catalog.taxa.labels.meta_description') }}</label>
                                    <Textarea v-model="metaDescription" rows="3" class="w-full rounded-xl" :placeholder="t('catalog.taxa.placeholders.meta_description')" />
                                </div>
                            </div>
                        </TabPanel>

                        <TabPanel :value="4">
                            <div class="flex flex-col gap-8">
                                <MetadataManager v-model="public_metadata" :title="t('catalog.taxa.labels.public_metadata')" />
                                <MetadataManager v-model="private_metadata" :title="t('catalog.taxa.labels.private_metadata')" />
                            </div>
                        </TabPanel>
                    </TabPanels>
                </Tabs>
            </div>
          </template>
        </Card>
    </div>
  </div>
</template>

<style scoped>
.animate-fadein {
    animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}

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
