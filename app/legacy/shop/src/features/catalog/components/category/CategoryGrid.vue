<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { RouterLink } from 'vue-router'
import CategoryCard from './CategoryCard.vue'
import { useCategoryStore } from '../../store/category'
import type { Category } from '../../types'

interface Props {
  viewAllRoute?: string
}

defineProps<Props>()

const categoryStore = useCategoryStore()
const categories = computed(() => categoryStore.categories)
const loading = computed(() => categoryStore.loading)

onMounted(() => {
  categoryStore.fetchCategories()
})

const emit = defineEmits<{
  (e: 'categoryClick', category: Category): void
}>()

function handleCategoryClick(category: Category) {
  emit('categoryClick', category)
}
</script>

<template>
  <section class="categories">
    <div class="section-header">
      <h2>Shop by Category</h2>
      <RouterLink v-if="viewAllRoute" :to="viewAllRoute" class="view-all">
        View All <i class="pi pi-arrow-right"></i>
      </RouterLink>
    </div>
    <div v-if="loading" class="loading-state">
      <i class="pi pi-spin pi-spinner"></i>
    </div>
    <div v-else class="category-grid">
      <RouterLink
        v-for="category in categories"
        :key="category.id"
        :to="`/shop?category=${category.slug}`"
        class="category-link"
      >
        <CategoryCard
          :name="category.name"
          :image="category.image"
          @click="handleCategoryClick(category)"
        />
      </RouterLink>
    </div>
  </section>
</template>

<style scoped lang="scss">
.categories {
  max-width: 1400px;
  margin: 0 auto;
  padding: 4rem 2rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;

  h2 {
    font-size: var(--font-size-3xl);
  }

  .view-all {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: var(--color-primary);

    &:hover {
      text-decoration: underline;
    }
  }
}

.loading-state {
  display: flex;
  justify-content: center;
  padding: 4rem 0;

  i {
    font-size: 2rem;
    color: var(--color-primary);
  }
}

.category-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1.5rem;

  @media (max-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
  }
}

.category-link {
  text-decoration: none;
}
</style>
