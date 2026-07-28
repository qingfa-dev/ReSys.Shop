<script setup lang="ts">
import { computed } from 'vue'
import type { Product } from '../../types'
import VisuallySimilar from './VisuallySimilar.vue'
import YouMightAlsoLike from './YouMightAlsoLike.vue'

interface Props {
  similarProducts?: Product[]
  alsoLikeProducts?: Product[]
}

const props = defineProps<Props>()

const hasSimilar = computed(() => props.similarProducts && props.similarProducts.length > 0)
const hasAlsoLike = computed(() => props.alsoLikeProducts && props.alsoLikeProducts.length > 0)
</script>

<template>
  <div class="product-recommendations">
    <VisuallySimilar v-if="hasSimilar" :products="similarProducts!" />
    <YouMightAlsoLike v-if="hasAlsoLike" :products="alsoLikeProducts!" />
  </div>
</template>

<style scoped lang="scss">
.product-recommendations {
  display: flex;
  flex-direction: column;
  gap: 3rem;
}
</style>
