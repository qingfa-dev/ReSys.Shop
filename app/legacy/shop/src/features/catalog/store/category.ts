import { defineStore } from 'pinia'
import { ref, computed, shallowRef } from 'vue'
import type { Category } from '../types'
import { categoryService } from '../services/category/category.service'

export const useCategoryStore = defineStore('category', () => {
  const categories = shallowRef<Category[]>([])
  const currentCategory = ref<Category | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const categoryCount = computed(() => categories.value.length)

  async function fetchCategories() {
    loading.value = true
    error.value = null
    try {
      const result = await categoryService.getCategories()
      if (result.isSuccess && result.data) {
        categories.value = result.data
      } else {
        error.value = result.message || 'Failed to fetch categories'
      }
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to fetch categories'
    } finally {
      loading.value = false
    }
  }

  function setCurrentCategory(category: Category | null) {
    currentCategory.value = category
  }

  function getCategoryBySlug(slug: string): Category | undefined {
    return categories.value.find(c => c.slug === slug)
  }

  function getCategoryById(id: string): Category | undefined {
    return categories.value.find(c => c.id === id)
  }

  return {
    categories,
    currentCategory,
    loading,
    error,
    categoryCount,
    fetchCategories,
    setCurrentCategory,
    getCategoryBySlug,
    getCategoryById,
  }
})
