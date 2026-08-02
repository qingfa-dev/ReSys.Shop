import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { RecommendationSet } from '../types'
import { recommendationsService } from '../services/recommendations.service'

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

    async function fetchPersonalizedRecommendations(_userId: string) {
        isLoading.value = true
        try {
            const result = await recommendationsService.getPersonalizedRecommendations()
            if (result.isSuccess && result.data) {
                userRecommendations.value = {
                    id: 'personalized',
                    type: 'personalized' as const,
                    title: 'Recommended for You',
                    products: result.data as unknown as RecommendationSet['products'],
                }
            }
        } finally {
            isLoading.value = false
        }
    }

    async function fetchProductRecommendations(productId: string) {
        isLoading.value = true
        try {
            const result = await recommendationsService.getSimilarProducts(productId)
            if (result.isSuccess && result.data) {
                productRecommendations.value = {
                    id: `similar-${productId}`,
                    type: 'visual-similarity' as const,
                    title: 'Similar Products',
                    products: result.data as unknown as RecommendationSet['products'],
                    contextProductId: productId,
                }
            }
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
