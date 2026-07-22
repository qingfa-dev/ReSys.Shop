<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { useToast } from '@/shared/composables/useToast'
import { getProduct, createProduct, updateProduct } from '../api/products'
import type { ProductResponse, ProductRequest, ProductStatus } from '../models/Product'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const id = computed(() => route.params.id as string | undefined)
const mode = computed<'create' | 'view' | 'edit'>(() => {
  if (!id.value) return 'create'
  if (route.name?.toString().endsWith('.edit')) return 'edit'
  return 'view'
})

const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)
const form = ref<ProductRequest & { status: ProductStatus }>({
  name: '', slug: '', description: null, status: 'Draft',
  styleCode: null, seasonName: null, department: null, genderTarget: null,
  metaTitle: null, metaDescription: null, metaKeywords: null,
})
const formErrors = ref<Record<string, string>>({})

const title = computed(() => {
  if (mode.value === 'create') return 'Create Product'
  if (mode.value === 'edit') return `Edit: ${form.value.name || 'Product'}`
  return form.value.name || 'Product Detail'
})

function validate(): boolean {
  formErrors.value = {}
  if (!form.value.name.trim()) formErrors.value.name = 'Required'
  if (!form.value.slug.trim()) formErrors.value.slug = 'Required'
  return Object.keys(formErrors.value).length === 0
}

async function load() {
  if (!id.value) return
  loading.value = true; error.value = null
  const result = await getProduct(id.value)
  if (result.success) { form.value = { ...result.data } }
  else { error.value = result.error?.message ?? 'Failed to load product' }
  loading.value = false
}

async function save() {
  if (!validate()) return
  saving.value = true
  const data: ProductRequest = { ...form.value }
  const result = id.value ? await updateProduct(id.value, data) : await createProduct(data)
  saving.value = false
  if (result.success) {
    toast.success(id.value ? 'Product updated' : 'Product created')
    if (mode.value === 'create') {
      router.replace({ name: 'catalog.products.view', params: { id: result.data.id } })
    } else {
      router.replace({ name: 'catalog.products.view', params: { id: id.value } })
    }
  } else { toast.error(result.error?.message ?? 'Save failed') }
}

function cancel() {
  if (id.value) router.push({ name: 'catalog.products.view', params: { id: id.value } })
  else router.push({ name: 'catalog.products.list' })
}

function toggleEdit() { router.push({ name: 'catalog.products.edit', params: { id: id.value } }) }

onMounted(() => { load() })
</script>

<template>
  <div>
    <PageHeader :title="title">
      <template #actions>
        <button v-if="mode === 'view'" class="p-button p-component" @click="toggleEdit">Edit</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="error" :title="error" @retry="load" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="Name" :error="formErrors.name" required>
            <input v-model="form.name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Slug" :error="formErrors.slug" required>
            <input v-model="form.slug" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Status">
            <select v-model="form.status" class="p-inputtext p-component w-full" :disabled="mode === 'view'">
              <option value="Draft">Draft</option>
              <option value="Active">Active</option>
              <option value="Archived">Archived</option>
            </select>
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Department">
            <input v-model="form.department" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-12">
          <FormField label="Description">
            <textarea v-model="form.description" rows="4" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? 'Create Product' : 'Save Changes'"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
