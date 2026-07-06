<template>
  <AppDialog :visible="visible" :title="user ? 'Edit user' : 'New user'" @update:visible="$emit('update:visible', $event)">
    <form class="flex flex-col gap-3" @submit.prevent="onSubmit">
      <AppFormField label="Email" :error="form.errors.value.email">
        <InputText v-model="email" :invalid="!!form.errors.value.email" :disabled="!!user" />
      </AppFormField>
      <AppFormField label="Display name" :error="form.errors.value.displayName">
        <InputText v-model="displayName" :invalid="!!form.errors.value.displayName" />
      </AppFormField>
      <AppFormField v-if="!user" label="Password" :error="form.errors.value.password">
        <Password v-model="password" :feedback="false" toggle-mask :invalid="!!form.errors.value.password" input-class="w-full" />
      </AppFormField>
      <AppFormField label="Roles" :error="form.errors.value.roleIds">
        <MultiSelect v-model="roleIds" :options="[]" option-label="name" option-value="id" placeholder="Select roles" display="chip" class="w-full" />
      </AppFormField>
    </form>
    <template #footer>
      <AppButton label="Cancel" variant="secondary" @click="$emit('update:visible', false)" />
      <AppButton :label="user ? 'Save' : 'Create'" :loading="form.isPending.value" @click="onSubmit" />
    </template>
  </AppDialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { User, UserCreateRequest, UserUpdateRequest } from '../model/user.types'
import { useUserForm } from '../composables/useUserForm'

const props = defineProps<{ visible: boolean; user?: User | null }>()
const emit = defineEmits<{
  'update:visible': [v: boolean]
  saved: [user: User]
}>()

const form = useUserForm()
const email = ref('')
const displayName = ref('')
const password = ref('')
const roleIds = ref<string[]>([])

watch(
  () => props.user,
  (u) => {
    email.value = u?.email ?? ''
    displayName.value = u?.displayName ?? ''
    password.value = ''
    roleIds.value = u?.roles ?? []
  },
  { immediate: true },
)

async function onSubmit() {
  try {
    if (props.user) {
      const body: UserUpdateRequest = {
        id: props.user.id,
        displayName: displayName.value,
        roleIds: roleIds.value,
      }
      const updated = await form.submitUpdate(body)
      emit('saved', updated)
    } else {
      const body: UserCreateRequest = {
        email: email.value,
        displayName: displayName.value,
        password: password.value,
        roleIds: roleIds.value,
      }
      const created = await form.submitCreate(body)
      emit('saved', created)
    }
  } catch {
    // validation errors surfaced via form.errors
  }
}
</script>
