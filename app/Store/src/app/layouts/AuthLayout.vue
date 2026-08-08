<template>
  <div class="min-h-screen flex items-center justify-center bg-neutral-50 px-4">
    <div class="w-full max-w-md">
      <div class="text-center mb-8">
        <router-link to="/" class="text-2xl font-semibold tracking-tight text-neutral-900">
          ReSys.Shop
        </router-link>
      </div>
      <router-view />
    </div>
  </div>
</template>
