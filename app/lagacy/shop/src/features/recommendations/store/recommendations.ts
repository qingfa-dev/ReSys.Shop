import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { RecommendationSet } from '../types'

export const useRecommendationsStore = defineStore('recommendations', () => {
    const recommendations = ref<RecommendationSet[]>([])
    const isLoading = ref(false)
    const userRecommendations = ref<RecommendationSet | null>(null)
    const productRecommendations = ref<RecommendationSet | null>(null)

    function setRecommendations(items: RecommendationSet[]) {
        recommendations.value = items
    }

    function setUserRecommendations(rec: RecommendationSet | null) {
        userRecommendations.value = rec
    }

    function setProductRecommendations(rec: RecommendationSet | null) {
        productRecommendations.value = rec
    }

    async function fetchPersonalizedRecommendations(userId: string) {
        isLoading.value = true
        try {
            // TODO: Call recommendations service
            // const result = await recommendationsService.getPersonalized(userId)
            // userRecommendations.value = result
        } finally {
            isLoading.value = false
        }
    }

    async function fetchProductRecommendations(productId: string) {
        isLoading.value = true
        try {
            // TODO: Call recommendations service
            // const result = await recommendationsService.getForProduct(productId)
            // productRecommendations.value = result
        } finally {
            isLoading.value = false
        }
    }

    return {
        recommendations,
        isLoading,
        userRecommendations,
        productRecommendations,
        setRecommendations,
        setUserRecommendations,
        setProductRecommendations,
        fetchPersonalizedRecommendations,
        fetchProductRecommendations,
    }
})
