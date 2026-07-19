<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useToast } from '@/shared/composables/toast.use'
import { useI18n } from 'vue-i18n'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { createRoleSchema } from '../schemas/role.schema'
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import FormField from '@/shared/components/FormField.Component.vue'
import { roleService } from '../services/role.service'
import type { CreateRoleRequest, UpdateRoleRequest } from '../types/role.request.type'

const route = useRoute()
const router = useRouter()
const { showToast } = useToast()
const { t } = useI18n()

const isEditMode = computed(() => !!route.params.id)
const roleId = computed(() => route.params.id as string)
const loading = ref(false)
const submitting = ref(false)

const { defineField, handleSubmit, errors, setValues } = useForm({
  validationSchema: toTypedSchema(createRoleSchema(t)),
  initialValues: {
    name: '',
    displayName: '',
    description: '',
    priority: 0,
  },
})

const [name] = defineField('name')
const [displayName] = defineField('displayName')
const [description] = defineField('description')
const [priority] = defineField('priority')

const isSystemRole = ref(false)

onMounted(async () => {
  if (isEditMode.value) {
    loading.value = true
    try {
      await loadRole()
    } finally {
      loading.value = false
    }
  }
})

async function loadRole() {
  const res = await roleService.getById(roleId.value)
  if (res.isSuccess && res.value) {
    const role = res.value
    setValues({
      name: role.name,
      displayName: role.displayName || '',
      description: role.description || '',
      priority: role.priority,
    })
    isSystemRole.value = role.isSystem
  } else {
    showToast('error', t('common.error'), t('roles.messages.load_error'))
    router.push({ name: 'users.roles.list' })
  }
}

const onSubmit = handleSubmit(async (values) => {
  submitting.value = true
  try {
    if (isEditMode.value) {
      const updateData: UpdateRoleRequest = {
        displayName: values.displayName,
        description: values.description,
        priority: values.priority,
      }
      const res = await roleService.update(roleId.value, updateData)
      if (res.isSuccess) {
        showToast('success', t('common.success'), t('roles.messages.update_success'))
        router.push({ name: 'users.roles.list' })
      }
    } else {
      const createData: CreateRoleRequest = {
        name: values.name,
        displayName: values.displayName,
        description: values.description,
        priority: values.priority,
      }
      const res = await roleService.create(createData)
      if (res.isSuccess) {
        showToast('success', t('common.success'), t('roles.messages.create_success'))
        router.push({ name: 'users.roles.list' })
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
      :title="isEditMode ? 'Edit Role' : 'Create Role'"
      :description="isEditMode ? 'Update role details.' : 'Define a new system role.'"
      back
    />

    <div v-if="loading" class="flex justify-center p-12">
      <ProgressSpinner />
    </div>

    <form v-else @submit.prevent="onSubmit" class="flex flex-col gap-6">
      <FormField label="Role Name (Key)" name="name" :error="errors.name" hint="Unique identifier for the role (e.g. &quot;Storefront.Manager&quot;).">
        <InputText v-model="name" class="w-full" :disabled="isEditMode || isSystemRole" :invalid="!!errors.name" />
      </FormField>

      <FormField label="Display Name" name="displayName" :error="errors.displayName">
        <InputText v-model="displayName" class="w-full" :invalid="!!errors.displayName" />
      </FormField>

      <FormField :label="t('roles.labels.description')" name="description" :error="errors.description">
        <Textarea v-model="description" class="w-full" rows="3" :invalid="!!errors.description" />
      </FormField>

      <FormField :label="t('roles.labels.priority')" name="priority" hint="Higher priority roles override lower ones in some contexts.">
        <InputNumber v-model="priority" class="w-full" :min="0" showButtons />
      </FormField>

      <Message v-if="isSystemRole" severity="warn" variant="simple" :closable="false">
        <i class="pi pi-lock mr-2"></i>
        <span class="font-bold">System Role</span> — This is a built-in system role. Some properties cannot be modified.
      </Message>

      <div class="flex justify-end gap-3 mt-4">
        <Button :label="t('common.cancel')" severity="secondary" text @click="router.back()" />
        <Button type="submit" :label="isEditMode ? 'Save Changes' : 'Create Role'" :loading="submitting" icon="pi pi-check" />
      </div>
    </form>
  </PageShell>
</template>
