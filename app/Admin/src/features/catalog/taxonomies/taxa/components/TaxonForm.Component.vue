<script setup lang="ts">
import { ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { TaxonSchema } from '../schemas/taxon.schema'
import MetadataManager from '@/shared/components/MetadataManager.Component.vue'
import type { TaxonDetail } from '../types/taxon.types'

const { t } = useI18n()

const props = defineProps<{
  initialData?: TaxonDetail | null
  taxonomyId: string
  parentId?: string | null
}>()

const emit = defineEmits<{
  (e: 'submit', values: any): void
  (e: 'cancel'): void
}>()

const { defineField, handleSubmit, errors, setValues, resetForm, values: formValues } = useForm({
  validationSchema: toTypedSchema(TaxonSchema),
  initialValues: {
    taxonomyId: props.taxonomyId,
    name: '',
    presentation: '',
    description: '',
    slug: '',
    position: 0,
    hideFromNav: false,
    parentId: props.parentId || null,
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
const [metaTitle] = defineField('metaTitle')
const [metaDescription] = defineField('metaDescription')
const [metaKeywords] = defineField('metaKeywords')

const public_metadata = ref<Record<string, any>>({})
const private_metadata = ref<Record<string, any>>({})

const generateSlug = () => {
  if (!name.value || (props.initialData && slug.value)) return
  slug.value = name.value
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '')
}

watch(
  () => props.initialData,
  (newData) => {
    if (newData) {
      setValues({
        taxonomyId: newData.taxonomyId,
        name: newData.name,
        presentation: newData.presentation,
        description: newData.description || '',
        slug: newData.slug,
        position: newData.position,
        hideFromNav: newData.hideFromNav,
        parentId: newData.parentId as any,
        automatic: newData.automatic,
        rulesMatchPolicy: newData.rulesMatchPolicy as any,
        sortOrder: newData.sortOrder,
        metaTitle: newData.metaTitle || '',
        metaDescription: newData.metaDescription || '',
        metaKeywords: newData.metaKeywords || '',
      })
    } else {
      resetForm({
        values: {
          taxonomyId: props.taxonomyId,
          name: '',
          presentation: '',
          description: '',
          slug: '',
          position: 0,
          hideFromNav: false,
          parentId: props.parentId || null,
          automatic: false,
          rulesMatchPolicy: 'all',
          sortOrder: 'manual',
          metaTitle: '',
          metaDescription: '',
          metaKeywords: '',
        }
      })
    }
  },
  { immediate: true },
)

const onSubmit = handleSubmit((values) => {
  emit('submit', {
    ...values,
    public_metadata: public_metadata.value,
    private_metadata: private_metadata.value,
  })
})
</script>

<template>
  <form @submit="onSubmit" class="flex flex-col gap-6">
    <Tabs value="0">
      <TabList>
        <Tab value="0">{{ t('catalog.taxa.tabs.general') }}</Tab>
        <Tab value="1">{{ t('catalog.taxa.tabs.seo') }}</Tab>
        <Tab value="2">{{ t('catalog.taxa.tabs.metadata') }}</Tab>
      </TabList>
      <TabPanels>
        <TabPanel value="0">
          <div class="flex flex-col gap-6 py-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div class="flex flex-col gap-2">
                <label class="font-bold text-sm text-surface-700 dark:text-surface-300">
                  {{ t('catalog.taxa.labels.name') }}
                </label>
                <InputText v-model="name" class="w-full rounded-xl" :invalid="!!errors.name" @blur="generateSlug" />
                <small class="text-red-500" v-if="errors.name">{{ errors.name }}</small>
              </div>

              <div class="flex flex-col gap-2">
                <label class="font-bold text-sm text-surface-700 dark:text-surface-300">
                  {{ t('catalog.taxa.labels.presentation') }}
                </label>
                <InputText v-model="presentation" class="w-full rounded-xl" :invalid="!!errors.presentation" />
                <small class="text-red-500" v-if="errors.presentation">{{ errors.presentation }}</small>
              </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div class="flex flex-col gap-2">
                <label class="font-bold text-sm text-surface-700 dark:text-surface-300">
                  {{ t('catalog.taxa.labels.slug') }}
                </label>
                <InputText v-model="slug" class="w-full font-mono text-sm rounded-xl" :invalid="!!errors.slug" />
                <small class="text-red-500" v-if="errors.slug">{{ errors.slug }}</small>
              </div>

              <div class="flex flex-col gap-2">
                <label class="font-bold text-sm text-surface-700 dark:text-surface-300">
                  {{ t('catalog.taxa.labels.position') }}
                </label>
                <InputNumber v-model="position" showButtons :min="0" class="w-full rounded-xl" />
              </div>
            </div>

            <div class="flex flex-col gap-2">
              <label class="font-bold text-sm text-surface-700 dark:text-surface-300">
                {{ t('catalog.taxa.labels.description') }}
              </label>
              <Textarea v-model="description" rows="3" class="w-full rounded-xl" />
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
               <div class="flex items-center justify-between p-4 bg-surface-50 dark:bg-surface-800 rounded-xl border border-surface-100 dark:border-surface-700">
                <div class="flex flex-col gap-0.5">
                  <span class="font-bold text-sm text-surface-700 dark:text-surface-200">
                    {{ t('catalog.taxa.labels.hide_from_nav') }}
                  </span>
                  <small class="text-surface-500 text-xs text-pretty">{{ t('catalog.taxa.descriptions.hide_from_nav') }}</small>
                </div>
                <ToggleSwitch v-model="hideFromNav" />
              </div>

              <div class="flex items-center justify-between p-4 bg-surface-50 dark:bg-surface-800 rounded-xl border border-surface-100 dark:border-surface-700">
                <div class="flex flex-col gap-0.5">
                  <span class="font-bold text-sm text-surface-700 dark:text-surface-200">
                    {{ t('catalog.taxa.labels.automatic') }}
                  </span>
                  <small class="text-surface-500 text-xs text-pretty">{{ t('catalog.taxa.descriptions.automatic') }}</small>
                </div>
                <ToggleSwitch v-model="automatic" />
              </div>
            </div>

            <div v-if="automatic" class="p-4 bg-primary/5 rounded-xl border border-primary/10 flex flex-col gap-4">
              <div class="flex flex-col gap-2">
                <label class="font-bold text-sm text-primary">{{ t('catalog.taxa.labels.rules_policy') }}</label>
                <SelectButton v-model="rulesMatchPolicy" :options="[{label: t('catalog.taxa.labels.rules_policy_all'), value: 'all'}, {label: t('catalog.taxa.labels.rules_policy_any'), value: 'any'}]" optionLabel="label" optionValue="value" class="w-full" />
              </div>
            </div>
          </div>
        </TabPanel>

        <TabPanel value="1">
          <div class="flex flex-col gap-6 py-4">
            <div class="flex flex-col gap-2">
              <label class="font-bold text-sm text-surface-700 dark:text-surface-300">{{ t('catalog.taxa.labels.meta_title') }}</label>
              <InputText v-model="metaTitle" class="w-full rounded-xl" :placeholder="t('catalog.taxa.placeholders.meta_title')" />
            </div>

            <div class="flex flex-col gap-2">
              <label class="font-bold text-sm text-surface-700 dark:text-surface-300">{{ t('catalog.taxa.labels.meta_description') }}</label>
              <Textarea v-model="metaDescription" rows="3" class="w-full rounded-xl" :placeholder="t('catalog.taxa.placeholders.meta_description')" />
            </div>

            <div class="flex flex-col gap-2">
              <label class="font-bold text-sm text-surface-700 dark:text-surface-300">{{ t('catalog.taxa.labels.meta_keywords') }}</label>
              <InputText v-model="metaKeywords" class="w-full rounded-xl" :placeholder="t('catalog.taxa.placeholders.meta_keywords')" />
            </div>
          </div>
        </TabPanel>

        <TabPanel value="2">
          <div class="py-4 flex flex-col gap-8">
            <MetadataManager v-model="public_metadata" :title="t('catalog.taxa.labels.public_metadata')" />
            <MetadataManager v-model="private_metadata" :title="t('catalog.taxa.labels.private_metadata')" />
          </div>
        </TabPanel>
      </TabPanels>
    </Tabs>

    <div class="flex justify-end gap-3 pt-4 border-t border-surface-100 dark:border-surface-800">
      <Button type="button" :label="t('catalog.taxa.actions.cancel')" severity="secondary" text @click="emit('cancel')" class="rounded-xl" />
      <Button type="submit" :label="t('catalog.taxa.actions.save')" icon="pi pi-check" class="rounded-xl px-8 shadow-lg shadow-primary/20" />
    </div>
  </form>
</template>
