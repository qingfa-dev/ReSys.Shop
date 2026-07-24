<script setup lang="ts">
export interface Feature {
  icon: string
  title: string
  description: string
}

interface Props {
  features?: Feature[]
}

withDefaults(defineProps<Props>(), {
  features: () => [
    { icon: 'pi-truck', title: 'Free Shipping', description: 'On orders over $100' },
    { icon: 'pi-shield', title: 'Secure Payment', description: '100% secure checkout' },
    { icon: 'pi-refresh', title: 'Easy Returns', description: '30-day return policy' },
    { icon: 'pi-headphones', title: '24/7 Support', description: 'Dedicated support team' },
  ],
})
</script>

<template>
  <section class="features">
    <div class="features-container">
      <div v-for="feature in features" :key="feature.title" class="feature-card">
        <i :class="['pi', feature.icon]"></i>
        <h3>{{ feature.title }}</h3>
        <p>{{ feature.description }}</p>
      </div>
    </div>
  </section>
</template>

<style scoped lang="scss">
.features {
  background: var(--color-surface);
  border-top: 1px solid var(--color-border-light);
  border-bottom: 1px solid var(--color-border-light);
}

.features-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 3rem 2rem;
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 2rem;

  @media (max-width: 768px) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (max-width: 480px) {
    grid-template-columns: 1fr;
  }
}

.feature-card {
  text-align: center;
  padding: 1.5rem;

  i {
    font-size: 2rem;
    color: var(--color-primary);
    margin-bottom: 1rem;
  }

  h3 {
    font-family: var(--font-body);
    font-size: var(--font-size-base);
    font-weight: var(--font-weight-semibold);
    margin-bottom: 0.5rem;
  }

  p {
    font-size: var(--font-size-sm);
    color: var(--color-text-muted);
  }
}
</style>
