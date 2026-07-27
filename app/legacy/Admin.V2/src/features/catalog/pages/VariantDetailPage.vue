<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { DetailLayout, AppCard } from '@/shared/components'
import VariantForm from '../components/VariantForm.vue'
import VariantImageManager from '../components/VariantImageManager.vue'
import { VariantImageApi } from '../api'
import type { VariantImageDetailResponse } from '../types'

const route = useRoute()
const id = computed(() => route.params.id as string)

const images = ref<VariantImageDetailResponse[]>([])

async function loadImages() {
  try {
    const result = await VariantImageApi.list(id.value)
    if (result.isSuccess) {
      images.value = result.value.images
    }
  } catch (err) {
    console.error(err)
  }
}

onMounted(loadImages)
</script>

<template>
  <DetailLayout>
    <VariantForm />
    <template #sub-entities>
      <AppCard v-if="id">
        <VariantImageManager
          :variant-id="id"
          :images="images"
          @update:images="images = $event"
        />
      </AppCard>
    </template>
  </DetailLayout>
</template>
