<template>
  <!-- Section: Footer -->
  <footer class="bg-white border-t border-gray-200 mt-auto">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex flex-col md:flex-row justify-between items-center gap-4 text-sm text-gray-500">
        <p>&copy; {{ new Date().getFullYear() }} ReSys.Shop. All rights reserved.</p>
        <nav class="flex gap-6">
          <router-link to="/terms" class="hover:text-gray-700">Terms</router-link>
          <router-link to="/privacy" class="hover:text-gray-700">Privacy</router-link>
        </nav>
      </div>
    </div>
  </footer>
</template>
