import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Review, ReviewStats, ReviewFilter } from '../types'

export const useReviewsStore = defineStore('reviews', () => {
    const reviews = ref<Review[]>([])
    const stats = ref<ReviewStats | null>(null)
    const isLoading = ref(false)
    const currentFilter = ref<ReviewFilter>({})

    const filteredReviews = computed(() => {
        return reviews.value.filter(r => {
            if (currentFilter.value.rating && r.rating !== currentFilter.value.rating) return false
            if (currentFilter.value.verified && !r.verified) return false
            return true
        })
    })

    function setFilter(filter: ReviewFilter) {
        currentFilter.value = filter
    }

    function clearFilter() {
        currentFilter.value = {}
    }

    function addReview(review: Review) {
        reviews.value.push(review)
    }

    function removeReview(id: string) {
        reviews.value = reviews.value.filter(r => r.id !== id)
    }

    return {
        reviews,
        stats,
        isLoading,
        currentFilter,
        filteredReviews,
        setFilter,
        clearFilter,
        addReview,
        removeReview,
    }
})
