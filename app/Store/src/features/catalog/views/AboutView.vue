<script setup lang="ts">
import { useRouter } from 'vue-router'
import Button from 'primevue/button'

const router = useRouter()
</script>
<template>
  <!-- Section: About Page -->
  <div>
    <!-- Section: Hero -->
    <section class="bg-gradient-to-br from-teal-800 via-teal-700 to-teal-900 text-white py-24">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
        <h1 class="text-4xl md:text-6xl font-bold font-['Playfair_Display'] mb-4">About ReSys</h1>
        <p class="text-xl text-teal-100 max-w-2xl mx-auto">Fashion meets technology</p>
      </div>
    </section>

    <!-- Section: Our Story -->
    <section class="py-16">
      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <h2 class="text-3xl font-bold text-stone-900 font-['Playfair_Display'] mb-6">Our Story</h2>
        <p class="text-stone-600 leading-relaxed mb-4">
          ReSys was born from a simple idea: make fashion discovery effortless. We combine cutting-edge AI technology with curated fashion to help you find exactly what you're looking for.
        </p>
        <p class="text-stone-600 leading-relaxed">
          Our visual search technology lets you upload any fashion image and instantly find similar products from our collection. No more endless scrolling — just upload, discover, and shop.
        </p>
      </div>
    </section>

    <!-- Section: Our Mission -->
    <section class="py-16 bg-stone-50">
      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        <h2 class="text-3xl font-bold text-stone-900 font-['Playfair_Display'] mb-6">Our Mission</h2>
        <p class="text-stone-600 leading-relaxed">
          We believe technology should serve style, not replace it. Our mission is to bridge the gap between inspiration and purchase, making every fashion find just one upload away.
        </p>
      </div>
    </section>

    <!-- Section: Values -->
    <section class="py-16">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <h2 class="text-3xl font-bold text-stone-900 font-['Playfair_Display'] mb-8 text-center">Our Values</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          <div class="bg-white rounded-xl border border-stone-200 p-6 text-center">
            <i class="pi pi-star text-3xl text-teal-600 mb-4" />
            <h3 class="font-semibold text-stone-900 mb-2">Quality</h3>
            <p class="text-sm text-stone-600">Every product meets our high standards</p>
          </div>
          <div class="bg-white rounded-xl border border-stone-200 p-6 text-center">
            <i class="pi pi-heart text-3xl text-teal-600 mb-4" />
            <h3 class="font-semibold text-stone-900 mb-2">Sustainability</h3>
            <p class="text-sm text-stone-600">Responsible fashion for a better future</p>
          </div>
          <div class="bg-white rounded-xl border border-stone-200 p-6 text-center">
            <i class="pi pi-bolt text-3xl text-teal-600 mb-4" />
            <h3 class="font-semibold text-stone-900 mb-2">Innovation</h3>
            <p class="text-sm text-stone-600">AI-powered discovery for effortless shopping</p>
          </div>
          <div class="bg-white rounded-xl border border-stone-200 p-6 text-center">
            <i class="pi pi-users text-3xl text-teal-600 mb-4" />
            <h3 class="font-semibold text-stone-900 mb-2">Customer First</h3>
            <p class="text-sm text-stone-600">Your satisfaction drives everything we do</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Section: CTA -->
    <section class="py-16 bg-stone-900 text-white">
      <div class="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
        <h2 class="text-3xl font-bold font-['Playfair_Display'] mb-4">Ready to Discover?</h2>
        <p class="text-stone-300 mb-8">Explore our collection and find your next favorite piece.</p>
        <Button label="Shop Now" icon="pi pi-arrow-right" iconPos="right" size="large" @click="router.push('/shop')" />
      </div>
    </section>
  </div>
</template>
