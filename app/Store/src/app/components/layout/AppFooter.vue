<script setup lang="ts">
import { ref } from 'vue'
import { useNotify } from '@/shared/composables/useNotify'

// Email: Newsletter capture model; client-side only until a service exists.
const email = ref('')
const notify = useNotify()

// Subscribe: Sanity-check the address shape, then confirm via toast.
function subscribe(): void {
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value.trim())) {
    notify.warn('Enter a valid email address')
    return
  }
  notify.success('Subscribed')
  email.value = ''
}
</script>

<template>
  <!-- Section: Footer — brand, link columns and newsletter capture -->
  <footer class="border-t border-surface-200 bg-surface-50">
    <div class="mx-auto grid max-w-screen-2xl gap-10 px-4 py-12 sm:grid-cols-2 sm:px-6 lg:grid-cols-4 lg:px-8">
      <!-- Brand: Wordmark with a one-line blurb about the storefront -->
      <div>
        <div class="flex items-center gap-2">
          <i class="pi pi-sparkles text-xl text-brand" />
          <span class="text-lg font-semibold tracking-tight">ReSys.Shop</span>
        </div>
        <p class="mt-3 text-sm text-muted">
          Fashion retail built on semantic search, visual discovery and curated collections.
        </p>
      </div>

      <!-- Shop Links: Catalog destinations -->
      <div>
        <h3 class="text-sm font-semibold uppercase tracking-wide text-heading">Shop</h3>
        <div class="mt-3 flex flex-col items-start gap-1">
          <Button as="router-link" to="/shop" label="Shop" variant="text" class="p-0" />
          <Button as="router-link" to="/collections" label="Collections" variant="text" class="p-0" />
          <Button as="router-link" to="/recommendations" label="Visual Search" variant="text" class="p-0" />
        </div>
      </div>

      <!-- Company Links: About and legal pages -->
      <div>
        <h3 class="text-sm font-semibold uppercase tracking-wide text-heading">Company</h3>
        <div class="mt-3 flex flex-col items-start gap-1">
          <Button as="router-link" to="/about" label="About" variant="text" class="p-0" />
          <Button as="router-link" to="/terms" label="Terms" variant="text" class="p-0" />
          <Button as="router-link" to="/privacy" label="Privacy" variant="text" class="p-0" />
        </div>
      </div>

      <!-- Newsletter: Email input group; subscribe confirms via toast -->
      <div>
        <h3 class="text-sm font-semibold uppercase tracking-wide text-heading">Newsletter</h3>
        <p class="mt-3 text-sm text-muted">
          Get new arrivals and promotions in your inbox.
        </p>
        <InputGroup class="mt-3">
          <InputText v-model="email" type="email" placeholder="you@example.com" aria-label="Email address" />
          <Button icon="pi pi-send" severity="secondary" aria-label="Subscribe" @click="subscribe" />
        </InputGroup>
      </div>
    </div>

    <!-- Copyright: Divider then the legal line -->
    <Divider />
    <div class="mx-auto max-w-screen-2xl px-4 pb-8 sm:px-6 lg:px-8">
      <p class="text-center text-sm text-muted">
        &copy; {{ new Date().getFullYear() }} ReSys.Shop. All rights reserved.
      </p>
    </div>
  </footer>
</template>
