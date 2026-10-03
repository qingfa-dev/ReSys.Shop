<template>
  <div class="profile-card">
    <div class="profile-card__avatar">
      <img
        v-if="profile?.avatar"
        :src="profile.avatar"
        :alt="profile.displayName"
        class="profile-card__avatar-image"
      />
      <div v-else class="profile-card__avatar-placeholder">
        {{ initials }}
      </div>
    </div>
    <div class="profile-card__info">
      <h2 class="profile-card__name">{{ profile?.displayName }}</h2>
      <p class="profile-card__email">{{ profile?.email }}</p>
      <p v-if="profile?.phone" class="profile-card__phone">{{ profile.phone }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Profile } from '../types/entity'

const props = defineProps<{
  profile: Profile | null
}>()

const initials = computed(() => {
  if (!props.profile) return ''
  const first = props.profile.firstName?.charAt(0) ?? ''
  const last = props.profile.lastName?.charAt(0) ?? ''
  return `${first}${last}`.toUpperCase()
})
</script>

<style scoped>
.profile-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.5rem;
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 0.75rem;
}

.profile-card__avatar {
  flex-shrink: 0;
}

.profile-card__avatar-image {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  object-fit: cover;
}

.profile-card__avatar-placeholder {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: #6366f1;
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 1.25rem;
}

.profile-card__info {
  flex: 1;
  min-width: 0;
}

.profile-card__name {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
}

.profile-card__email {
  margin: 0.25rem 0 0;
  font-size: 0.875rem;
  color: #64748b;
}

.profile-card__phone {
  margin: 0.25rem 0 0;
  font-size: 0.875rem;
  color: #64748b;
}
</style>
