# Admin Identity Views — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace 5 Identity placeholder views (UsersList, UserDetail, RolesList, RoleDetail, PermissionsList) with functional CRUD UIs following the Catalog/Location pattern.

**Architecture:** List views use `usePagedQuery` + DataTable + export via `useDataTableExport`. Detail views use `@primevue/forms` `<Form>` with Zod resolvers + tabbed layout. UserDetail has Profile + Roles tabs. RoleDetail has Profile + Permissions tabs. PermissionsList is a read-only DataTable.

**Tech Stack:** Vue 3 + TypeScript, PrimeVue (DataTable, Form, Tabs, Card, ToggleSwitch), Pinia stores, existing `UserApi`/`RoleApi`/`PermissionApi` services

**Global Constraints:**
- Follows established Catalog/Location view patterns
- All services, types, validations, and stores already exist — no new data layer files
- List views: `usePagedQuery` + `useDataTableExport` + DataTable with search/delete/export
- Detail views: `@primevue/forms` `<Form>` with Zod resolver, tabs for multi-section layouts
- View files already exist as placeholders — modify in place

---

## File Structure (modified files only)

```
app/Admin/src/features/identity/views/
├── UsersList.vue          # Replace placeholder with DataTable
├── UserDetail.vue         # Replace placeholder with tabs (Profile + Roles)
├── RolesList.vue          # Replace placeholder with DataTable
├── RoleDetail.vue         # Replace placeholder with tabs (Profile + Permissions)
└── PermissionsList.vue    # Replace placeholder with read-only DataTable
```

---

### Task 1: UsersList.vue

**Files:**
- Modify: `app/Admin/src/features/identity/views/UsersList.vue`

**Interfaces:**
- Consumes: `UserApi.getUsers(query)` → `PagedResult<UserListItem>`, `deleteUser(id)` → `Result<void>`
- Consumes: `UserListItem` from `../types/user` — `{ id, userName, email, firstName, lastName, phoneNumber, emailConfirmed, isActive, lockoutEnd, createdAtUtc }`
- Consumes: `USER_FILTER_FIELDS`, `USER_SORT_FIELDS`, `USER_SEARCH_FIELDS` from `../types/user`
- Consumes: `usePagedQuery`, `useDataTableExport` from `@/shared/composables`
- Consumes: `IDENTITY` from `@/shared/constants/api` — `${IDENTITY}/users`

- [ ] **Step 1: Write UsersList.vue**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { IDENTITY } from '@/shared/constants/api'
import { UserApi } from '../services/userApi'
import type { UserListItem } from '../types/user'
import { USER_FILTER_FIELDS, USER_SORT_FIELDS, USER_SEARCH_FIELDS } from '../types/user'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { dt, exportCSV } = useDataTableExport()
const search = ref('')
const selectedItems = ref<UserListItem[]>([])

const { items, loading, setSearch, refresh } = usePagedQuery<UserListItem>(
  `${IDENTITY}/users`,
  {
    allowedFilterFields: USER_FILTER_FIELDS,
    allowedSortFields: USER_SORT_FIELDS,
    allowedSearchFields: USER_SEARCH_FIELDS,
    defaultSearchFields: ['email', 'userName', 'firstName', 'lastName'],
  },
)

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}

function navigateToNew() {
  router.push('/identity/users/new')
}

function navigateToEdit(id: string) {
  router.push(`/identity/users/${id}`)
}

