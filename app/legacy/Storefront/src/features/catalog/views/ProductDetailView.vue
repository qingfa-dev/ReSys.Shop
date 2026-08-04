<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import Button from "primevue/button";
import Rating from "primevue/rating";
import Dialog from "primevue/dialog";
import InputText from "primevue/inputtext";
import Textarea from "primevue/textarea";
import Select from "primevue/select";
import { useCatalog } from "../composables/useCatalog";
import { useCart } from "@/features/ordering/composables/useCart";
import ProductGallery from "../components/product/ProductGallery.vue";
import ProductPrice from "../components/product/ProductPrice.vue";
import ProductOptionPicker from "../components/product/ProductOptionPicker.vue";
import ProductDescription from "../components/product/ProductDescription.vue";
import ProductDetailsInfo from "../components/product/ProductDetailsInfo.vue";
import ProductQuality from "../components/product/ProductQuality.vue";
import ProductRecommendations from "../components/recommendations/ProductRecommendations.vue";
import ReviewList from "@/features/reviews/components/ReviewList.vue";
import type { ProductDetail, ProductImage, ProductColor, ProductSize } from "../types";
import type { Review } from "@/features/reviews/types";

const route = useRoute();
const router = useRouter();
const {
  currentProduct,
  isLoading,
  loadProduct,
  loadProducts,
  products: allProducts,
} = useCatalog();
const { addToCart } = useCart();

const productId = route.params.id as string;
const selectedColorId = ref("");
const selectedSizeId = ref("");
const quantity = ref(1);
const showSizeGuide = ref(false);

const mockColors: ProductColor[] = [
  { id: "col-black", name: "Black", hex: "#1a1a1a" },
  { id: "col-white", name: "White", hex: "#ffffff" },
  { id: "col-navy", name: "Navy", hex: "#1e3a5f" },
  { id: "col-red", name: "Red", hex: "#dc2626" },
];

const mockSizes: ProductSize[] = [
  { id: "sz-xs", name: "XS", stock: 5 },
  { id: "sz-s", name: "S", stock: 12 },
  { id: "sz-m", name: "M", stock: 20 },
  { id: "sz-l", name: "L", stock: 15 },
  { id: "sz-xl", name: "XL", stock: 8 },
  { id: "sz-xxl", name: "XXL", stock: 3 },
];

const mockReviews: Review[] = [
  {
    id: "rev-1",
    productId: productId,
    userId: "user-1",
    userName: "Sarah M.",
    rating: 5,
    title: "Absolutely love this!",
    body: "This product exceeded my expectations. The quality is outstanding and it fits perfectly. I've received so many compliments!",
    images: [],
    verified: true,
    fit: "true-to-size",
    helpful: 24,
    unhelpful: 2,
    createdAt: "2026-03-15T10:30:00Z",
    status: "approved",
  },
  {
    id: "rev-2",
    productId: productId,
    userId: "user-2",
    userName: "Jessica K.",
    rating: 4,
    title: "Great quality, runs slightly small",
    body: "The material is beautiful and the design is elegant. However, I would recommend sizing up as it runs a bit small.",
    images: [],
    verified: true,
    fit: "runs-small",
    helpful: 18,
    unhelpful: 5,
    createdAt: "2026-03-10T14:20:00Z",
    status: "approved",
  },
  {
    id: "rev-3",
    productId: productId,
    userId: "user-3",
    userName: "Amanda R.",
    rating: 5,
    title: "Perfect for any occasion",
    body: "I've worn this to work, on dates, and even to formal events. It's incredibly versatile and always looks great.",
    images: [],
    verified: true,
    fit: "true-to-size",
    helpful: 31,
    unhelpful: 1,
    createdAt: "2026-02-28T09:15:00Z",
    status: "approved",
  },
  {
    id: "rev-4",
    productId: productId,
    userId: "user-4",
    userName: "Michelle T.",
    rating: 3,
    title: "Good but expected more",
    body: "The style is nice but the fabric feels thinner than expected. Still, a decent purchase for the price.",
    images: [],
    verified: false,
    fit: "true-to-size",
    helpful: 8,
    unhelpful: 3,
    createdAt: "2026-02-20T16:45:00Z",
    status: "approved",
  },
  {
    id: "rev-5",
    productId: productId,
    userId: "user-5",
    userName: "Emily L.",
    rating: 5,
    title: "Best purchase this year!",
    body: "I can't say enough good things about this. The attention to detail is impeccable and it washes beautifully.",
    images: [],
    verified: true,
    fit: "true-to-size",
    helpful: 42,
    unhelpful: 0,
    createdAt: "2026-02-15T11:00:00Z",
    status: "approved",
  },
];

