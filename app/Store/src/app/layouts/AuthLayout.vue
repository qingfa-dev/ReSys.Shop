<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

// Compute: Derive the opposite auth target for the footer secondary link.
const route = useRoute()
const secondaryAuth = computed(() =>
  route.path === '/login'
    ? { label: 'Create account', to: '/register' }
    : { label: 'Sign in', to: '/login' },
)
</script>

<template>
  <!-- Section: Split Shell — brand panel (lg+) beside the form panel -->
  <div class="flex min-h-screen bg-surface-50">
    <!-- Brand Panel: Hidden below lg; brand mark, tagline and sparkle accent -->
    <aside
      class="hidden w-1/2 flex-col items-center justify-center gap-8 bg-gradient-to-br from-primary-950 via-primary-900 to-primary-600 p-12 lg:flex"
    >
      <div class="flex items-center gap-4">
        <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-surface-0/10">
          <i class="pi pi-sparkles text-3xl text-brand-subtle" />
        </div>
        <span class="text-4xl font-semibold tracking-tight text-on-brand">ReSys.Shop</span>
      </div>
      <p class="max-w-sm text-center text-lg leading-relaxed text-brand-subtle">
        Fashion that moves with you — curated looks, effortless checkout, delivered worldwide.
      </p>
    </aside>

    <!-- Form Panel: Centered auth card; routed views render inside the Fluid slot -->
    <main class="flex w-full items-center justify-center p-4 lg:w-1/2">
      <Card class="w-full max-w-md">
        <template #content>
          <!-- Mobile Brand Mark: Shown below lg where the brand panel is hidden -->
          <div class="mb-8 flex flex-col items-center gap-2 lg:hidden">
            <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-brand/10">
              <i class="pi pi-sparkles text-xl text-brand" />
            </div>
            <span class="text-xl font-semibold tracking-tight text-heading">ReSys.Shop</span>
          </div>

          <!-- Form Slot: Fluid makes all descendant inputs full-width -->
          <Fluid class="w-full">
            <slot />
          </Fluid>

          <Divider align="center" />

          <!-- Footer Links: Back to store plus the opposite auth route -->
          <div class="flex flex-wrap items-center justify-between gap-2">
            <Button as="router-link" to="/" text label="Back to store" icon="pi pi-arrow-left" />
            <Button
              as="router-link"
              :to="secondaryAuth.to"
              text
              :label="secondaryAuth.label"
              icon="pi pi-arrow-right"
              iconPos="right"
            />
          </div>
        </template>
      </Card>
    </main>
  </div>
</template>
