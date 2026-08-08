<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'

usePageTitle('Privacy Policy')
</script>

<template>
  <!-- Section: Page Header — breadcrumb and heading -->
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Privacy Policy' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Privacy Policy</h1>

    <!-- Section: Content Card — static privacy policy content -->
    <div class="prose prose-neutral max-w-none">
      <p class="text-sm text-neutral-400 mb-6">Last updated: August 8, 2026</p>

      <h2 class="text-xl font-semibold text-neutral-900">1. Information We Collect</h2>
      <p class="text-neutral-600 leading-relaxed">
        We collect information you provide directly, including account details,
        payment information, and communications with us. We also collect usage
        data such as browsing history and search queries to improve our service.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">2. How We Use Your Information</h2>
      <p class="text-neutral-600 leading-relaxed">
        We use your information to process orders, personalize your shopping
        experience, improve our AI recommendations, and communicate with you
        about products and services.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">3. Data Sharing</h2>
      <p class="text-neutral-600 leading-relaxed">
        We do not sell your personal information. We may share data with
        service providers who assist in operating our platform, such as
        payment processors and shipping partners.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">4. Data Security</h2>
      <p class="text-neutral-600 leading-relaxed">
        We implement industry-standard security measures to protect your
        personal information. However, no method of transmission over
        the Internet is completely secure.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">5. Your Rights</h2>
      <p class="text-neutral-600 leading-relaxed">
        You have the right to access, correct, or delete your personal data.
        You may also opt out of marketing communications at any time.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">6. Contact Us</h2>
      <p class="text-neutral-600 leading-relaxed">
        For privacy-related inquiries, contact
        <a href="mailto:privacy@resys.shop" class="text-teal-600 hover:text-teal-800">
          privacy@resys.shop
        </a>.
      </p>
    </div>
  </div>
</template>