const showWriteReview = ref(false);

const reviewForm = ref({
  rating: 0,
  title: "",
  body: "",
  fit: "" as "" | "true-to-size" | "runs-small" | "runs-large",
});

const fitOptions = [
  { label: "True to Size", value: "true-to-size" },
  { label: "Runs Small", value: "runs-small" },
  { label: "Runs Large", value: "runs-large" },
];

function submitReview() {
  console.log("Submit review:", reviewForm.value);
  showWriteReview.value = false;
  reviewForm.value = { rating: 0, title: "", body: "", fit: "" };
}

onMounted(async () => {
  if (!productId) {
    router.push("/shop");
    return;
  }
  await loadProduct(productId);
  await loadProducts();
  if (!currentProduct.value) {
    router.push("/shop");
    return;
  }
  const p = currentProduct.value as ProductDetail;
  const colors = p.colors || mockColors.slice(0, 3);
  const sizes = p.sizes || mockSizes;
  if (colors.length > 0 && !selectedColorId.value) {
    const firstColor = colors[0];
    if (firstColor) selectedColorId.value = firstColor.id;
  }
  if (sizes.length > 0 && !selectedSizeId.value) {
    const firstSize = sizes[0];
    if (firstSize) selectedSizeId.value = firstSize.id;
  }
});

const product = computed(() => {
  const p = currentProduct.value as ProductDetail | null;
  if (!p) return null;
  return {
    ...p,
    colors: p.colors || mockColors.slice(0, 3),
    sizes: p.sizes || mockSizes,
    inStock: p.inStock ?? true,
  };
});

const productImages = computed(() => {
  if (!product.value?.images) return [];
  return product.value.images.map((img: string | ProductImage) => {
    if (typeof img === "string") return { url: img, alt: "" };
    return img;
  });
});

const canAddToCart = computed(() => {
  const hasColors = product.value?.colors && product.value.colors.length > 0;
  const hasSizes = product.value?.sizes && product.value.sizes.length > 0;

  const colorSelected = hasColors ? !!selectedColorId.value : true;
  const sizeSelected = hasSizes ? !!selectedSizeId.value : true;

  return product.value?.inStock && colorSelected && sizeSelected && quantity.value > 0;
});

const similarProducts = computed(() => {
  return allProducts.value.slice(0, 4);
});

const alsoLikeProducts = computed(() => {
  return allProducts.value.slice(4, 8);
});

const reviewStats = computed(() => {
  const total = mockReviews.length;
  const avgRating = mockReviews.reduce((sum, r) => sum + r.rating, 0) / total;
  const distribution = { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 };
  mockReviews.forEach((r) => {
    distribution[r.rating as keyof typeof distribution]++;
  });
  return {
    total,
    averageRating: avgRating.toFixed(1),
    distribution: Object.fromEntries(
      Object.entries(distribution).map(([star, count]) => [
        star,
        { count, percentage: (count / total) * 100 },
      ]),
    ),
    verifiedCount: mockReviews.filter((r) => r.verified).length,
  };
});

async function handleAddToCart() {
  if (!canAddToCart.value || !product.value) return;

  await addToCart(
    product.value.variants?.[0]?.id ?? product.value.id,
    quantity.value,
  );
}

async function handleBuyNow() {
  await handleAddToCart();
  router.push("/checkout");
}

function goBack() {
  router.back();
}

function openSizeGuide() {
  showSizeGuide.value = true;
}
</script>

