<script setup lang="ts">
import { ref, computed } from "vue";
import Galleria from "primevue/galleria";
import type { ProductImage } from "../../types";

interface Props {
  images: (string | ProductImage)[];
}

const props = defineProps<Props>();

const selectedIndex = ref(0);

const imageList = computed(() => {
  return props.images.map((img) => {
    if (typeof img === "string") {
      return { url: img, alt: "" };
    }
    return img;
  });
});
</script>

<template>
  <div class="product-gallery">
    <Galleria
      :value="imageList"
      :numVisible="4"
      :showThumbnails="true"
      :showIndicators="false"
      :showItemNavigators="true"
      :circular="false"
      containerClass="gallery-galleria"
      class="product-galleria"
    >
      <template #item="slotProps">
        <div class="gallery__main">
          <img
            v-if="slotProps.item.url"
            :src="slotProps.item.url"
            :alt="slotProps.item.alt || 'Product image'"
            class="gallery-main-image"
          />
          <div v-else class="no-image">No image available</div>
        </div>
      </template>
      <template #thumbnail="slotProps">
        <div class="thumbnail-wrapper">
          <img
            :src="slotProps.item.url"
            :alt="slotProps.item.alt || `Product thumbnail`"
            class="gallery-thumbnail-image"
          />
        </div>
      </template>
    </Galleria>
  </div>
</template>

<style scoped lang="scss">
.product-gallery {
  width: 100%;
}

.product-galleria {
  :deep(.p-galleria) {
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }

  :deep(.p-galleria-items-container) {
    width: 100%;
  }

  :deep(.p-galleria-items) {
    aspect-ratio: 3/4;
    border-radius: var(--radius-lg);
    overflow: hidden;
    background: var(--color-surface-ground);
  }

  :deep(.p-galleria-item) {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  :deep(.p-galleria-item-nav) {
    background: rgba(255, 255, 255, 0.9);
    color: var(--color-text);
    width: 2.5rem;
    height: 2.5rem;
    border-radius: 50%;
    margin: 0 0.5rem;

    &:hover {
      background: var(--color-surface);
    }
  }

  :deep(.p-galleria-thumbnails-container) {
    width: 100%;
    padding: 0;
  }

  :deep(.p-galleria-thumbnails) {
    padding: 0;
    gap: 0.5rem;
    width: 100%;
  }

  :deep(.p-galleria-thumbnail-items-container) {
    width: 100%;
  }

  :deep(.p-galleria-thumbnail-item) {
    width: 80px;
    height: 80px;
    aspect-ratio: 1;
    border-radius: var(--radius-md);
    overflow: hidden;
    border: 2px solid transparent;
    transition: border-color var(--transition-fast);
  }

  :deep(.p-galleria-thumbnail-item-container) {
    padding: 0;
    width: 80px;
    height: 80px;
  }

  :deep(.p-galleria-thumbnail-item:hover) {
    border-color: var(--color-text-muted);
  }

  :deep(.p-galleria-thumbnail-item-current) {
    border-color: var(--color-primary);
  }
}

.gallery__main {
  width: 100%;
  height: 100%;
  aspect-ratio: 3/4;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-surface-ground);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.gallery-main-image {
  width: auto;
  height: auto;
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}

.thumbnail-wrapper {
  width: 80px;
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-surface-ground);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.gallery-thumbnail-image {
  width: auto;
  height: auto;
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
}

.no-image {
  color: var(--color-text-muted);
  font-size: var(--font-size-lg);
}

// Legacy styles (keep for backward compatibility)
.gallery {
  &__main {
    width: 100%;
    aspect-ratio: 3/4;
    background: var(--color-surface-ground, #f5f5f5);
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--radius-lg);
    overflow: hidden;

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }

    .no-image {
      color: var(--color-text-muted);
    }
  }

  &__thumbnails {
    display: flex;
    gap: 0.5rem;
    overflow-x: auto;
  }
}

.thumbnail {
  width: 80px;
  height: 80px;
  border: 2px solid transparent;
  border-radius: var(--radius-md);
  padding: 0;
  cursor: pointer;
  background: var(--color-surface-ground, #f5f5f5);
  overflow: hidden;
  flex-shrink: 0;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  &.active {
    border-color: var(--color-primary);
  }

  &:hover {
    border-color: var(--color-text-muted);
  }
}
</style>
