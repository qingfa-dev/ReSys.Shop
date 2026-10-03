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
import Textarea from 'primevue/textarea'
import Message from 'primevue/message'
import ToggleSwitch from 'primevue/toggleswitch'
import { Form, FormField, type FormSubmitEvent } from '@primevue/forms'
import { zodResolver } from '@primevue/forms/resolvers/zod'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { RoleApi } from '../services/roleApi'
import { roleSchema, type RoleForm } from '../validations/role'
import type { RoleRequest } from '../types/role'
import type { PermissionCategory, PermissionItem } from '../types/permission'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const resolver = zodResolver(roleSchema)
const form = ref<RoleForm>({ name: '', description: '' })
const formLoaded = ref(false)
const loading = ref(false)
const activeTab = ref('0')
const permissionCategories = ref<PermissionCategory[]>([])
const permissionsLoaded = ref(false)

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => (isEdit.value ? 'Edit Role' : 'New Role'))

async function initEditMode(id: string) {
  // Load: Fetch the role to seed the edit form.
  const result = await RoleApi.getRole(id)
  if (result.isSuccess) {
    form.value = { name: result.value.name, description: result.value.description ?? '' }
    formLoaded.value = true
  } else {
    handleResult(result)
    router.push('/identity/roles')
  }
}

async function loadPermissions() {
  // Check: Fetch permission categories only once per role instance.
  if (permissionsLoaded.value) return
  const result = await RoleApi.getPermissions(route.params.id as string)
  if (result.isSuccess) {
    permissionCategories.value = result.value.categories ?? []
  } else {
    handleResult(result)
  }
  permissionsLoaded.value = true
}

async function onSubmit(event: FormSubmitEvent) {
  // Validate: Return early when zod form validation fails.
  if (!event.valid) return
  loading.value = true
  const data = event.values as RoleForm
  const request: RoleRequest = { name: data.name, description: data.description || null }
  // Call: Persist the role, branching between update and create.
  const result = isEdit.value
    ? await RoleApi.updateRole(route.params.id as string, request)
    : await RoleApi.createRole(request)
  loading.value = false
  if (result.isSuccess) {
    notify.success(pageTitle.value, 'Role saved successfully')
    if (!isEdit.value && result.value) {
      router.replace(`/identity/roles/${result.value.id}`)
    }
  } else {
    handleResult(result)
  }
}

async function togglePermission(perm: PermissionItem) {
  const id = route.params.id as string
  if (perm.isAssigned) {
    // Call: Revoke the permission when it is currently assigned.
    const result = await RoleApi.revokePermissions(id, [perm.identifier])
    if (result.isSuccess) {
      perm.isAssigned = false
    } else {
      handleResult(result)
    }
  } else {
    // Call: Assign the permission when it is currently unassigned.
    const result = await RoleApi.assignPermissions(id, [perm.identifier])
    if (result.isSuccess) {
      perm.isAssigned = true
    } else {
      handleResult(result)
    }
  }
}

onMounted(() => {
  if (isEdit.value) initEditMode(route.params.id as string)
  else formLoaded.value = true
})

watch(
  () => route.params.id,
  (newId) => {
    if (newId && newId !== 'new') {
      permissionsLoaded.value = false
      initEditMode(newId as string)
    }
  },
)

watch(activeTab, (tab) => {
  if (isEdit.value && tab === '1') loadPermissions()
})
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Section: Page Header — back navigation and dynamic edit/create title -->
    <div class="flex items-center gap-4 mb-6">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/identity/roles')" />
      <h1 class="text-2xl font-semibold">{{ pageTitle }}</h1>
    </div>

    <Form id="role-form" :key="String(formLoaded)" :resolver="resolver" :initial-values="form" @submit="onSubmit">
      <!-- Section: Tabs — profile fields and edit-mode permission matrix -->
      <Tabs v-model:value="activeTab">
        <TabList>
          <Tab value="0">Profile</Tab>
          <Tab v-if="isEdit" value="1">Permissions</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <!-- Section: Content Card — holds the role name and description -->
            <Card>
              <template #content>
                <div class="flex flex-col gap-4">
                  <!-- Section: Form Fields — role name and description -->
                  <FormField v-slot="$field" name="name" class="flex flex-col gap-1">
                    <label>Name <span class="text-red-500">*</span></label>
                    <InputText fluid />
                    <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                  </FormField>
                  <FormField v-slot="$field" name="description" class="flex flex-col gap-1">
                    <label>Description</label>
                    <Textarea fluid rows="3" />
                    <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                  </FormField>
                </div>
              </template>
            </Card>
          </TabPanel>
          <TabPanel v-if="isEdit" value="1">
            <!-- Section: Form Fields — permission toggles grouped by category -->
            <Card v-for="category in permissionCategories" :key="category.category" class="mb-4">
              <template #content>
                <h3 class="text-lg font-semibold mb-1">{{ category.category }}</h3>
                <p v-if="category.description" class="text-sm text-muted-color mb-4">{{ category.description }}</p>
                <div v-for="resource in category.resources ?? []" :key="resource.resource" class="mb-4 last:mb-0">
                  <div class="font-medium mb-2">{{ resource.resource }}</div>
                  <div class="flex flex-col gap-2">
                    <div
                      v-for="perm in resource.permissions ?? []"
                      :key="perm.identifier"
                      class="flex items-center gap-2 p-2 hover:bg-surface-100 rounded"
                    >
                      <ToggleSwitch :model-value="perm.isAssigned" @change="togglePermission(perm)" />
                      <div>
                        <div class="font-medium">{{ perm.name }}</div>
                        <div v-if="perm.description" class="text-sm text-muted-color">{{ perm.description }}</div>
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Card>
            <div v-if="permissionCategories.length === 0" class="text-muted-color">No permissions available.</div>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </Form>

    <!-- Section: Action Footer — Save and Cancel actions for the role form -->
    <div class="flex gap-3 mt-4">
      <Button label="Save" icon="pi pi-check" form="role-form" type="submit" :loading="loading" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="router.push('/identity/roles')" />
    </div>
  </div>
</template>