<template>
  <div class="product-detail">
    <div v-if="isLoading" class="loading-state">
      <i class="pi pi-spin pi-spinner"></i>
      <span>Loading product...</span>
    </div>

    <div v-else-if="!product" class="not-found">
      <i class="pi pi-exclamation-circle"></i>
      <p>Product not found</p>
      <Button label="Back to Shop" @click="goBack" />
    </div>

    <div v-else class="product-detail__content">
      <nav class="breadcrumb">
        <router-link to="/">Home</router-link>
        <span class="separator">/</span>
        <router-link to="/shop">Shop</router-link>
        <span class="separator">/</span>
        <span>{{ product.name }}</span>
      </nav>

      <div class="product-main">
        <div class="product-gallery">
          <ProductGallery :images="productImages" />
        </div>

        <div class="product-info">
          <div class="product-header">
            <span v-if="product.brand" class="brand">{{ product.brand }}</span>
            <h1 class="product-title">{{ product.name }}</h1>
          </div>

          <div class="product-rating">
            <Rating v-model="product.rating" :cancel="false" readonly />
            <span class="rating-value">{{ product.rating }}</span>
            <span class="review-count">({{ product.reviews }} reviews)</span>
          </div>

          <ProductPrice
            :price="product.price"
            :compare-at-price="product.compareAtPrice"
            :in-stock="product.inStock"
          />

          <ProductDescription
            :short-description="product.description"
            :long-description="product.longDescription"
          />

          <ProductOptionPicker
            :colors="product.colors"
            :sizes="product.sizes"
            :show-quantity="true"
            :max-quantity="
              selectedSizeId ? product.sizes?.find((s) => s.id === selectedSizeId)?.stock || 10 : 10
            "
            @update:selected-color="selectedColorId = $event"
            @update:selected-size="selectedSizeId = $event"
            @update:quantity="quantity = $event"
            @open-size-guide="openSizeGuide"
          />

          <div class="product-actions">
            <div v-if="!canAddToCart && product?.inStock" class="selection-required">
              <small v-if="product?.colors?.length && !selectedColorId"
                >Please select a color</small
              >
              <small v-else-if="product?.sizes?.length && !selectedSizeId"
                >Please select a size</small
              >
            </div>
            <Button
              label="Add to Cart"
              class="btn-add-cart"
              :disabled="!canAddToCart"
              @click="handleAddToCart"
            />
            <Button
              label="Buy Now"
              class="btn-buy-now"
              :disabled="!canAddToCart"
              @click="handleBuyNow"
            />
          </div>

          <ProductDetailsInfo
            :material="product.material"
            :care-instructions="product.careInstructions"
            :dimensions="product.dimensions"
            :weight="product.weight"
            :origin="product.origin"
          />

          <ProductQuality
            :features="[
              { icon: 'pi pi-truck', title: 'Free Shipping', description: 'On orders over $50' },
              { icon: 'pi pi-refresh', title: 'Easy Returns', description: '30-day return policy' },
              {
                icon: 'pi pi-shield',
                title: 'Secure Payment',
                description: '100% secure checkout',
              },
            ]"
            warranty="1-year limited warranty"
            :certifications="['OEKO-TEX', 'GOTS Certified']"
            shipping="Free shipping on orders over $50"
            returns="30-day hassle-free returns"
          />
        </div>
      </div>

      <!-- MVP: dropped — no storefront API for reviews -->
      <div v-if="false">
      <div class="reviews-section">
        <div class="reviews-header">
          <h2>Customer Reviews</h2>
          <p class="reviews-subtitle">What our customers say about this product</p>
        </div>

        <div class="reviews-summary">
          <div class="rating-overview">
            <div class="average-rating">
              <span class="rating-number">{{ reviewStats.averageRating }}</span>
              <Rating
                :model-value="Math.round(Number(reviewStats.averageRating))"
                :cancel="false"
                readonly
              />
              <span class="total-reviews">{{ reviewStats.total }} reviews</span>
            </div>

            <div class="rating-distribution">
              <div v-for="star in [5, 4, 3, 2, 1]" :key="star" class="distribution-row">
                <span class="star-label">{{ star }} star</span>
                <div class="bar-container">
                  <div
                    class="bar-fill"
                      :style="{
                        width: `${reviewStats.distribution[star as keyof typeof reviewStats.distribution]?.percentage ?? 0}%`,
                      }"
                  ></div>
                </div>
                <span class="count-label">{{
                  reviewStats.distribution[star as keyof typeof reviewStats.distribution]?.count ?? 0
                }}</span>
              </div>
            </div>

            <div class="verified-badge">
              <i class="pi pi-check-circle"></i>
              <span>{{ reviewStats.verifiedCount }} verified purchases</span>
            </div>
          </div>

          <Button
            label="Write a Review"
            icon="pi pi-pencil"
            class="btn-write-review"
            @click="showWriteReview = true"
          />
        </div>

        <ReviewList :reviews="mockReviews" />
      </div>
      </div>

      <ProductRecommendations
        :similar-products="similarProducts"
        :also-like-products="alsoLikeProducts"
      />
    </div>

    <Dialog v-model:visible="showSizeGuide" header="Size Guide" modal class="size-guide-dialog">
      <table class="size-guide-table">
        <thead>
          <tr>
            <th>Size</th>
            <th>Chest (in)</th>
            <th>Length (in)</th>
            <th>Sleeves (in)</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="row in product?.sizeChart" :key="row.size">
            <td>{{ row.size }}</td>
            <td>{{ row.chest }}</td>
            <td>{{ row.length }}</td>
            <td>{{ row.sleeves }}</td>
          </tr>
        </tbody>
      </table>
    </Dialog>

    <!-- MVP: dropped — no storefront API for reviews -->
    <div v-if="false">
    <Dialog
      v-model:visible="showWriteReview"
      header="Write a Review"
      modal
      class="write-review-dialog"
      :style="{ width: '500px' }"
    >
      <div class="review-form">
        <div class="form-group">
          <label class="form-label">Your Rating *</label>
          <Rating v-model="reviewForm.rating" :cancel="false" class="rating-input" />
        </div>

        <div class="form-group">
          <label class="form-label">Review Title *</label>
          <InputText
            v-model="reviewForm.title"
            placeholder="Summarize your experience"
            class="form-input"
          />
        </div>

        <div class="form-group">
          <label class="form-label">Your Review *</label>
          <Textarea
            v-model="reviewForm.body"
            placeholder="What did you like or dislike? Would you recommend this product?"
            rows="5"
            class="form-textarea"
          />
        </div>

        <div class="form-group">
          <label class="form-label">How does it fit?</label>
          <Select
            v-model="reviewForm.fit"
            :options="fitOptions"
            optionLabel="label"
            optionValue="value"
            placeholder="Select fit (optional)"
            class="form-select"
          />
        </div>

        <div class="form-actions">
          <Button label="Cancel" severity="secondary" outlined @click="showWriteReview = false" />
          <Button
            label="Submit Review"
            icon="pi pi-send"
            :disabled="!reviewForm.rating || !reviewForm.title || !reviewForm.body"
            @click="submitReview"
          />
        </div>
      </div>
    </Dialog>
    </div>
  </div>
