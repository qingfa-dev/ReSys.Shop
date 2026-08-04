<template>
  <!-- Section: Not Found Page -->
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24 text-center">
    <i class="pi pi-exclamation-circle text-5xl text-gray-300 mb-6 block" />
    <h1 class="text-3xl font-bold text-gray-900 mb-3">Page not found</h1>
    <p class="text-gray-500 mb-8">The page you are looking for does not exist or has been moved.</p>
    <router-link to="/">
      <Button label="Back to Home" icon="pi pi-home" />
    </router-link>
  </div>
</template>
