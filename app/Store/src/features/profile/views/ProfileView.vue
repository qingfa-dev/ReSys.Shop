<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'
import { usePreferences } from '@/shared/composables/usePreferences'
import { useProfileStore } from '../stores/profileStore'
import { UpdateProfileRequestSchema } from '../validations'

usePageTitle('Profile')

// Stores: Profile owns the entity; preferences back the summary card.
const profileStore = useProfileStore()
const notify = useNotify()
const { preferences } = usePreferences()

// Draft: Editable name fields committed by the save button.
const firstName = ref('')
const lastName = ref('')

// Editing: Per-row Inplace open state so only one row edits at a time.
const firstEditing = ref(false)
const lastEditing = ref(false)

// Sync: Seed the edit drafts whenever the profile entity is replaced.
watch(
  () => profileStore.profile,
  (profile) => {
    if (profile) {
      firstName.value = profile.firstName
      lastName.value = profile.lastName
    }
  },
  { immediate: true },
)

// Save: Validate the draft against the shared schema, persist, toast the result.
async function onSave(): Promise<void> {
  const parsed = UpdateProfileRequestSchema.safeParse({
    firstName: firstName.value,
    lastName: lastName.value,
    email: profileStore.profile?.email ?? '',
  })
  if (!parsed.success) {
    notify.error('Enter both first and last name')
    return
  }
  const ok = await profileStore.updateProfile(parsed.data)
  if (ok) notify.success('Profile updated')
  else notify.error(profileStore.error ?? 'Could not update profile')
}

onMounted(() => {
  // Load: Guarded store init fetches the profile exactly once per session.
  void profileStore.init()
})
</script>

<template>
  <!-- Section: Content Cards — personal information plus a preferences summary -->
  <div class="flex flex-col gap-6">
    <!-- Section: Personal Information Card — inplace rows for the editable names -->
    <Card>
      <template #title>Personal Information</template>
      <template #content>
        <div class="flex flex-col gap-5">
          <!-- Inplace Rows: Click a display value to swap in its float-label input -->
          <div class="flex flex-col gap-4">
            <div class="flex items-center justify-between gap-4 border-b border-surface-200 pb-3">
              <span class="text-sm text-muted">First name</span>
              <Inplace v-model:active="firstEditing">
                <template #display>{{ firstName || 'Not set' }}</template>
                <template #content="{ closeCallback }">
                  <div class="flex items-center gap-2">
                    <FloatLabel variant="on">
                      <InputText id="profile-first-name" v-model="firstName" class="w-56" />
                      <Label for="profile-first-name">First name</Label>
                    </FloatLabel>
                    <Button size="small" label="Done" variant="text" @click="closeCallback" />
                  </div>
                </template>
              </Inplace>
            </div>
            <div class="flex items-center justify-between gap-4 border-b border-surface-200 pb-3">
              <span class="text-sm text-muted">Last name</span>
              <Inplace v-model:active="lastEditing">
                <template #display>{{ lastName || 'Not set' }}</template>
                <template #content="{ closeCallback }">
                  <div class="flex items-center gap-2">
                    <FloatLabel variant="on">
                      <InputText id="profile-last-name" v-model="lastName" class="w-56" />
                      <Label for="profile-last-name">Last name</Label>
                    </FloatLabel>
                    <Button size="small" label="Done" variant="text" @click="closeCallback" />
                  </div>
                </template>
              </Inplace>
            </div>
            <div class="flex items-center justify-between gap-4">
              <span class="text-sm text-muted">Email</span>
              <span class="text-sm font-medium">{{ profileStore.profile?.email ?? '—' }}</span>
            </div>
          </div>

          <!-- Action Footer: Save persists both name drafts -->
          <div>
            <Button label="Save" icon="pi pi-check" :loading="profileStore.saving" @click="onSave" />
          </div>
        </div>
      </template>
    </Card>

    <!-- Section: Preferences Summary Card — display prefs and the edit shortcut -->
    <Card>
      <template #title>Preferences</template>
      <template #content>
        <div class="flex flex-col gap-3 text-sm">
          <div class="flex items-center justify-between">
            <span class="text-muted">Currency</span>
            <span>{{ preferences.currency }}</span>
          </div>
          <div class="flex items-center justify-between">
            <span class="text-muted">Language</span>
            <span>{{ preferences.language }}</span>
          </div>
          <Divider />
          <Button
            as="router-link"
            to="/account/preferences"
            label="Edit preferences"
            icon="pi pi-sliders-h"
            variant="text"
            class="self-start"
          />
        </div>
      </template>
    </Card>
  </div>
</template>
