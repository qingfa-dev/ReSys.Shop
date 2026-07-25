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
import { useProfile } from '../composables/useProfile'
import { ProfileForms } from '../schemas'
import { ProfileFormMapper } from '../mappers/profile.mapper'

const { route, toast, api } = useProfile()
const { t } = useI18n()

const schemas = new ProfileForms(t)
const { handleSubmit, defineField, errors, setValues } = useForm({
  validationSchema: toTypedSchema(schemas.update()),
})

const [firstName] = defineField('firstName')
const [lastName] = defineField('lastName')
const [phone] = defineField('phone')
const [avatarUrl] = defineField('avatarUrl')
const [dateOfBirth] = defineField('dateOfBirth')

const loading = ref(false)
const saving = ref(false)
const loadError = ref<string | null>(null)

const isEditing = ref(false)

const title = computed(() => isEditing.value ? 'Edit Profile' : 'My Profile')

async function loadProfile() {
  loading.value = true
  loadError.value = null
  const result = await api.get()
  if (result.isSuccess) {
    setValues({
      firstName: result.value.firstName,
      lastName: result.value.lastName,
      phone: result.value.phone ?? undefined,
      avatarUrl: result.value.avatarUrl ?? undefined,
      dateOfBirth: result.value.dateOfBirth ?? undefined,
    })
  } else {
    loadError.value = result.message ?? 'Failed to load profile'
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = ProfileFormMapper.toUpdate(values)
  const result = await api.update(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success('Profile updated successfully')
    isEditing.value = false
  } else {
    toast.error(result.message ?? 'Save failed')
  }
})

function cancel() {
  isEditing.value = false
  loadProfile()
}

onMounted(loadProfile)
</script>

<template>
  <div>
    <PageHeader :title="title" :icon="route.meta?.icon as string | undefined">
      <template #actions>
        <button v-if="!isEditing" class="p-button p-component" @click="isEditing = true">Edit Profile</button>
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading" :rows="6" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadProfile" />
    <div v-else class="card">
      <div class="grid">
        <div class="col-6">
          <FormField label="First Name" :error="errors.firstName" required>
            <input v-model="firstName" type="text" class="p-inputtext p-component w-full" :disabled="!isEditing" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Last Name" :error="errors.lastName" required>
            <input v-model="lastName" type="text" class="p-inputtext p-component w-full" :disabled="!isEditing" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-6">
          <FormField label="Phone" :error="errors.phone">
            <input v-model="phone" type="text" class="p-inputtext p-component w-full" :disabled="!isEditing" />
          </FormField>
        </div>
        <div class="col-6">
          <FormField label="Date of Birth" :error="errors.dateOfBirth">
            <input v-model="dateOfBirth" type="date" class="p-inputtext p-component w-full" :disabled="!isEditing" />
          </FormField>
        </div>
      </div>
      <div class="grid">
        <div class="col-12">
          <FormField label="Avatar URL" :error="errors.avatarUrl">
            <input v-model="avatarUrl" type="url" class="p-inputtext p-component w-full" placeholder="https://example.com/avatar.jpg" :disabled="!isEditing" />
          </FormField>
        </div>
      </div>
      <FormActions
        v-if="isEditing"
        :loading="saving"
        save-label="Save Profile"
        cancel-label="Cancel"
        @save="save"
        @cancel="cancel"
      />
    </div>
  </div>
</template>
