import { useRecommendationsStore } from '../store/recommendations'

export function useRecommendations() {
    const store = useRecommendationsStore()

    return {
        recommendations: store.recommendations,
        isLoading: store.isLoading,
        userRecommendations: store.userRecommendations,
        productRecommendations: store.productRecommendations,
        setRecommendations: store.setRecommendations,
        fetchPersonalizedRecommendations: store.fetchPersonalizedRecommendations,
        fetchProductRecommendations: store.fetchProductRecommendations,
    }
}
