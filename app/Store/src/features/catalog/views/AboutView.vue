<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'

usePageTitle('About Us')
</script>

<template>
  <!-- Section: Page Header — breadcrumb and heading -->
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'About Us' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">About Us</h1>

    <!-- Section: Content Card — static about page content -->
    <div class="prose prose-neutral max-w-none">
      <p class="text-lg text-neutral-600 leading-relaxed">
        ReSys.Shop is an AI-powered fashion e-commerce platform that helps you
        discover pieces matched to your personal style through intelligent curation.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">Our Mission</h2>
      <p class="text-neutral-600 leading-relaxed">
        We believe finding the right clothes should be effortless. Our platform
        combines cutting-edge AI technology with a curated selection of fashion
        to deliver a personalized shopping experience.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">How It Works</h2>
      <p class="text-neutral-600 leading-relaxed">
        Upload a photo or describe what you are looking for, and our visual search
        engine will find matching items from our catalog. The more you shop, the
        better our recommendations become.
      </p>

      <h2 class="text-xl font-semibold text-neutral-900 mt-8">Contact</h2>
      <p class="text-neutral-600 leading-relaxed">
        Have questions? Reach out to us at
        <a href="mailto:support@resys.shop" class="text-teal-600 hover:text-teal-800">
          support@resys.shop
        </a>.
      </p>
    </div>
  </div>
</template>
