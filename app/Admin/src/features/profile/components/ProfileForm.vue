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

const title = computed(() => isEditing.value ? t('profile.titles.edit_profile') : t('profile.titles.my_profile'))

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
    loadError.value = result.message ?? t('profile.messages.load_error')
  }
  loading.value = false
}

const save = handleSubmit(async (values) => {
  saving.value = true
  const data = ProfileFormMapper.toUpdate(values)
  const result = await api.update(data)
  saving.value = false
  if (result.isSuccess) {
    toast.success(t('profile.messages.update_success'))
    isEditing.value = false
  } else {
    toast.error(result.message ?? t('profile.messages.save_failed'))
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
    <PageHeader
      :title="title"
      :subtitle="t('profile.descriptions.manage')"
      :icon="route.meta?.icon as string | undefined"
    >
      <template #actions>
        <Button
          v-if="!isEditing"
          :label="t('profile.actions.edit_profile')"
          icon="pi pi-pencil"
          size="small"
          @click="isEditing = true"
        />
      </template>
    </PageHeader>
    <LoadingSkeleton v-if="loading" :rows="6" :columns="2" />
    <ErrorState v-else-if="loadError" :title="loadError" @retry="loadProfile" />
    <AppCard v-else>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.labels.first_name')" :error="errors.firstName" required>
            <input v-model="firstName" type="text" class="p-inputtext p-component w-full" :disabled="!isEditing" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.labels.last_name')" :error="errors.lastName" required>
            <input v-model="lastName" type="text" class="p-inputtext p-component w-full" :disabled="!isEditing" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.labels.phone')" :error="errors.phone">
            <input v-model="phone" type="text" class="p-inputtext p-component w-full" :disabled="!isEditing" />
          </FormField>
        </div>
        <div class="col-span-full sm:col-span-6">
          <FormField :label="t('profile.labels.date_of_birth')" :error="errors.dateOfBirth">
            <input v-model="dateOfBirth" type="date" class="p-inputtext p-component w-full" :disabled="!isEditing" />
          </FormField>
        </div>
      </div>
      <div class="grid grid-cols-12 gap-4">
        <div class="col-span-full">
          <FormField :label="t('profile.labels.avatar_url')" :error="errors.avatarUrl">
            <input v-model="avatarUrl" type="url" class="p-inputtext p-component w-full" placeholder="https://example.com/avatar.jpg" :disabled="!isEditing" />
          </FormField>
        </div>
      </div>
      <FormActions
        v-if="isEditing"
        :loading="saving"
        :save-label="t('profile.actions.save_profile')"
        :cancel-label="t('profile.actions.cancel')"
        @save="save"
        @cancel="cancel"
      />
    </AppCard>
  </div>
</template>
