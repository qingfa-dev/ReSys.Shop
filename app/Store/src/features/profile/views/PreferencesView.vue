<script setup lang="ts">
import { usePreferences } from '@/shared/composables/usePreferences'
import { useTheme } from '@/shared/composables/useTheme'
import { useNotify } from '@/shared/composables/useNotify'

const { preferences } = usePreferences()
const { mode: themeMode, setMode } = useTheme()
const notify = useNotify()

const currencies = [
  { label: 'USD ($)', value: 'USD' },
  { label: 'EUR (€)', value: 'EUR' },
  { label: 'VND (₫)', value: 'VND' },
]

const languages = [
  { label: 'English', value: 'en' },
  { label: 'Vietnamese', value: 'vi' },
]

const themeOptions = [
  { label: 'Light', value: 'light' },
  { label: 'Dark', value: 'dark' },
  { label: 'System', value: 'system' },
]

function save(): void {
  setMode(themeMode.value)
  notify.success('Saved', 'Preferences updated')
}
</script>
<template>
  <!-- Section: Preferences Page -->
  <div class="max-w-md">
    <h1 class="text-2xl font-bold text-stone-900 mb-6">Preferences</h1>
    <div class="space-y-6">
      <!-- Section: Appearance -->
      <section class="space-y-3">
        <h2 class="text-sm font-semibold text-stone-900">Appearance</h2>
        <div>
          <label class="block text-sm text-stone-600 mb-1">Theme</label>
          <Select v-model="themeMode" :options="themeOptions" option-label="label" option-value="value" class="w-full" />
        </div>
      </section>
      <!-- Section: Regional -->
      <section class="space-y-3">
        <h2 class="text-sm font-semibold text-stone-900">Regional</h2>
        <div>
          <label class="block text-sm text-stone-600 mb-1">Currency</label>
          <Select v-model="preferences.currency" :options="currencies" option-label="label" option-value="value" class="w-full" />
        </div>
        <div>
          <label class="block text-sm text-stone-600 mb-1">Language</label>
          <Select v-model="preferences.language" :options="languages" option-label="label" option-value="value" class="w-full" />
        </div>
      </section>
      <!-- Section: Save -->
      <Button label="Save Preferences" class="w-full" @click="save" />
    </div>
  </div>
</template>
