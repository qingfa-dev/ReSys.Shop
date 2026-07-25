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
import Checkbox from 'primevue/checkbox'
import Button from 'primevue/button'
import { useUser } from '../composables/useUser'
import type { CreateUserForm, UpdateUserForm } from '../schemas'
import type { CreateUserRequest, UpdateUserRequest } from '../types'
import { UserForms } from '../schemas'
import { UserFormMapper } from '../mappers/user.mapper'
import { ROUTE } from '../routes'

const props = defineProps<{
  userType: 'staff' | 'customer'
}>()

const { id, mode, route, router, toast, api } = useUser()
const { t } = useI18n()

const schemas = new UserForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(
    mode.value === 'create' ? schemas.create() : schemas.update(),
  ),
})

const [email] = defineField('email')
const [userName] = defineField('userName')
const [password] = defineField('password')
const [firstName] = defineField('firstName')
const [lastName] = defineField('lastName')
const [phone] = defineField('phone')
const [isActive] = defineField('isActive')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const routeMap = { staff: ROUTE.STAFF, customer: ROUTE.CUSTOMERS }
const routes = routeMap[props.userType]

const title = computed(() => {
  if (mode.value === 'create') return `Create ${props.userType}`
  if (mode.value === 'edit') return `Edit: ${firstName.value || ''} ${lastName.value || ''}`
  return `${firstName.value || ''} ${lastName.value || ''}` || `${props.userType} details`
})

async function loadUser() {
  if (!id.value) return
  loading.value = true
  loadError.value = null
  const result = await api.get(id.value)
  if (result.isSuccess) {
    setValues({
      email: result.value.email,
      userName: result.value.userName,
      firstName: result.value.firstName,
      lastName: result.value.lastName,
      phone: result.value.phone ?? undefined,
      isActive: result.value.isActive ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load user'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const form = values as CreateUserForm | UpdateUserForm
  const data: CreateUserRequest | UpdateUserRequest = mode.value === 'create'
    ? UserFormMapper.toCreate(form as CreateUserForm)
    : UserFormMapper.toUpdate(form as UpdateUserForm)
  const result = id.value
    ? await api.update(id.value, data as UpdateUserRequest)
    : await api.create(data as CreateUserRequest)
  saving.value = false
  if (result.isSuccess) {
    toast.success(id.value ? 'User updated successfully' : 'User created successfully')
    const newId = result.value.id
    router.replace({ name: routes.VIEW, params: { id: newId } })
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  if (id.value) router.push({ name: routes.VIEW, params: { id: id.value } })
  else router.push({ name: routes.LIST })
}

function toggleEdit() {
  router.push({ name: routes.EDIT, params: { id: id.value } })
}

onMounted(async () => {
  await loadUser()
})
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
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
    <LoadingSkeleton v-if="loading && mode !== 'create'" :rows="8" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadUser" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField label="Email" :error="errors.email" required>
            <input v-model="email" type="email" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField label="Username" :error="errors.userName" required>
            <input v-model="userName" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div v-if="mode === 'create'" class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField label="Password" :error="errors.password" required>
            <input v-model="password" type="password" class="p-inputtext p-component w-full" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField label="First Name" :error="errors.firstName" required>
            <input v-model="firstName" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField label="Last Name" :error="errors.lastName" required>
            <input v-model="lastName" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField label="Phone" :error="errors.phone">
            <input v-model="phone" type="text" class="p-inputtext p-component w-full" :disabled="mode === 'view'" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField label="Active">
            <div class="flex items-center gap-2 mt-1">
              <Checkbox v-model="isActive" :binary="true" :disabled="mode === 'view'" input-id="isActive" />
              <label for="isActive">User is active</label>
            </div>
          </FormField>
        </div>
      </div>

      <div v-if="mode !== 'create' && id" class="mt-5 border-t border-surface-200 pt-5">
        <slot name="after-form" :user-id="id" />
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
