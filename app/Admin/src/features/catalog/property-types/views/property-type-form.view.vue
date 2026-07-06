<script setup lang="ts">
import { onMounted, computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePropertyTypeStore } from '../stores/property-type.store'
import { storeToRefs } from 'pinia'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { PropertyTypeSchema } from '../schemas/property-type.schema'
import { propertyTypeLocales } from '../locales/property-type.locales'
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use'
import AppBreadcrumb from '@/shared/components/breadcrumb.component.vue'
import { PropertyKind, PropertyKindOptions } from '../types/property-kind'
import MetadataManager from '@/shared/components/metadata-manager.component.vue'

const route = useRoute()
const router = useRouter()
const store = usePropertyTypeStore()
const { loading } = storeToRefs(store)
const { handleApiResult } = useApiErrorHandler()

const isEdit = computed(() => route.params.id !== undefined)
const itemId = computed(() => route.params.id as string)

// Metadata state
const publicMetadata = ref<Record<string, any>>({})
const privateMetadata = ref<Record<string, any>>({})

const { defineField, handleSubmit, errors, setValues, setErrors, values } = useForm({
  validationSchema: toTypedSchema(PropertyTypeSchema),
  initialValues: {
    name: '',
    presentation: '',
    kind: PropertyKind.String,
    position: 0,
    filterable: false
  },
})

const [name] = defineField('name')
const [presentation] = defineField('presentation')
const [kind] = defineField('kind')
const [position] = defineField('position')
const [filterable] = defineField('filterable')

const loadItem = async () => {
  if (!isEdit.value) {
    store.clearCurrent()
    publicMetadata.value = {}
    privateMetadata.value = {}
    return
  }

  const result = await store.fetchById(itemId.value)
  const handled = handleApiResult(result, { genericError: 'Failed to load property type' })

  if (handled && result.data) {
    setValues({
      name: result.data.name,
      presentation: result.data.presentation,
      kind: result.data.kind,
      position: result.data.position,
      filterable: result.data.filterable
    })
    publicMetadata.value = result.data.publicMetadata || {}
    privateMetadata.value = result.data.privateMetadata || {}
  } else if (!handled) {
    router.push({ name: 'catalog.property-types.list' })
  }
}

const onSubmit = handleSubmit(async (formValues) => {
  const payload = {
    ...formValues,
    publicMetadata: publicMetadata.value,
    privateMetadata: privateMetadata.value
  }

  const result = isEdit.value
    ? await store.update(itemId.value, payload)
    : await store.create(payload)

  const handled = handleApiResult(result, {
    setErrors,
    fieldNames: Object.keys(values),
    successTitle: propertyTypeLocales.common?.success,
    successMessage: isEdit.value 
        ? propertyTypeLocales.messages?.update_success 
        : propertyTypeLocales.messages?.create_success,
    errorTitle: propertyTypeLocales.common?.error,
  })

  if (handled) {
    router.push({ name: 'catalog.property-types.list' })
  }
})

onMounted(() => {
  loadItem()
})

const cancel = () => {
  router.push({ name: 'catalog.property-types.list' })
}
</script>

