import { useReviewsStore } from '../store/reviews'

export function useReviews() {
    const store = useReviewsStore()

    return {
        reviews: store.reviews,
        stats: store.stats,
        isLoading: store.isLoading,
        filteredReviews: store.filteredReviews,
        setFilter: store.setFilter,
        clearFilter: store.clearFilter,
        addReview: store.addReview,
        removeReview: store.removeReview,
    }
}
