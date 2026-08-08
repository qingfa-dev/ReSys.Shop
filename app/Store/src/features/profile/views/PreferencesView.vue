<script setup lang="ts">
import { ref } from 'vue'
import { usePageTitle } from '@/shared/composables/usePageTitle'
import { useNotify } from '@/shared/composables/useNotify'

usePageTitle('Preferences')
const notify = useNotify()

const currency = ref('USD')
const language = ref('English')

// Options: Supported currency and language selections
const currencies = ['USD', 'EUR', 'GBP', 'JPY']
const languages = ['English', 'Spanish', 'French', 'German']

// Placeholder: Show saved toast without server call
function save(): void {
  notify.success('Saved')
}
</script>
<template>
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Section: Page Header — breadcrumb navigation and title -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Preferences' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Preferences</h1>

    <!-- Section: Content Card — currency and language selectors -->
    <Card>
      <template #content>
        <div class="space-y-6">
          <div>
            <label for="currency" class="block text-sm font-medium text-neutral-700 mb-1">
              Currency
            </label>
            <Select
              id="currency"
              v-model="currency"
              :options="currencies"
              class="w-full max-w-xs"
            />
          </div>

          <div>
            <label for="language" class="block text-sm font-medium text-neutral-700 mb-1">
              Language
            </label>
            <Select
              id="language"
              v-model="language"
              :options="languages"
              class="w-full max-w-xs"
            />
          </div>

          <!-- Section: Action Footer — save button -->
          <Button label="Save" @click="save" />
        </div>
      </template>
    </Card>
  </div>
</template>
