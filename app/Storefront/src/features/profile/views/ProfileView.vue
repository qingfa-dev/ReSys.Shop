<template>
  <div class="profile-view">
    <h1 class="profile-view__title">My Profile</h1>
    <div v-if="isLoading" class="profile-view__loading">Loading profile...</div>
    <div v-else-if="error" class="profile-view__error">{{ error }}</div>
    <template v-else-if="profile">
      <ProfileCard :profile="profile" />
      <div class="profile-view__details">
        <div class="profile-view__field">
          <label>Email</label>
          <span>{{ profile.email }}</span>
        </div>
        <div class="profile-view__field">
          <label>Display Name</label>
          <span>{{ profile.displayName }}</span>
        </div>
        <div class="profile-view__field">
          <label>Phone</label>
          <span>{{ profile.phone || '-' }}</span>
        </div>
        <div class="profile-view__field">
          <label>Date of Birth</label>
          <span>{{ profile.dateOfBirth || '-' }}</span>
        </div>
        <div class="profile-view__field">
          <label>Gender</label>
          <span>{{ profile.gender || '-' }}</span>
        </div>
      </div>
    </template>
    <div v-else class="profile-view__empty">No profile data available.</div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import ProfileCard from '../components/ProfileCard.vue'
import { useProfile } from '../composables/useProfile'

const { profile, isLoading, error, fetchProfile } = useProfile()

const userId = 'profile-1'

onMounted(() => {
  fetchProfile(userId)
})
</script>

<style scoped>
.profile-view {
  max-width: 600px;
  margin: 0 auto;
  padding: 2rem 1rem;
}

.profile-view__title {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 1.5rem;
}

.profile-view__loading,
.profile-view__error,
.profile-view__empty {
  padding: 2rem;
  text-align: center;
  color: #64748b;
}

.profile-view__error {
  color: #ef4444;
}

.profile-view__details {
  margin-top: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.profile-view__field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.profile-view__field label {
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #94a3b8;
}

.profile-view__field span {
  font-size: 0.875rem;
  color: #334155;
}
</style>