function confirmDelete() {
  const names = selectedItems.value.map((u) => u.email).join(', ')
  confirm.require({
    message: `Delete ${names}? This action cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      for (const item of selectedItems.value) {
        const result = await UserApi.deleteUser(item.id)
        if (result.isSuccess) {
          notify.success('Deleted', item.email)
        } else {
          notify.error('Failed', `${item.email}: ${result.message}`)
        }
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Users</h1>
      <p class="text-muted-color">Manage registered users</p>
    </div>

    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search users..."
          @update:model-value="onSearch"
        />
      </IconField>
      <Button
        v-if="search"
        label="Clear"
        severity="secondary"
        icon="pi pi-times"
        @click="clearSearch"
      />
      <div class="flex-1" />
      <Button
        label="New User"
        icon="pi pi-plus"
        @click="navigateToNew"
      />
      <Button
        label="Reload"
        icon="pi pi-refresh"
        severity="secondary"
        @click="refresh"
      />
      <Button
        label="Export"
        icon="pi pi-download"
        severity="secondary"
        @click="exportCSV"
      />
    </div>

    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      scrollable
      paginator
      :rows="20"
      :rows-per-page-options="[10, 20, 50]"
      data-key="id"
    >
      <Column selection-mode="multiple" header-style="width:3rem" />
      <Column field="email" header="Email" :sortable="true" />
      <Column field="userName" header="Username" :sortable="true" />
      <Column field="firstName" header="First Name" :sortable="true" />
      <Column field="lastName" header="Last Name" :sortable="true" />
      <Column field="phoneNumber" header="Phone" />
      <Column field="emailConfirmed" header="Confirmed" :sortable="true">
        <template #body="{ data }">
          <Tag :value="data.emailConfirmed ? 'Yes' : 'No'" :severity="data.emailConfirmed ? 'success' : 'warn'" />
        </template>
      </Column>
      <Column header="Actions" header-style="width:8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEdit(data.id)" />
          <Button
            icon="pi pi-trash"
            severity="danger"
            text
            rounded
            @click="selectedItems = [data]; confirmDelete()"
          />
        </template>
      </Column>
      <template #empty>No users found.</template>
    </DataTable>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**

```bash
cd app/Admin && pnpm run type-check && pnpm run lint
```

Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/identity/views/UsersList.vue
git commit -m "feat(identity): implement users list view with search, delete, export"
```

---

### Task 2: UserDetail.vue

**Files:**
- Modify: `app/Admin/src/features/identity/views/UserDetail.vue`

**Interfaces:**
- Consumes: `UserApi.getUser(id)` → `Result<UserDetail>`, `createUser(request)` → `Result<UserDetail>`, `updateUser(id, request)` → `Result<UserDetail>`
- Consumes: `UserApi.getRoles(id)` → `PagedResult<UserRoleAssignment>`, `assignRoles(id, names)` → `Result<void>`, `revokeRoles(id, names)` → `Result<void>`
- Consumes: `RoleApi.getRoles(...)` → `PagedResult<RoleListItem>` for the roles dropdown list
- Consumes: `UserRequest` from `../types/user` — `{ email, userName, password, firstName, lastName, phoneNumber, emailConfirmed, isActive, lockoutEnd }`
- Consumes: `userSchema`, `UserForm` from `../validations/user`
- Consumes: `zodResolver` from `@primevue/forms/resolvers/zod`

- [ ] **Step 1: Write UserDetail.vue**

```vue
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
import DatePicker from 'primevue/datepicker'
import Message from 'primevue/message'
import Checkbox from 'primevue/checkbox'
import { Form, FormField, type FormSubmitEvent, zodResolver } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { UserApi } from '../services/userApi'
import { RoleApi } from '../services/roleApi'
import { userSchema, type UserForm } from '../validations/user'
import type { RoleListItem } from '../types/role'
import type { UserRoleAssignment } from '../types/user'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const resolver = zodResolver(userSchema)
const form = ref<UserForm>({
  email: '',
  userName: '',
  password: '',
  firstName: '',
  lastName: '',
  phoneNumber: '',
  emailConfirmed: false,
  isActive: true,
  lockoutEnd: null,
})
const formLoaded = ref(false)
const loading = ref(false)
const activeTab = ref('0')
const allRoles = ref<RoleListItem[]>([])
const assignedRoleNames = ref<string[]>([])
const rolesLoaded = ref(false)

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit User' : 'New User')

async function initEditMode(id: string) {
  const result = await UserApi.getUser(id)
  if (result.isSuccess) {
    const u = result.value
    form.value = {
      email: u.email ?? '',
      userName: u.userName ?? '',
      password: '',
      firstName: u.firstName ?? '',
      lastName: u.lastName ?? '',
      phoneNumber: u.phoneNumber ?? '',
      emailConfirmed: u.emailConfirmed ?? false,
      isActive: u.isActive ?? true,
      lockoutEnd: u.lockoutEnd ? new Date(u.lockoutEnd) : null,
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
    password: data.password || undefined,
    firstName: data.firstName || null,
    lastName: data.lastName || null,
    phoneNumber: data.phoneNumber || null,
    emailConfirmed: data.emailConfirmed,
    isActive: data.isActive,
    lockoutEnd: data.lockoutEnd?.toISOString() ?? null,
  }
  const result = isEdit.value
    ? await UserApi.updateUser(route.params.id as string, request as any)
    : await UserApi.createUser(request as any)
  loading.value = false
  if (result.isSuccess) {
    notify.success(pageTitle.value, 'User saved successfully')
    if (!isEdit.value && result.value) {
      router.replace(`/identity/users/${(result.value as any).id}`)
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
                      <label>First Name</label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <FormField v-slot="$field" name="lastName" class="flex flex-col gap-1">
                      <label>Last Name</label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                  </div>
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField v-slot="$field" name="password" class="flex flex-col gap-1">
                      <label>Password <span v-if="!isEdit" class="text-red-500">*</span></label>
                      <InputText fluid type="password" />
                      <small v-if="isEdit" class="text-muted-color">Leave blank to keep current password</small>
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                    <FormField v-slot="$field" name="phoneNumber" class="flex flex-col gap-1">
                      <label>Phone</label>
                      <InputText fluid />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
                    </FormField>
                  </div>
                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <FormField v-slot="$field" name="emailConfirmed" class="flex flex-col gap-1">
                      <label>Email Confirmed</label>
                      <ToggleSwitch />
                    </FormField>
                    <FormField v-slot="$field" name="isActive" class="flex flex-col gap-1">
                      <label>Active</label>
                      <ToggleSwitch />
                    </FormField>
                    <FormField v-slot="$field" name="lockoutEnd" class="flex flex-col gap-1">
                      <label>Lockout End</label>
                      <DatePicker fluid show-time />
                      <Message v-if="$field?.invalid" severity="error" size="small" variant="simple">{{ $field.error?.message }}</Message>
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
```

- [ ] **Step 2: Verify type-check and lint**

```bash
cd app/Admin && pnpm run type-check && pnpm run lint
```

Expected: 0 errors.

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/identity/views/UserDetail.vue
git commit -m "feat(identity): implement user detail view with profile and roles tabs"
```

---

### Task 3: RolesList.vue

**Files:**
- Modify: `app/Admin/src/features/identity/views/RolesList.vue`

**Interfaces:**
- Consumes: `RoleApi.getRoles(query)` → `PagedResult<RoleListItem>`, `deleteRole(id)` → `Result<void>`
- Consumes: `ROLE_FILTER_FIELDS`, `ROLE_SORT_FIELDS`, `ROLE_SEARCH_FIELDS`, `RoleListItem` from `../types/role`
- Consumes: `IDENTITY` from `@/shared/constants/api` — `${IDENTITY}/roles`

- [ ] **Step 1: Write RolesList.vue**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useConfirm } from 'primevue/useconfirm'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { useNotify } from '@/shared/composables/useNotify'
import { IDENTITY } from '@/shared/constants/api'
import { RoleApi } from '../services/roleApi'
import type { RoleListItem } from '../types/role'
import { ROLE_FILTER_FIELDS, ROLE_SORT_FIELDS, ROLE_SEARCH_FIELDS } from '../types/role'

const router = useRouter()
const confirm = useConfirm()
const notify = useNotify()
const { dt, exportCSV } = useDataTableExport()
const search = ref('')
const selectedItems = ref<RoleListItem[]>([])

const { items, loading, setSearch, refresh } = usePagedQuery<RoleListItem>(
  `${IDENTITY}/roles`,
  {
    allowedFilterFields: ROLE_FILTER_FIELDS,
    allowedSortFields: ROLE_SORT_FIELDS,
    allowedSearchFields: ROLE_SEARCH_FIELDS,
    defaultSearchFields: ['name'],
  },
)

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}

function navigateToNew() {
  router.push('/identity/roles/new')
}

function navigateToEdit(id: string) {
  router.push(`/identity/roles/${id}`)
}

function confirmDelete() {
  const names = selectedItems.value.map((r) => r.name).join(', ')
  confirm.require({
    message: `Delete role${selectedItems.value.length > 1 ? 's' : ''} "${names}"? This action cannot be undone.`,
    header: 'Confirm Delete',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: 'Cancel',
    acceptLabel: 'Delete',
    acceptClass: 'p-button-danger',
    accept: async () => {
      for (const item of selectedItems.value) {
        const result = await RoleApi.deleteRole(item.id)
        if (result.isSuccess) {
          notify.success('Deleted', item.name)
        } else {
          notify.error('Failed', `${item.name}: ${result.message}`)
        }
      }
      selectedItems.value = []
      refresh()
    },
  })
}
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Roles</h1>
      <p class="text-muted-color">Manage role definitions</p>
    </div>

    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText :model-value="search" placeholder="Search roles..." @update:model-value="onSearch" />
      </IconField>
      <Button v-if="search" label="Clear" severity="secondary" icon="pi pi-times" @click="clearSearch" />
      <div class="flex-1" />
      <Button label="New Role" icon="pi pi-plus" @click="navigateToNew" />
      <Button label="Reload" icon="pi pi-refresh" severity="secondary" @click="refresh" />
      <Button label="Export" icon="pi pi-download" severity="secondary" @click="exportCSV" />
    </div>

    <DataTable
      ref="dt"
      v-model:selection="selectedItems"
      :value="items"
      :loading="loading"
      scrollable
      paginator
      :rows="20"
      :rows-per-page-options="[10, 20, 50]"
      data-key="id"
    >
      <Column selection-mode="multiple" header-style="width:3rem" />
      <Column field="name" header="Name" :sortable="true" />
      <Column header="Actions" header-style="width:8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" severity="secondary" text rounded @click="navigateToEdit(data.id)" />
          <Button icon="pi pi-trash" severity="danger" text rounded @click="selectedItems = [data]; confirmDelete()" />
        </template>
      </Column>
      <template #empty>No roles found.</template>
    </DataTable>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/identity/views/RolesList.vue
git commit -m "feat(identity): implement roles list view with search, delete, export"
```

---

### Task 4: RoleDetail.vue

**Files:**
- Modify: `app/Admin/src/features/identity/views/RoleDetail.vue`

**Interfaces:**
- Consumes: `RoleApi.getRole(id)` → `Result<RoleDetail>`, `createRole(request)` → `Result<RoleDetail>`, `updateRole(id, request)` → `Result<RoleDetail>`
- Consumes: `RoleApi.getPermissions(id)` → `Result<PermissionGroupResponse>`, `assignPermissions(id, names)` → `Result<void>`, `revokePermissions(id, names)` → `Result<void>`
- Consumes: `PermissionApi.getPermissions()` → `PagedResult<PermissionMetadata>` for all system permissions
- Consumes: `roleSchema`, `RoleForm` from `../validations/role`

- [ ] **Step 1: Write RoleDetail.vue**

```vue
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
import { Form, FormField, type FormSubmitEvent, zodResolver } from '@primevue/forms'
import { useNotify } from '@/shared/composables/useNotify'
import { useApiErrorHandler } from '@/shared/composables/useApiErrorHandler'
import { RoleApi } from '../services/roleApi'
import { PermissionApi } from '../services/permissionApi'
import { roleSchema, type RoleForm } from '../validations/role'
import type { PermissionMetadata, PermissionGroupResponse, PermissionGroup } from '../types/permission'

const route = useRoute()
const router = useRouter()
const notify = useNotify()
const { handleResult } = useApiErrorHandler()

const resolver = zodResolver(roleSchema)
const form = ref<RoleForm>({ name: '', description: '' })
const formLoaded = ref(false)
const loading = ref(false)
const activeTab = ref('0')
const permissionGroups = ref<PermissionGroup[]>([])
const assignedPermissionNames = ref<string[]>([])
const permissionsLoaded = ref(false)

const isEdit = computed(() => !!route.params.id && route.params.id !== 'new')
const pageTitle = computed(() => isEdit.value ? 'Edit Role' : 'New Role')

async function initEditMode(id: string) {
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
  if (permissionsLoaded.value) return
  const [allResult, assignedResult] = await Promise.all([
    PermissionApi.getPermissions(),
    RoleApi.getPermissions(route.params.id as string),
  ])
  if (allResult.isSuccess) {
    const groups = (allResult.value as unknown as PermissionGroupResponse).groups ?? []
    permissionGroups.value = groups
  }
  if (assignedResult.isSuccess && 'groups' in (assignedResult.value as PermissionGroupResponse)) {
    const flat: string[] = []
    for (const g of (assignedResult.value as PermissionGroupResponse).groups ?? []) {
      for (const p of g.permissions ?? []) {
        if (p.isAssigned) flat.push(p.name)
      }
    }
    assignedPermissionNames.value = flat
  }
  permissionsLoaded.value = true
}

async function onSubmit(event: FormSubmitEvent) {
  if (!event.valid) return
  loading.value = true
  const data = event.values as RoleForm
  const request = { name: data.name, description: data.description || null }
  const result = isEdit.value
    ? await RoleApi.updateRole(route.params.id as string, request as any)
    : await RoleApi.createRole(request as any)
  loading.value = false
  if (result.isSuccess) {
    notify.success(pageTitle.value, 'Role saved successfully')
    if (!isEdit.value && result.value) {
      router.replace(`/identity/roles/${(result.value as any).id}`)
    }
  } else {
    handleResult(result)
  }
}

function isPermissionAssigned(name: string): boolean {
  return assignedPermissionNames.value.includes(name)
}

async function togglePermission(name: string) {
  if (isPermissionAssigned(name)) {
    const result = await RoleApi.revokePermissions(route.params.id as string, [name])
    if (result.isSuccess) {
      assignedPermissionNames.value = assignedPermissionNames.value.filter((n) => n !== name)
    } else {
      handleResult(result)
    }
  } else {
    const result = await RoleApi.assignPermissions(route.params.id as string, [name])
    if (result.isSuccess) {
      assignedPermissionNames.value = [...assignedPermissionNames.value, name]
    } else {
      handleResult(result)
    }
  }
}

onMounted(() => {
  if (isEdit.value) initEditMode(route.params.id as string)
  else formLoaded.value = true
})

watch(() => route.params.id, (newId) => { if (newId && newId !== 'new') initEditMode(newId as string) })
watch(activeTab, (tab) => { if (isEdit.value && tab === '1') loadPermissions() })
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex items-center gap-4 mb-6">
      <Button icon="pi pi-arrow-left" severity="secondary" text rounded @click="router.push('/identity/roles')" />
      <h1 class="text-2xl font-semibold">{{ pageTitle }}</h1>
    </div>

    <Form id="role-form" :key="String(formLoaded)" :resolver="resolver" :initial-values="form" @submit="onSubmit">
      <Tabs v-model:value="activeTab">
        <TabList>
          <Tab value="0">Profile</Tab>
          <Tab v-if="isEdit" value="1">Permissions</Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="0">
            <Card>
              <template #content>
                <div class="flex flex-col gap-4">
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
            <Card v-for="group in permissionGroups" :key="group.name" class="mb-4">
              <template #content>
                <h3 class="text-lg font-semibold mb-3">{{ group.name }}</h3>
                <div class="flex flex-col gap-2">
                  <div
                    v-for="perm in (group.permissions ?? [])"
                    :key="perm.name"
                    class="flex items-center gap-2 p-2 hover:bg-surface-100 rounded"
                  >
                    <ToggleSwitch
                      :model-value="isPermissionAssigned(perm.name)"
                      @change="togglePermission(perm.name)"
                    />
                    <div>
                      <div class="font-medium">{{ perm.name }}</div>
                      <div v-if="perm.description" class="text-sm text-muted-color">{{ perm.description }}</div>
                    </div>
                  </div>
                </div>
              </template>
            </Card>
            <div v-if="permissionGroups.length === 0" class="text-muted-color">No permissions available.</div>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </Form>

    <div class="flex gap-3 mt-4">
      <Button label="Save" icon="pi pi-check" form="role-form" type="submit" :loading="loading" />
      <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="router.push('/identity/roles')" />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/identity/views/RoleDetail.vue
git commit -m "feat(identity): implement role detail view with profile and permissions tabs"
```

---

### Task 5: PermissionsList.vue

**Files:**
- Modify: `app/Admin/src/features/identity/views/PermissionsList.vue`

**Interfaces:**
- Consumes: `PermissionApi.getPermissions()` → `PagedResult<PermissionMetadata>`
- Consumes: `PermissionMetadata` from `../types/permission` — `{ name, category, description }`

- [ ] **Step 1: Write PermissionsList.vue**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import { useDataTableExport } from '@/shared/composables/useDataTableExport'
import { usePagedQuery } from '@/shared/composables/usePagedQuery'
import { IDENTITY } from '@/shared/constants/api'
import type { PermissionMetadata } from '../types/permission'

const { dt, exportCSV } = useDataTableExport()
const search = ref('')

const { items, loading, setSearch, refresh } = usePagedQuery<PermissionMetadata>(
  `${IDENTITY}/permissions`,
  {
    defaultPageSize: 100,
    allowedSearchFields: ['name', 'category', 'description'],
    defaultSearchFields: ['name', 'category'],
  },
)

function onSearch(value: string) {
  search.value = value
  setSearch(value)
}

function clearSearch() {
  search.value = ''
  setSearch('')
}
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold mb-1">Permissions</h1>
      <p class="text-muted-color">System permissions reference</p>
    </div>

    <div class="flex items-center gap-3 mb-4">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText
          :model-value="search"
          placeholder="Search permissions..."
          @update:model-value="onSearch"
        />
      </IconField>
      <Button v-if="search" label="Clear" severity="secondary" icon="pi pi-times" @click="clearSearch" />
      <div class="flex-1" />
      <Button label="Reload" icon="pi pi-refresh" severity="secondary" @click="refresh" />
      <Button label="Export" icon="pi pi-download" severity="secondary" @click="exportCSV" />
    </div>

    <DataTable
      ref="dt"
      :value="items"
      :loading="loading"
      scrollable
      paginator
      :rows="50"
      data-key="name"
    >
      <Column field="name" header="Name" :sortable="true" />
      <Column field="category" header="Category" :sortable="true" />
      <Column field="description" header="Description" />
      <template #empty>No permissions found.</template>
    </DataTable>
  </div>
</template>
```

- [ ] **Step 2: Verify type-check and lint**
- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/identity/views/PermissionsList.vue
git commit -m "feat(identity): implement permissions list view (read-only)"
```
