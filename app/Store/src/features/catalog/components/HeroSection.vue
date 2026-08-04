<template>
  <!-- Section: Hero Banner -->
  <section class="bg-gradient-to-br from-teal-800 via-teal-700 to-teal-900 text-white">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24 md:py-32 text-center">
      <span class="inline-block bg-white/20 text-white text-sm font-medium px-4 py-1 rounded-full mb-6">
        New Collection
      </span>
      <h1 class="text-4xl md:text-6xl font-bold mb-4">Discover Your Style</h1>
      <p class="text-lg text-white/70 mb-8 max-w-xl mx-auto">
        Shop the latest fashion trends with visual search. Upload an image, find your look.
      </p>
      <div class="flex justify-center gap-4">
        <router-link to="/shop">
          <Button label="Shop All" size="large" />
        </router-link>
        <router-link to="/recommendations">
          <Button label="Visual Search" severity="secondary" size="large" />
        </router-link>
      </div>
    </div>
  </section>
</template>