<template>
  <div class="max-w-4xl p-6 mx-auto">
    <div class="mb-6">
      <AppBreadcrumb :locales="propertyTypeLocales" />

      <div class="flex flex-col justify-between gap-4 md:flex-row md:items-center">
        <div class="flex items-center gap-4">
          <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="cancel" class="bg-surface-100 dark:bg-surface-800" />
          <div>
            <h2 class="text-3xl font-black tracking-tight text-surface-900 dark:text-surface-50">
              {{ isEdit ? propertyTypeLocales.titles?.edit : propertyTypeLocales.titles?.create }}
            </h2>
            <p class="text-sm text-surface-500 dark:text-surface-400">
              {{ isEdit ? 'Update existing property type details' : propertyTypeLocales.descriptions?.create }}
            </p>
          </div>
        </div>
        <div class="flex gap-2">
          <Button :label="propertyTypeLocales.actions?.cancel" severity="secondary" text @click="cancel" />
          <Button 
            type="button"
            :label="isEdit ? propertyTypeLocales.actions?.save_edit : propertyTypeLocales.actions?.save_create" 
            icon="pi pi-check" 
            :loading="loading" 
            @click="onSubmit"
            class="px-6 shadow-lg rounded-xl"
          />
        </div>
      </div>
    </div>

    <Tabs value="0">
        <TabList>
            <Tab value="0">Basic Details</Tab>
            <Tab value="1">Metadata</Tab>
        </TabList>
        <TabPanels>
            <TabPanel value="0">
                <form @submit="onSubmit" class="flex flex-col gap-6 mt-4">
                    <Card class="border-none shadow-sm rounded-2xl bg-surface-0 dark:bg-surface-900">
                        <template #title>
                            <span class="text-xl font-bold text-surface-800 dark:text-surface-50">{{ propertyTypeLocales.titles?.basic_info }}</span>
                        </template>
                        <template #content>
                            <div class="flex flex-col gap-6 pt-2">
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                                    <div class="flex flex-col gap-2">
                                        <label for="name" class="font-bold text-surface-700 dark:text-surface-200">{{ propertyTypeLocales.labels?.name }}</label>
                                        <InputText id="name" v-model="name" class="w-full rounded-xl" :invalid="!!errors.name" :placeholder="propertyTypeLocales.placeholders?.name" />
                                        <small class="text-red-500 font-medium" v-if="errors.name">{{ errors.name }}</small>
                                    </div>

                                    <div class="flex flex-col gap-2">
                                        <label for="presentation" class="font-bold text-surface-700 dark:text-surface-200">{{ propertyTypeLocales.labels?.presentation }}</label>
                                        <InputText id="presentation" v-model="presentation" class="w-full rounded-xl" :invalid="!!errors.presentation" :placeholder="propertyTypeLocales.placeholders?.presentation" />
                                        <small class="text-red-500 font-medium" v-if="errors.presentation">{{ errors.presentation }}</small>
                                    </div>
                                </div>

                                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                                    <div class="flex flex-col gap-2">
                                        <label for="kind" class="font-bold text-surface-700 dark:text-surface-200">{{ propertyTypeLocales.labels?.kind }}</label>
                                        <Select id="kind" v-model="kind" :options="PropertyKindOptions" optionLabel="label" optionValue="value" class="w-full rounded-xl" :invalid="!!errors.kind" />
                                        <small class="text-red-500 font-medium" v-if="errors.kind">{{ errors.kind }}</small>
                                    </div>

                                    <div class="flex flex-col gap-2">
                                        <label for="position" class="font-bold text-surface-700 dark:text-surface-200">{{ propertyTypeLocales.labels?.position }}</label>
                                        <InputNumber id="position" v-model="position" class="w-full rounded-xl" :invalid="!!errors.position" showButtons :min="0" />
                                        <small class="text-red-500 font-medium" v-if="errors.position">{{ errors.position }}</small>
                                    </div>
                                </div>

                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-surface-700 dark:text-surface-200">{{ propertyTypeLocales.labels?.filterable }}</label>
                                    <div class="flex items-center gap-2">
                                        <ToggleSwitch v-model="filterable" />
                                        <span class="text-sm text-surface-500">{{ filterable ? 'Enabled' : 'Disabled' }}</span>
                                    </div>
                                </div>
                            </div>
                        </template>
                    </Card>
                </form>
            </TabPanel>

            <TabPanel value="1">
                <div class="flex flex-col gap-6 mt-4">
                    <Card class="border-none shadow-sm rounded-2xl bg-surface-0 dark:bg-surface-900">
                        <template #content>
                            <MetadataManager 
                                v-model="publicMetadata" 
                                title="Public Metadata" 
                                description="Information visible to customers and external systems." 
                            />
                            <Divider class="my-8" />
                            <MetadataManager 
                                v-model="privateMetadata" 
                                title="Private Metadata" 
                                description="Internal notes and configuration hidden from customers." 
                            />
                        </template>
                    </Card>
                </div>
            </TabPanel>
        </TabPanels>
    </Tabs>
  </div>
</template>