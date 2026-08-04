<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import PageHeader from '@/shared/components/layout/PageHeader.vue'
import FormField from '@/shared/components/forms/FormField.vue'
import FormActions from '@/shared/components/forms/FormActions.vue'
import LoadingSkeleton from '@/shared/components/feedback/LoadingSkeleton.vue'
import ErrorState from '@/shared/components/feedback/ErrorState.vue'
import { AppCard } from '@/shared/components'
import Button from 'primevue/button'
import { useRole } from '../composables/useRole'
import { RoleForms } from '../schemas'
import { RoleFormMapper } from '../mappers/role.mapper'
import { ROUTE } from '../routes'

const { id, mode, route, router, toast, api } = useRole()
const { t } = useI18n()

const schemas = new RoleForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [name] = defineField('name')
const [description] = defineField('description')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const title = computed(() => {
  if (mode.value === 'create') return 'Create Role'
  if (mode.value === 'edit') return `Edit: ${name.value || ''}`
  return name.value || 'Role details'
})

async function loadRole() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await api.get(id.value)
  if (result.isSuccess) {
    setValues({
      name: result.value.name,
      description: result.value.description ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load role'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = mode.value === 'create'
    ? RoleFormMapper.toCreate(values)
    : RoleFormMapper.toUpdate(values)
  const result = id.value
    ? await api.update(id.value, data)
    : await api.create(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? 'Role updated successfully' : 'Role created successfully')
    const newId = result.value.id
    router.replace({ name: ROUTE.ROLES.VIEW, params: { id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: ROUTE.ROLES.VIEW, params: { id: id.value } })
  else router.push({ name: ROUTE.ROLES.LIST })
}

function toggleEdit() {
  router.push({ name: ROUTE.ROLES.EDIT, params: { id: id.value } })
}

onMounted(async () => {
  await loadRole()
})
</script>

<template>
  <div>
    <PageHeader :title="title" subtitle="" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <Button
          v-if="mode === 'view'"
          label="Edit"
          icon="pi pi-pencil"
          size="small"
          @click="toggleEdit"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="4" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadRole" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField label="Name" :error="errors.name" required>
            <input v-model="name" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField label="Description" :error="errors.description">
            <input v-model="description" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>

      <div v-if="mode !== 'create' && id" class="mt-5 border-t border-surface-200 pt-5">
        <slot name="after-form" :role-id="id" />
      </div>

      <FormActions
        v-if="mode !== 'view'"
        :loading="saving"
        :save-label="mode === 'create' ? 'Create' : 'Save Changes'"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
