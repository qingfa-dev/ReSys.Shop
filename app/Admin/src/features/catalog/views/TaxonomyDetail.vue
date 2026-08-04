<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Plus from '@primeicons/vue/plus'
import Card from 'primevue/card'
import Message from 'primevue/message'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { TaxonomyApi } from '../services/taxonomyApi'
import { taxonomySchema, taxonomyName, taxonomyPresentation, taxonomyPosition } from '../validations/taxonomy'
import type { TaxonomyForm } from '../validations/taxonomy'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Taxonomy' : 'New Taxonomy')
const pageDescription = computed(() =>
  isEdit.value
    ? 'Edit the details of the taxonomy.'
    : 'Create a new taxonomy by filling out the form below.',
)

const form = ref<TaxonomyForm>({
  name: '',
  presentation: '',
  position: 1,
})

const taxonomyResolver = zodResolver(taxonomySchema)
const nameResolver = zodResolver(taxonomyName)
const presentationResolver = zodResolver(taxonomyPresentation)
const positionResolver = zodResolver(taxonomyPosition)
const saving = ref(false)
const formLoaded = ref(!isEdit.value)

async function initEditMode(id: string) {
  // Load: Fetch the taxonomy to seed the edit form.
  const result = await TaxonomyApi.getTaxonomy(id)
  if (result.isSuccess) {
    const t = result.value
    form.value = {
      name: t.name,
      presentation: t.presentation,
      position: t.position,
    }
    formLoaded.value = true
  } else {
    handleResult(result)
    router.push('/catalog/taxonomies')
  }
}

onMounted(() => {
  if (isEdit.value) {
    initEditMode(route.params.id as string)
  }
})

watch(() => route.params.id, (newId) => {
  if (newId && newId !== 'new') {
    initEditMode(newId as string).then(() => {
      formLoaded.value = true
    })
  }
})

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return

  saving.value = true
  const data = event.values as TaxonomyForm
  const request = {
    name: data.name,
    presentation: data.presentation,
    position: data.position,
  }

  // Call: Persist the taxonomy, branching between update and create.
  const result = isEdit.value
    ? await TaxonomyApi.updateTaxonomy(route.params.id as string, request)
    : await TaxonomyApi.createTaxonomy(request)

  saving.value = false

  if (result.isSuccess) {
    notify.success(isEdit.value ? 'Taxonomy updated' : 'Taxonomy created')
    if (!isEdit.value && result.value) {
      const created = result.value
      form.value = {
        name: created.name,
        presentation: created.presentation,
        position: created.position,
      }
      router.replace(`/catalog/taxonomies/${created.id}`)
    }
  } else {
    handleResult(result)
  }
}

function onCancel() {
  router.push('/catalog/taxonomies')
}

function navigateToCreateTaxon() {
  router.push(`/catalog/taxons/new?taxonomyId=${route.params.id}`)
}
</script>

<template>
  <div class="flex flex-col h-full p-4">
    <!-- Section: Page Header — dynamic title plus Save and Cancel actions -->
    <div class="flex-none flex justify-between items-start gap-4 mb-4">
      <div>
        <div class="font-semibold text-xl">{{ pageTitle }}</div>
        <p v-if="pageDescription" class="text-muted-color mt-1">{{ pageDescription }}</p>
      </div>
      <div class="flex items-center gap-2 shrink-0">
        <Button label="Save" type="submit" icon="pi pi-check" severity="primary" :loading="saving" form="taxonomy-form" />
        <Button label="Cancel" type="button" icon="pi pi-times" severity="secondary" @click="onCancel()" />
      </div>
    </div>

    <div class="flex-1 min-h-0 overflow-auto">
      <!-- Section: Content Card — holds the taxonomy form fields -->
      <Card>
        <template #content>
          <Form id="taxonomy-form" :key="String(formLoaded)" :resolver="taxonomyResolver" :initial-values="form" class="flex flex-col gap-4" @submit="onSubmit">
              <!-- Section: Form Fields — name, presentation, and position -->
              <FormField v-slot="$field" name="name" :resolver="nameResolver" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Name <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="presentation" :resolver="presentationResolver" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Presentation <span class="text-red-500">*</span></label>
                <InputText fluid />
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
              <FormField v-slot="$field" name="position" :resolver="positionResolver" class="flex flex-col gap-1">
                <label class="text-surface-900 dark:text-surface-0 font-medium">Position</label>
                <InputNumber fluid :min="-1" />
                <small class="text-muted-color">Sort order (lower = first)</small>
                <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
              </FormField>
            </Form>
        </template>
      </Card>

      <!-- Section: Action Toolbar — edit-mode shortcut to create a taxon -->
      <Toolbar v-if="isEdit" class="mb-4 mt-4">
        <template #start>
          <Button label="Add Taxon" severity="secondary" @click="navigateToCreateTaxon()">
            <Plus />
          </Button>
        </template>
      </Toolbar>
    </div>
  </div>
</template>
