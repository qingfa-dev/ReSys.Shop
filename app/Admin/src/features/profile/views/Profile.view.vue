<script setup lang="ts">
import { onMounted, watch } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { z } from 'zod'
import { useProfileStore } from '../stores/profile.store'
import { storeToRefs } from 'pinia'
import { useToast } from '@/shared/composables/toast.use'

const profileSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  phoneNumber: z.string().default(''),
})

const store = useProfileStore()
const { profile, loading, submitting } = storeToRefs(store)
const { showToast } = useToast()

const { defineField, errors, handleSubmit: submitForm, setValues, resetForm } = useForm({
  validationSchema: toTypedSchema(profileSchema),
  initialValues: {
    firstName: '',
    lastName: '',
    phoneNumber: '',
  },
})

const [firstName] = defineField('firstName')
const [lastName] = defineField('lastName')
const [phoneNumber] = defineField('phoneNumber')

watch(profile, (val) => {
  if (val) {
    setValues({
      firstName: val.firstName || '',
      lastName: val.lastName || '',
      phoneNumber: val.phoneNumber || '',
    })
  }
})

const onFormSubmit = submitForm(async (values) => {
  const result = await store.updateProfile(values)
  if (result.success) {
    showToast('success', 'Updated', 'Profile updated successfully')
  }
})

onMounted(() => {
  store.fetchProfile()
})
</script>

<template>
  <div class="p-6 max-w-4xl mx-auto">
    <div class="mb-8">
      <h1 class="text-3xl font-black uppercase tracking-tighter text-surface-900 dark:text-surface-0">My Profile</h1>
      <p class="text-surface-500">Manage your profile information.</p>
    </div>

    <div v-if="loading" class="flex justify-center p-20">
      <ProgressSpinner />
    </div>

    <div v-else-if="profile" class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <div class="flex flex-col gap-6">
        <div class="bg-surface-0 dark:bg-surface-900 p-6 rounded-3xl border border-surface-100 dark:border-surface-800 shadow-sm">
          <h3 class="text-lg font-bold mb-6 flex items-center gap-2">
            <i class="pi pi-user text-primary"></i>
            Profile Details
          </h3>

          <div class="flex flex-col gap-4 mb-6">
            <div class="flex flex-col">
              <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Email</span>
              <span class="text-lg font-medium font-mono">{{ profile.email }}</span>
            </div>
          </div>

          <form @submit="onFormSubmit" class="flex flex-col gap-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div class="flex flex-col gap-2">
                <label for="firstName" class="font-bold text-sm">First Name</label>
                <InputText id="firstName" v-model="firstName" class="w-full rounded-xl" :invalid="!!errors.firstName" />
                <small class="text-red-500 font-medium" v-if="errors.firstName">{{ errors.firstName }}</small>
              </div>
              <div class="flex flex-col gap-2">
                <label for="lastName" class="font-bold text-sm">Last Name</label>
                <InputText id="lastName" v-model="lastName" class="w-full rounded-xl" :invalid="!!errors.lastName" />
                <small class="text-red-500 font-medium" v-if="errors.lastName">{{ errors.lastName }}</small>
              </div>
            </div>

            <div class="flex flex-col gap-2">
              <label for="phoneNumber" class="font-bold text-sm">Phone</label>
              <InputText id="phoneNumber" v-model="phoneNumber" class="w-full rounded-xl" :invalid="!!errors.phoneNumber" />
              <small class="text-red-500 font-medium" v-if="errors.phoneNumber">{{ errors.phoneNumber }}</small>
            </div>

            <Button
              type="submit"
              label="Save Profile"
              icon="pi pi-check"
              class="mt-2 rounded-xl"
              :loading="submitting"
            />
          </form>
        </div>
      </div>

      <div class="flex flex-col gap-6">
        <div class="bg-surface-0 dark:bg-surface-900 p-6 rounded-3xl border border-surface-100 dark:border-surface-800 shadow-sm">
          <h3 class="text-lg font-bold mb-6 flex items-center gap-2">
            <i class="pi pi-info-circle text-primary"></i>
            Account Info
          </h3>
          <div class="flex flex-col gap-4">
            <div class="flex flex-col">
              <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Email</span>
              <span class="text-lg font-medium font-mono">{{ profile.email }}</span>
            </div>
            <div class="flex flex-col">
              <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Full Name</span>
              <span class="text-lg font-medium">{{ profile.firstName }} {{ profile.lastName }}</span>
            </div>
            <div class="flex flex-col">
              <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Phone</span>
              <span class="text-lg font-medium">{{ profile.phoneNumber || 'N/A' }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
