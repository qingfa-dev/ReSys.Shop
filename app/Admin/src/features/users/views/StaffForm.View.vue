<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from '@/shared/composables/toast.use'
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createStaffSchema } from '../schemas/staff.schema'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import FormField from '@/shared/components/FormField.Component.vue'
import { userService } from '../services/user.service'
import { roleService } from '../roles/services/role.service'
import type { CreateAdminUserRequest, UpdateAdminUserRequest } from '../types/user.request.type'

const route = useRoute()
const router = useRouter()
const { showToast } = useToast()
const { t } = useI18n()

const isEditMode = computed(() => !!route.params.id)
const userId = computed(() => route.params.id as string)
const loading = ref(false)
const submitting = ref(false)
const roleOptions = ref<{ label: string, value: string }[]>([])

const { defineField, handleSubmit, errors, setValues } = useForm({
  validationSchema: toTypedSchema(createStaffSchema(t)),
  initialValues: {
    email: '',
    displayName: '',
    roleIds: [],
    isActive: true,
  },
})

const [email] = defineField('email')
const [displayName] = defineField('displayName')
const [roleIds] = defineField('roleIds')
const [isActive] = defineField('isActive')
const [password] = defineField('password')

onMounted(async () => {
  loading.value = true
  try {
    await fetchRoles()
    if (isEditMode.value) {
      await loadUser()
    }
  } finally {
    loading.value = false
  }
})

async function fetchRoles() {
  const res = await roleService.list({ pageSize: 100 })
  if (res.isSuccess && res.items) {
    roleOptions.value = res.items.map(r => ({
      label: r.displayName || r.name,
      value: r.name
    }))
  }
}

async function loadUser() {
  const res = await userService.getById(userId.value)
  if (res.isSuccess && res.value) {
    const user = res.value
    setValues({
      email: user.email,
      displayName: [user.firstName, user.lastName].filter(Boolean).join(' ') || user.email,
      roleIds: (user as any).roles || [],
      isActive: (user as any).isActive ?? true,
    })
  } else {
    showToast('error', t('common.error'), t('users.messages.load_error'))
    router.push({ name: 'users.staff.list' })
  }
}

const onSubmit = handleSubmit(async (values) => {
  submitting.value = true
  try {
    if (isEditMode.value) {
      const updateData: UpdateAdminUserRequest = {
        firstName: values.displayName,
        lastName: '',
        role: values.roleIds,
        isActive: values.isActive ?? true,
      }
      const res = await userService.update(userId.value, updateData)
      if (res.isSuccess) {
        showToast('success', t('common.success'), t('users.messages.update_success'))
        router.push({ name: 'users.staff.list' })
      }
    } else {
      const createData: CreateAdminUserRequest = {
        email: values.email,
        firstName: values.displayName,
        lastName: '',
        role: values.roleIds,
        password: values.password || '',
        isActive: true,
      }
      const res = await userService.create(createData)
      if (res.isSuccess) {
        showToast('success', t('common.success'), t('users.messages.create_success'))
        router.push({ name: 'users.staff.list' })
      }
    }
  } finally {
    submitting.value = false
  }
})
</script>

<template>
  <PageShell maxWidth="7xl">
    <PageHeader
      :title="isEditMode ? 'Edit Staff' : 'Invite Staff'"
      :description="isEditMode ? 'Update staff member details and permissions.' : 'Create a new staff account.'"
      back
    >
      <template #actions>
        <Button :label="t('common.cancel')" severity="secondary" text @click="router.back()" />
        <Button type="submit" form="staffForm" :label="isEditMode ? 'Save Changes' : 'Send Invitation'" :loading="submitting" icon="pi pi-check" />
      </template>
    </PageHeader>

    <div v-if="loading" class="flex justify-center p-12">
      <ProgressSpinner />
    </div>

    <form v-else id="staffForm" @submit.prevent="onSubmit" class="flex flex-col gap-6">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <FormField label="Email Address" name="email" :error="errors.email">
          <InputText v-model="email" class="w-full" :disabled="isEditMode" type="email" :invalid="!!errors.email" />
          <small v-if="isEditMode" class="text-surface-500">Email cannot be changed.</small>
        </FormField>

        <div v-if="!isEditMode">
          <FormField label="Initial Password" name="password">
            <Password v-model="password" class="w-full" :feedback="true" toggleMask :invalid="!!errors.password" />
          </FormField>
        </div>
        <div v-else />
      </div>

      <FormField label="Display Name" name="displayName" :error="errors.displayName">
        <InputText v-model="displayName" class="w-full" :invalid="!!errors.displayName" placeholder="Full name" />
      </FormField>

      <Divider />

      <FormField label="Assigned Roles" name="roleIds" :error="errors.roleIds">
        <div class="bg-surface-50 dark:bg-surface-900 p-4 rounded-xl border border-surface-200 dark:border-surface-700">
          <div class="flex flex-wrap gap-3">
            <div v-for="role in roleOptions" :key="role.value" class="flex align-items-center">
              <Checkbox v-model="roleIds" :inputId="role.value" :name="role.value" :value="role.value" />
              <label :for="role.value" class="ml-2 cursor-pointer select-none">{{ role.label }}</label>
            </div>
          </div>
        </div>
        <small class="block mt-2 text-surface-500">Select roles to define user permissions.</small>
      </FormField>

      <div v-if="isEditMode">
        <FormField label="Account Status" name="isActive">
          <div class="flex items-center gap-3">
            <ToggleSwitch v-model="isActive" inputId="isActive" />
            <label for="isActive" class="cursor-pointer">{{ isActive ? 'Active' : 'Inactive' }}</label>
          </div>
        </FormField>
      </div>
    </form>
  </PageShell>
</template>
