<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import TabPanels from 'primevue/tabpanels'
import TabPanel from 'primevue/tabpanel'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import ToggleSwitch from 'primevue/toggleswitch'
import Message from 'primevue/message'
import Checkbox from 'primevue/checkbox'
import { Form, FormField } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import type { FormSubmitEvent } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { UserApi, type UserRoleAssignment } from '../services/userApi'
import { RoleApi } from '../services/roleApi'
import { userSchema, type UserForm } from '../validations/user'
import type { RoleListItem } from '../types/role'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const resolver = zodResolver(userSchema)
const form = ref<UserForm>({
  email: '',
  userName: '',
  firstName: '',
  lastName: '',
  phoneNumber: '',
  emailConfirmed: false,
  phoneNumberConfirmed: false,
})
const formLoaded = ref(false)
const loading = ref(false)
const activeTab = ref('0')
const allRoles = ref<RoleListItem[]>([])
const assignedRoleNames = ref<string[]>([])
const rolesLoaded = ref(false)

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => (isEdit.value ? 'Edit User' : 'New User'))

async function initEditMode(id: string) {
  const result = await UserApi.getUser(id)
  if (result.isSuccess) {
    const u = result.value
    form.value = {
      email: u.email ?? '',
      userName: u.userName ?? '',
      firstName: u.firstName ?? '',
      lastName: u.lastName ?? '',
      phoneNumber: u.phoneNumber ?? '',
      emailConfirmed: u.emailConfirmed ?? false,
      phoneNumberConfirmed: u.phoneNumberConfirmed ?? false,
    }
    formLoaded.value = true
  } else {
    handleResult(result)
    router.push('/identity/users')
  }
}

async function loadRoles() {
  if (rolesLoaded.value) return
  const [rolesResult, assignedResult] = await Promise.all([
    RoleApi.getRoles({ pageSize: 100 }),
    UserApi.getRoles(route.params.id as string),
  ])
  if (rolesResult.isSuccess) {
    allRoles.value = rolesResult.items
  }
  if (assignedResult.isSuccess) {
    assignedRoleNames.value = assignedResult.items.map((r: UserRoleAssignment) => r.name)
  }
  rolesLoaded.value = true
}

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return
  loading.value = true
  const data = event.values as UserForm
  const request = {
    email: data.email,
    userName: data.userName,
    firstName: data.firstName,
    lastName: data.lastName,
    phoneNumber: data.phoneNumber || null,
    emailConfirmed: data.emailConfirmed,
    phoneNumberConfirmed: data.phoneNumberConfirmed,
  }
  const result = isEdit.value
    ? await UserApi.updateUser(route.params.id as string, request)
    : await UserApi.createUser(request)
  loading.value = false
  if (result.isSuccess) {
    notify.success(pageTitle.value, 'User saved successfully')
    if (!isEdit.value && result.value) {
      router.replace(`/identity/users/${result.value.id}`)
    }
  } else {
    handleResult(result)
  }
}

function isRoleAssigned(roleName: string): boolean {
  return assignedRoleNames.value.includes(roleName)
}

async function toggleRole(roleName: string) {
  if (isRoleAssigned(roleName)) {
    const result = await UserApi.revokeRoles(route.params.id as string, [roleName])
    if (result.isSuccess) {
      assignedRoleNames.value = assignedRoleNames.value.filter((n) => n !== roleName)
      notify.success('Role removed', roleName)
    } else {
      handleResult(result)
    }
  } else {
    const result = await UserApi.assignRoles(route.params.id as string, [roleName])
    if (result.isSuccess) {
      assignedRoleNames.value = [...assignedRoleNames.value, roleName]
      notify.success('Role assigned', roleName)
    } else {
      handleResult(result)
    }
  }
}

onMounted(() => {
  if (isEdit.value) {
    initEditMode(route.params.id as string)
  } else {
    formLoaded.value = true
  }
})

watch(
  () => route.params.id,
  (newId) => {
    if (newId && newId !== 'new') {
      rolesLoaded.value = false
      assignedRoleNames.value = []
      initEditMode(newId as string)
    }
  },
)

watch(activeTab, (tab) => {
  if (isEdit.value && tab === '1') {
    loadRoles()
  }
})
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex items-center gap-4 mb-6">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/identity/users')" />
      <h1 class="text-2xl font-semibold">{{ pageTitle }}</h1>
    </div>

    <Form
      id="user-form"
      :key="String(formLoaded)"
      :resolver="resolver"
      :initial-values="form"
      @submit="onSubmit"
    >
      <Tabs v-model:value="activeTab">
        <TabList>
          <Tab value="0">Profile</Tab>
          <Tab v-if="isEdit" value="1">Roles</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <Card>
              <template #content>
                <div class="flex flex-col gap-4">
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField v-slot="$field" name="email" class="flex flex-col gap-1">
                      <label>Email <span class="text-red-500">*</span></label>
                      <InputText fluid :disabled="isEdit" />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <FormField v-slot="$field" name="userName" class="flex flex-col gap-1">
                      <label>Username <span class="text-red-500">*</span></label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                  </div>
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField v-slot="$field" name="firstName" class="flex flex-col gap-1">
                      <label>First Name <span class="text-red-500">*</span></label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <FormField v-slot="$field" name="lastName" class="flex flex-col gap-1">
                      <label>Last Name <span class="text-red-500">*</span></label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                  </div>
                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <FormField v-slot="$field" name="phoneNumber" class="flex flex-col gap-1">
                      <label>Phone</label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <FormField name="emailConfirmed" class="flex flex-col gap-1">
                      <label>Email Confirmed</label>
                      <ToggleSwitch />
                    </FormField>
                    <FormField name="phoneNumberConfirmed" class="flex flex-col gap-1">
                      <label>Phone Confirmed</label>
                      <ToggleSwitch />
                    </FormField>
                  </div>
                </div>
              </template>
            </Card>
          </TabPanel>
          <TabPanel v-if="isEdit" value="1">
            <Card>
              <template #content>
                <div class="flex flex-col gap-2">
                  <div
                    v-for="role in allRoles"
                    :key="role.id"
                    class="flex items-center gap-2 p-2 hover:bg-surface-100 rounded"
                  >
                    <Checkbox
                      :model-value="isRoleAssigned(role.name)"
                      :binary="true"
                      @change="toggleRole(role.name)"
                    />
                    <div>
                      <div class="font-medium">{{ role.name }}</div>
                      <div v-if="role.description" class="text-sm text-muted-color">{{ role.description }}</div>
                    </div>
                  </div>
                  <div v-if="allRoles.length === 0" class="text-muted-color">No roles available.</div>
                </div>
              </template>
            </Card>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </Form>

    <div class="flex gap-3 mt-4">
      <Button label="Save" icon="pi pi-check" form="user-form" type="submit" :loading="loading" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="router.push('/identity/users')" />
    </div>
  </div>
</template>