</template>

<style scoped lang="scss">
.product-detail {
  max-width: 1400px;
  margin: 0 auto;
  padding: 2rem;
}

.loading-state,
.not-found {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
  gap: 1rem;

  i {
    font-size: 3rem;
    color: var(--color-text-muted);
  }

  p {
    font-size: var(--font-size-lg);
    color: var(--color-text-muted);
  }
}

.breadcrumb {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
  margin-bottom: 2rem;

  a {
    color: inherit;
    text-decoration: none;
    transition: color var(--transition-fast);

    &:hover {
      color: var(--color-primary);
    }
  }

  .separator {
    color: var(--color-border);
  }
}

.product-main {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 3rem;
  margin-bottom: 4rem;

  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
  }
}

.product-gallery {
  position: sticky;
  top: 2rem;
  align-self: start;
}

.product-info {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.product-header {
  .brand {
    font-size: var(--font-size-sm);
    color: var(--color-text-muted);
    text-transform: uppercase;
    letter-spacing: 1px;
  }

  .product-title {
    font-size: var(--font-size-3xl);
    font-weight: var(--font-weight-bold);
    margin: 0.5rem 0;
    line-height: 1.2;
  }
}

.product-rating {
  display: flex;
  align-items: center;
  gap: 0.5rem;

  .rating-value {
    font-weight: var(--font-weight-semibold);
  }

  .review-count {
    color: var(--color-text-muted);
    font-size: var(--font-size-sm);
  }
}

.product-actions {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border-light);

  .selection-required {
    color: var(--color-danger);
    font-size: var(--font-size-sm);
    text-align: center;
  }

  .btn-add-cart {
    flex: 1;
    padding: 1rem;
    background: var(--color-primary);
    border: none;
    border-radius: var(--radius-md);
    font-weight: var(--font-weight-semibold);
    cursor: pointer;
    transition: all var(--transition-fast);

    &:hover:not(:disabled) {
      background: var(--color-primary-hover);
    }

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  }

  .btn-buy-now {
    flex: 1;
    padding: 1rem;
    background: var(--color-text);
    color: white;
    border: none;
    border-radius: var(--radius-md);
    font-weight: var(--font-weight-semibold);
    cursor: pointer;
    transition: all var(--transition-fast);

    &:hover:not(:disabled) {
      background: var(--color-text-muted);
    }

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  }
}

