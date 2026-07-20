<script setup lang="ts">
import { onMounted, computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createTaxonomySchema } from '../schemas/taxonomy.schema'
import { useTaxonomyStore } from '../stores/taxonomy.store'
import { useToast } from '@/shared/composables/toast.use'
import FormField from '@/shared/components/FormField.Component.vue'

const { t } = useI18n()

const route = useRoute()
const router = useRouter()
const store = useTaxonomyStore()
const { showToast } = useToast()

const isEdit = computed(() => !!route.params.id)
const itemId = computed(() => route.params.id as string)

const activeTab = ref(0)

const { defineField, errors, handleSubmit: submitForm, setValues } = useForm({
  validationSchema: toTypedSchema(createTaxonomySchema(t)),
  initialValues: {
    name: '',
    presentation: '',
    position: 0,
  },
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')
const [position] = defineField('position')

onMounted(async () => {
  store.clearCurrent()
  if (isEdit.value) {
    const result = await store.fetchTaxonomyById(itemId.value)
    if (result.isSuccess && result.value) {
      setValues({
        name: result.value.name,
        presentation: result.value.presentation || '',
        position: result.value.position,
      })

    }
  }
})

const onFormSubmit = submitForm(async (values) => {
  const payload = {
    ...values,
  }

  const result = isEdit.value
    ? await store.updateTaxonomy(itemId.value, payload)
    : await store.createTaxonomy(payload)

  if (result.isSuccess) {
    showToast(
      'success',
      t('common.success'),
      isEdit.value ? t('catalog.taxonomies.messages.update_success') : t('catalog.taxonomies.messages.create_success'),
    )
    if (!isEdit.value && result.value) {
        router.push({ name: 'catalog.taxonomies.edit', params: { id: result.value.id } })
    }
    store.fetchTaxonomies({ pageSize: 100 })
  }
})
</script>

<template>
  <div class="flex flex-col h-full overflow-hidden">
    <div class="flex items-center justify-between mb-4 bg-surface-0 dark:bg-surface-900 p-4 rounded-2xl border border-surface-100 dark:border-surface-800 shadow-sm">
        <div class="flex items-center gap-3 overflow-hidden">
            <div class="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary shrink-0">
                <i :class="isEdit ? 'pi pi-pencil' : 'pi pi-plus'"></i>
            </div>
            <div class="overflow-hidden">
                <h3 class="text-lg font-black tracking-tight m-0 truncate">{{ isEdit ? (presentation || 'Edit Taxonomy') : t('catalog.taxonomies.titles.create') }}</h3>
                <p class="text-xs text-surface-500 m-0 truncate">{{ isEdit ? t('catalog.taxonomies.descriptions.edit') : t('catalog.taxonomies.descriptions.create') }}</p>
            </div>
        </div>
        <div class="flex items-center gap-2 shrink-0">
            <Button 
                :label="isEdit ? t('catalog.taxonomies.actions.save_edit') : t('catalog.taxonomies.actions.save_create')" 
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
                    <Tab :value="0">{{ t('catalog.taxonomies.tabs.general') }}</Tab>
                </TabList>

                <TabPanels class="flex-1 overflow-y-auto p-6 scrollbar-thin">
                    <TabPanel :value="0">
                        <div class="flex flex-col gap-6">
                            <div class="grid grid-cols-1 gap-6">
                                <FormField :label="t('catalog.taxonomies.labels.name')" name="name" :error="errors.name">
                                    <InputText v-model="name" :placeholder="t('catalog.taxonomies.placeholders.name')" :invalid="!!errors.name" class="rounded-xl h-11" />
                                </FormField>

                                <FormField :label="t('catalog.taxonomies.labels.presentation')" name="presentation" :error="errors.presentation">
                                    <InputText v-model="presentation" :placeholder="t('catalog.taxonomies.placeholders.presentation')" :invalid="!!errors.presentation" class="rounded-xl h-11" />
                                </FormField>

                                <FormField :label="t('catalog.taxonomies.labels.position')" name="position" hint="Determine the display order in navigation menus.">
                                    <InputNumber v-model="position" showButtons :min="0" class="rounded-xl overflow-hidden" inputClass="h-11" />
                                </FormField>
                            </div>
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
.scrollbar-thin::-webkit-scrollbar {
    width: 4px;
}
.scrollbar-thin::-webkit-scrollbar-thumb {
    background: var(--p-surface-200);
    border-radius: 4px;
}
</style>
