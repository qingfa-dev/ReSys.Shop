<template>
  <!-- Section: Page Shell -->
  <div class="min-h-screen flex flex-col bg-neutral-50">
    <AppHeader />
    <!-- Section: Main Content -->
    <main class="flex-1">
      <router-view />
    </main>
    <AppFooter />
  </div>
</template>
