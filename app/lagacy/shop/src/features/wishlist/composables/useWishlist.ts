import { useWishlistStore } from '../store/wishlist'

export function useWishlist() {
    const store = useWishlistStore()

    return {
        items: store.items,
        count: store.count,
        isEmpty: store.isEmpty,
        isWishlisted: store.isWishlisted,
        addToWishlist: store.addToWishlist,
        removeFromWishlist: store.removeFromWishlist,
        moveToCart: store.moveToCart,
    }
}
