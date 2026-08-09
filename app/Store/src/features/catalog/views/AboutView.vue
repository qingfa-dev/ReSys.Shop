<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'

// Title: Browser tab title for the about page
usePageTitle('About Us')

// Copy: Static editorial sections rendered as prose cards
const sections = [
  {
    heading: 'Our Mission',
    body: 'We believe finding the right clothes should be effortless. Our platform combines cutting-edge AI technology with a curated selection of fashion to deliver a personalized shopping experience.',
  },
  {
    heading: 'How It Works',
    body: 'Upload a photo or describe what you are looking for, and our visual search engine will find matching items from our catalog. The more you shop, the better our recommendations become.',
  },
  {
    heading: 'Contact',
    body: 'Have questions? Reach out to us at support@resys.shop and our team will get back to you within one business day.',
  },
]
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
    <!-- Section: Page Header — breadcrumb and heading -->
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'About Us' }]" class="mb-6" />
    <h1 class="mb-8 text-2xl font-semibold tracking-tight text-heading">
      About Us
    </h1>

    <!-- Section: Intro — platform overview paragraph -->
    <Card class="mb-8">
      <template #content>
        <p class="text-lg leading-relaxed text-muted">
          ReSys.Shop is an AI-powered fashion e-commerce platform that helps you discover pieces
          matched to your personal style through intelligent curation.
        </p>
      </template>
    </Card>

    <!-- Section: Editorial Blocks — one prose card per section -->
    <Card v-for="section in sections" :key="section.heading" class="mb-6">
      <template #title>
        <h2 class="text-lg font-semibold text-heading">
          {{ section.heading }}
        </h2>
      </template>
      <template #content>
        <p class="leading-relaxed text-muted">{{ section.body }}</p>
      </template>
    </Card>
  </div>
</template>
