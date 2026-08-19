<template>
  <div class="review-list">
    <div v-if="displayReviews.length === 0" class="no-reviews">
      <p>No reviews yet. Be the first to review!</p>
    </div>

    <div v-else class="reviews">
      <div v-for="review in displayReviews" :key="review.id" class="review-item">
        <div class="review-header">
          <div class="reviewer-info">
            <span class="reviewer-name">{{ review.userName }}</span>
            <span v-if="review.verified" class="badge-verified">✓ Verified Purchase</span>
          </div>
          <span class="review-date">{{ formatDate(review.createdAt) }}</span>
        </div>

        <div class="rating-stars">
          <span v-for="i in 5" :key="i" class="star" :class="{ filled: i <= review.rating }"
            >★</span
          >
        </div>

        <h4 class="review-title">{{ review.title }}</h4>
        <p class="review-body">{{ review.body }}</p>

        <div v-if="review.fit" class="fit-info">
          <span class="fit-label">Fit:</span>
          <span class="fit-value" :class="review.fit">
            {{ review.fit === 'true-to-size' ? 'True to Size' : review.fit === 'runs-small' ? 'Runs Small' : 'Runs Large' }}
          </span>
        </div>

        <div v-if="review.images && review.images.length" class="review-images">
          <img
            v-for="(img, idx) in review.images"
            :key="idx"
            :src="img"
            :alt="'Review image ' + (idx + 1)"
          />
        </div>

        <div class="review-footer">
          <button class="helpful-btn">👍 Helpful ({{ review.helpful }})</button>
          <button class="unhelpful-btn">👎 ({{ review.unhelpful }})</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useReviews } from "../composables/useReviews";
import type { Review } from "../types";

const props = defineProps<{
  reviews?: Review[];
}>();

const { filteredReviews: storeReviews } = useReviews();

const displayReviews = computed(() => {
  return props.reviews ?? storeReviews;
});

function formatDate(date: string): string {
  return new Date(date).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}
</script>

<style scoped lang="scss">
.review-list {
  .no-reviews {
    text-align: center;
    padding: 2rem;
    color: var(--color-text-secondary);
  }

  .reviews {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
  }

  .review-item {
    border: 1px solid var(--color-border-light);
    border-radius: var(--radius-md);
    padding: 1.5rem;
    background: var(--color-surface);

    .review-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.75rem;

      .reviewer-info {
        display: flex;
        align-items: center;
        gap: 0.5rem;

        .reviewer-name {
          font-weight: var(--font-weight-medium);
        }

        .badge-verified {
          font-size: 0.75rem;
          background: #e8f5e9;
          color: #2e7d32;
          padding: 0.25rem 0.5rem;
          border-radius: var(--radius-sm);
        }
      }

      .review-date {
        font-size: var(--font-size-sm);
        color: var(--color-text-secondary);
      }
    }

    .rating-stars {
      display: flex;
      gap: 0.25rem;
      margin-bottom: 0.75rem;

      .star {
        font-size: 1.25rem;
        color: #ddd;

        &.filled {
          color: #ffc107;
        }
      }
    }

    .review-title {
      font-size: var(--font-size-base);
      font-weight: var(--font-weight-medium);
      margin-bottom: 0.5rem;
    }

    .review-body {
      color: var(--color-text-secondary);
      line-height: 1.6;
      margin-bottom: 0.75rem;
    }

    .fit-info {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin-bottom: 0.75rem;
      font-size: var(--font-size-sm);

      .fit-label {
        color: var(--color-text-secondary);
      }

      .fit-value {
        padding: 0.25rem 0.5rem;
        border-radius: var(--radius-sm);
        font-weight: var(--font-weight-medium);

        &.true-to-size {
          background: #e8f5e9;
          color: #2e7d32;
        }

        &.runs-small {
          background: #fff3e0;
          color: #ef6c00;
        }

        &.runs-large {
          background: #e3f2fd;
          color: #1565c0;
        }
      }
    }

    .review-images {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 0.75rem;

      img {
        width: 80px;
        height: 80px;
        object-fit: cover;
        border-radius: var(--radius-md);
      }
    }

    .review-footer {
      display: flex;
      gap: 1rem;

      button {
        background: none;
        border: 1px solid var(--color-border);
        padding: 0.5rem 1rem;
        border-radius: var(--radius-md);
        cursor: pointer;
        font-size: var(--font-size-sm);
        transition: all var(--transition-fast);

        &:hover {
          border-color: var(--color-primary);
          color: var(--color-primary);
        }
      }
    }
  }
}
</style>
