<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'

// Title: Browser tab title for the privacy page
usePageTitle('Privacy Policy')

// Copy: Static privacy sections rendered as prose cards
const sections = [
  {
    heading: '1. Information We Collect',
    body: 'We collect the information you provide when creating an account, placing an order, or contacting support, along with browsing data used to improve your shopping experience.',
  },
  {
    heading: '2. How We Use Your Information',
    body: 'Your information is used to process orders, personalize recommendations, and keep you informed about your purchases. We never sell your personal data to third parties.',
  },
  {
    heading: '3. Cookies and Analytics',
    body: 'We use cookies to remember your preferences and to measure how our storefront is used. You can disable cookies in your browser, though some features may not work as intended.',
  },
  {
    heading: '4. Data Retention',
    body: 'Account and order records are kept for as long as your account is active or as required by law. You may request deletion of your personal data at any time.',
  },
  {
    heading: '5. Your Rights',
    body: 'You have the right to access, correct, or delete the personal data we hold about you, and to object to certain processing. Contact support@resys.shop to exercise these rights.',
  },
]
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Section: Page Header — breadcrumb, heading and updated date -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Privacy Policy' }]" class="mb-6" />
    <h1 class="mb-2 text-2xl font-semibold tracking-tight text-surface-900 dark:text-surface-100">
      Privacy Policy
    </h1>
    <p class="mb-8 text-sm text-surface-400 dark:text-surface-500">Last updated: August 8, 2026</p>

    <!-- Section: Editorial Blocks — one prose card per section -->
    <Card v-for="section in sections" :key="section.heading" class="mb-6">
      <template #title>
        <h2 class="text-lg font-semibold text-surface-900 dark:text-surface-100">
          {{ section.heading }}
        </h2>
      </template>
      <template #content>
        <p class="leading-relaxed text-surface-600 dark:text-surface-400">{{ section.body }}</p>
      </template>
    </Card>
  </div>
</template>
