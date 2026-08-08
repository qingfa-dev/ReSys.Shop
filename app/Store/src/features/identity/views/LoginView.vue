<script setup lang="ts">
import { ref } from "vue";
import { usePageTitle } from "@/shared/composables/usePageTitle";
import { useAuthStore } from "../stores/authStore";

usePageTitle("Sign In");
const auth = useAuthStore();
const credential = ref("");
const password = ref("");
</script>
<template>
  <div class="max-w-md mx-auto py-16 px-4">
    <Breadcrumb :model="[{ label: 'Home', to: '/' }, { label: 'Sign In' }]" />
    <h1 class="text-2xl font-bold text-neutral-900 mt-4 mb-8">Sign In</h1>
    <Card>
      <template #content>
        <div class="space-y-4">
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
          <Skeleton width="100%" height="2.5rem" />
        </div>
      </template>
    </Card>
    <Message severity="info" class="mt-4"
      >Login form with PrimeVue InputText, Password, Button, and validation will be implemented
      here.</Message
    >
  </div>
</template>
