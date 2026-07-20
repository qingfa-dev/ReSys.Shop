<script setup lang="ts">
import { RouterLink } from 'vue-router'
import Button from 'primevue/button'

interface Props {
  badge?: string
  title: string
  subtitle?: string
  ctaPrimaryLabel?: string
  ctaPrimaryRoute?: string
  ctaSecondaryLabel?: string
  ctaSecondaryRoute?: string
}

withDefaults(defineProps<Props>(), {
  badge: '',
  title: 'Welcome',
  subtitle: '',
  ctaPrimaryLabel: 'Shop Now',
  ctaPrimaryRoute: '/shop',
  ctaSecondaryLabel: '',
  ctaSecondaryRoute: '/collections',
})
</script>

<template>
  <section class="hero">
    <div class="hero-content">
      <span v-if="badge" class="hero-badge animate-fadeIn">{{ badge }}</span>
      <h1 class="hero-title animate-slideUp" v-html="title.replace('<br/>', '<br>')"></h1>
      <p v-if="subtitle" class="hero-subtitle animate-slideUp" style="animation-delay: 100ms">
        {{ subtitle }}
      </p>
      <div class="hero-actions animate-slideUp" style="animation-delay: 200ms">
        <RouterLink :to="ctaPrimaryRoute">
          <Button :label="ctaPrimaryLabel" size="large" />
        </RouterLink>
        <RouterLink v-if="ctaSecondaryLabel" :to="ctaSecondaryRoute">
          <Button :label="ctaSecondaryLabel" severity="secondary" outlined size="large" />
        </RouterLink>
      </div>
    </div>
    <div class="hero-visual">
      <div class="hero-circle"></div>
      <div class="hero-image-placeholder">
        <i class="pi pi-image"></i>
      </div>
    </div>
  </section>
</template>

<style scoped lang="scss">
.hero {
  min-height: calc(100vh - 80px);
  display: grid;
  grid-template-columns: 1fr 1fr;
  align-items: center;
  gap: 4rem;
  max-width: 1400px;
  margin: 0 auto;
  padding: 4rem 2rem;

  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
    min-height: auto;
    text-align: center;
  }
}

.hero-content {
  max-width: 600px;

  @media (max-width: 1024px) {
    max-width: 100%;
    margin: 0 auto;
  }
}

.hero-badge {
  display: inline-block;
  padding: 0.5rem 1rem;
  background: var(--color-primary);
  color: white;
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-semibold);
  text-transform: uppercase;
  letter-spacing: 0.1em;
  border-radius: var(--radius-full);
  margin-bottom: 1.5rem;
}

.hero-title {
  font-size: clamp(2.5rem, 6vw, 4.5rem);
  line-height: 1.1;
  margin-bottom: 1.5rem;
  color: var(--color-text);
}

.hero-subtitle {
  font-size: var(--font-size-lg);
  color: var(--color-text-secondary);
  margin-bottom: 2rem;
  max-width: 480px;
}

.hero-actions {
  display: flex;
  gap: 1rem;

  @media (max-width: 1024px) {
    justify-content: center;
  }

  @media (max-width: 480px) {
    flex-direction: column;
  }
}

.hero-visual {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;

  @media (max-width: 1024px) {
    display: none;
  }
}

.hero-circle {
  width: 500px;
  height: 500px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--color-primary-light) 0%, var(--color-primary) 100%);
  opacity: 0.15;
  animation: pulse 4s ease-in-out infinite;
}

.hero-image-placeholder {
  position: absolute;
  width: 300px;
  height: 400px;
  background: var(--color-surface-elevated);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-xl);
  display: flex;
  align-items: center;
  justify-content: center;

  i {
    font-size: 4rem;
    color: var(--color-text-muted);
  }
}

@keyframes pulse {
  0%, 100% { transform: scale(1); opacity: 0.15; }
  50% { transform: scale(1.05); opacity: 0.2; }
}
</style>