:deep(.size-guide-dialog) {
  .p-dialog-content {
    padding: 1rem;
  }
}

.size-guide-table {
  width: 100%;
  border-collapse: collapse;

  th,
  td {
    padding: 0.75rem;
    text-align: left;
    border-bottom: 1px solid var(--color-border-light);
  }

  th {
    background: var(--color-surface-ground);
    font-weight: var(--font-weight-semibold);
  }
}

.reviews-section {
  margin-top: 4rem;
  padding-top: 3rem;
  border-top: 1px solid var(--color-border-light);
}

.reviews-header {
  text-align: center;
  margin-bottom: 2rem;

  h2 {
    font-family: var(--font-display);
    font-size: var(--font-size-2xl);
    color: var(--color-text);
    margin-bottom: 0.5rem;
  }

  .reviews-subtitle {
    color: var(--color-text-secondary);
    font-size: var(--font-size-base);
  }
}

.reviews-summary {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 2rem;
  margin-bottom: 3rem;
  padding: 2rem;
  background: var(--color-surface-ground);
  border-radius: var(--radius-lg);

  @media (max-width: 768px) {
    flex-direction: column;
  }
}

.rating-overview {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  flex: 1;
}

.average-rating {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;

  .rating-number {
    font-size: 3rem;
    font-weight: var(--font-weight-bold);
    color: var(--color-text);
    line-height: 1;
  }

  .total-reviews {
    color: var(--color-text-secondary);
    font-size: var(--font-size-sm);
  }
}

.rating-distribution {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.distribution-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: var(--font-size-sm);

  .star-label {
    width: 50px;
    color: var(--color-text-secondary);
  }

  .bar-container {
    flex: 1;
    height: 8px;
    background: var(--color-border-light);
    border-radius: var(--radius-full);
    overflow: hidden;
  }

  .bar-fill {
    height: 100%;
    background: var(--color-primary);
    border-radius: var(--radius-full);
    transition: width var(--transition-normal);
  }

  .count-label {
    width: 30px;
    text-align: right;
    color: var(--color-text-secondary);
  }
}

.verified-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--color-success);
  font-size: var(--font-size-sm);

  i {
    font-size: var(--font-size-base);
  }
}

.btn-write-review {
  background: var(--color-text);
  border-color: var(--color-text);
  color: var(--color-surface);
  padding: 0.75rem 2rem;
  font-weight: var(--font-weight-medium);

  &:hover {
    background: var(--color-primary);
    border-color: var(--color-primary);
  }
}

.review-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-label {
  font-weight: var(--font-weight-medium);
  color: var(--color-text);
  font-size: var(--font-size-sm);
}

.rating-input {
  :deep(.p-rating-item) {
    .p-rating-icon {
      font-size: 1.5rem;

      &.p-rating-icon-active {
        color: var(--color-primary);
      }
    }
  }
}

.form-input,
.form-textarea,
.form-select {
  width: 100%;

  &:focus {
    border-color: var(--color-primary);
    box-shadow: 0 0 0 2px rgba(var(--color-primary-rgb), 0.1);
  }
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid var(--color-border-light);
}
</style>
