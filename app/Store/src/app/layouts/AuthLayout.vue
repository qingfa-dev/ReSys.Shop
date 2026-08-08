<template>
  <div class="min-h-screen flex items-center justify-center bg-neutral-50 px-4">
    <div class="w-full max-w-md animate-fade-in">
      <div class="text-center mb-8">
        <router-link to="/" class="text-2xl font-semibold tracking-tight text-neutral-900">
          ReSys.Shop
        </router-link>
      </div>
      <router-view />
    </div>
  </div>
</template>

<style scoped>
.animate-fade-in {
  animation: fade-in 0.4s ease both;
}

@keyframes fade-in {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .animate-fade-in {
    animation: none;
  }
}
</style>
