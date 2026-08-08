<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'

// Title: Browser tab title for the terms page
usePageTitle('Terms of Service')

// Copy: Static term sections rendered inside the scroll panel
const sections = [
  {
    heading: '1. Acceptance of Terms',
    body: 'By accessing or using ReSys.Shop, you agree to be bound by these Terms of Service. If you do not agree, do not use the service.',
  },
  {
    heading: '2. Account Registration',
    body: 'You must provide accurate and complete information when creating an account. You are responsible for maintaining the confidentiality of your credentials.',
  },
  {
    heading: '3. Products and Orders',
    body: 'All product descriptions, images, and pricing are subject to change. We reserve the right to cancel orders that contain errors in pricing or product information.',
  },
  {
    heading: '4. Returns and Refunds',
    body: 'Items may be returned within 30 days of delivery in their original condition. Refunds are processed to the original payment method.',
  },
  {
    heading: '5. Limitation of Liability',
    body: 'ReSys.Shop shall not be liable for any indirect, incidental, or consequential damages arising from your use of the service.',
  },
  {
    heading: '6. Changes to Terms',
    body: 'We may update these terms from time to time. Continued use of the service after changes constitutes acceptance of the revised terms.',
  },
]
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Section: Page Header — breadcrumb, heading and updated date -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Terms of Service' }]" class="mb-6" />
    <h1 class="mb-2 text-2xl font-semibold tracking-tight text-surface-900 dark:text-surface-100">
      Terms of Service
    </h1>
    <p class="mb-8 text-sm text-surface-400 dark:text-surface-500">Last updated: August 8, 2026</p>

    <!-- Section: Scrollable Terms — long body inside a styled scroll panel -->
    <ScrollPanel class="max-h-[30rem]">
      <Card>
        <template #content>
          <div class="space-y-8">
            <section v-for="section in sections" :key="section.heading">
              <h2 class="mb-3 text-lg font-semibold text-surface-900 dark:text-surface-100">
                {{ section.heading }}
              </h2>
              <p class="leading-relaxed text-surface-600 dark:text-surface-400">{{ section.body }}</p>
            </section>
            <p class="text-sm text-surface-500 dark:text-surface-400">
              For questions about these terms, contact support@resys.shop.
            </p>
          </div>
        </template>
      </Card>
    </ScrollPanel>
  </div>
</template>
