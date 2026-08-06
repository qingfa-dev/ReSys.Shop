<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { z } from 'zod'
import { changePassword } from '@/features/identity/services/authApi'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'

const router = useRouter()
const notify = useNotify()
const { handleError } = useApiErrorHandler()

const form = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})
const loading = ref(false)
const errors = ref<Record<string, string>>({})

const schema = z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: z.string().min(8, 'Password must be at least 8 characters'),
  confirmPassword: z.string().min(1, 'Please confirm your password'),
}).refine(data => data.newPassword === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
})

// Compute: Password strength heuristic (0-4)
const strength = computed(() => {
  const p = form.value.newPassword
  let s = 0
  if (p.length >= 8) s++
  if (p.length >= 12) s++
  if (/[A-Z]/.test(p) && /[a-z]/.test(p)) s++
  if (/\d/.test(p)) s++
  return s
})

const strengthLabel = computed(() => ['Too weak', 'Weak', 'Fair', 'Good', 'Strong'][strength.value])
const strengthColor = computed(() => ['bg-red-400', 'bg-red-500', 'bg-yellow-400', 'bg-green-400', 'bg-green-600'][strength.value])

async function submit(): Promise<void> {
  errors.value = {}
  const result = schema.safeParse(form.value)
  if (!result.success) {
    for (const issue of result.error.issues) {
      errors.value[issue.path[0] as string] = issue.message
    }
    return
  }
  loading.value = true
  try {
    const res = await changePassword(form.value.currentPassword, form.value.newPassword)
    if (res.isSuccess) {
      notify.success('Password changed', 'Your password has been updated')
      router.push('/account/profile')
    } else {
      handleError(new Error(res.message ?? 'Current password may be incorrect'))
    }
  } finally {
    loading.value = false
  }
}
</script>
<template>
  <!-- Section: Change Password Page -->
  <div class="max-w-md">
    <h1 class="text-2xl font-bold text-stone-900 mb-6">Change Password</h1>
    <form class="space-y-4" @submit.prevent="submit">
      <!-- Section: Current Password -->
      <div>
        <label class="block text-sm font-medium text-stone-700 mb-1">Current Password</label>
        <InputText v-model="form.currentPassword" type="password" class="w-full" :invalid="!!errors.currentPassword" />
        <small v-if="errors.currentPassword" class="text-red-500">{{ errors.currentPassword }}</small>
      </div>
      <!-- Section: New Password -->
      <div>
        <label class="block text-sm font-medium text-stone-700 mb-1">New Password</label>
        <InputText v-model="form.newPassword" type="password" class="w-full" :invalid="!!errors.newPassword" />
        <div v-if="form.newPassword" class="mt-1 flex items-center gap-2">
          <div class="flex-1 h-1.5 bg-stone-200 rounded-full overflow-hidden">
            <div class="h-full rounded-full transition-all" :class="strengthColor" :style="{ width: `${(strength + 1) * 25}%` }" />
          </div>
          <span class="text-xs text-stone-500">{{ strengthLabel }}</span>
        </div>
        <small v-if="errors.newPassword" class="text-red-500">{{ errors.newPassword }}</small>
      </div>
      <!-- Section: Confirm Password -->
      <div>
        <label class="block text-sm font-medium text-stone-700 mb-1">Confirm New Password</label>
        <InputText v-model="form.confirmPassword" type="password" class="w-full" :invalid="!!errors.confirmPassword" />
        <small v-if="errors.confirmPassword" class="text-red-500">{{ errors.confirmPassword }}</small>
      </div>
      <!-- Section: Submit -->
      <Button type="submit" label="Change Password" class="w-full" :loading="loading" />
    </form>
  </div>
</template>
