<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'

usePageTitle('Terms of Service')
</script>

<template>
  <!-- Section: Page Header — breadcrumb and heading -->
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Terms of Service' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Terms of Service</h1>

    <!-- Section: Content Card — static terms of service content -->
    <div class="prose prose-neutral max-w-none">
      <p class="text-sm text-neutral-400 mb-6">Last updated: August 8, 2026</p>

      <h2 class="text-xl font-semibold text-neutral-900">1. Acceptance of Terms</h2>
      <p class="text-neutral-600 leading-relaxed">
        By accessing or using ReSys.Shop, you agree to be bound by these Terms
        of Service. If you do not agree, do not use the service.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">2. Account Registration</h2>
      <p class="text-neutral-600 leading-relaxed">
        You must provide accurate and complete information when creating an account.
        You are responsible for maintaining the confidentiality of your credentials.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">3. Products and Orders</h2>
      <p class="text-neutral-600 leading-relaxed">
        All product descriptions, images, and pricing are subject to change.
        We reserve the right to cancel orders that contain errors in pricing
        or product information.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">4. Returns and Refunds</h2>
      <p class="text-neutral-600 leading-relaxed">
        Items may be returned within 30 days of delivery in their original
        condition. Refunds are processed to the original payment method.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">5. Limitation of Liability</h2>
      <p class="text-neutral-600 leading-relaxed">
        ReSys.Shop shall not be liable for any indirect, incidental, or
        consequential damages arising from your use of the service.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">6. Changes to Terms</h2>
      <p class="text-neutral-600 leading-relaxed">
        We may update these terms from time to time. Continued use of the
        service after changes constitutes acceptance of the revised terms.
      </p>

      <p class="text-neutral-500 mt-8">
        For questions about these terms, contact
        <a href="mailto:support@resys.shop" class="text-teal-600 hover:text-teal-800">
          support@resys.shop
        </a>.
      </p>
    </div>
  </div>
</template>
