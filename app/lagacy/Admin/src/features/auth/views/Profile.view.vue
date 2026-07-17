<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { authService, type ChangePasswordRequest } from '../services/auth.service';
import { useToast } from '@/shared/composables/toast.use';
import { useFormatter } from '@/shared/composables/formatter.use';

const { showToast } = useToast();
const { formatDate } = useFormatter();

const user = ref<any>(null);
const loading = ref(false);
const submitting = ref(false);

const passwordForm = ref<ChangePasswordRequest>({
    current_password: '',
    new_password: '',
    confirm_new_password: ''
});

const notifications = ref({
    email_notifications: true,
    order_updates: true,
    marketing: false,
    security_alerts: true
});

onMounted(async () => {
    loading.value = true;
    const result = await authService.getProfile();
    if (result.success && result.data) {
        user.value = result.data;
    } else {
        showToast('error', 'Error', 'Failed to load user profile');
    }
    loading.value = false;
});

const initials = computed(() => {
    if (!user.value?.fullName) return '?';
    return user.value.fullName
        .split(' ')
        .map((n: string) => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2);
});

async function onChangePassword() {
    if (passwordForm.value.new_password !== passwordForm.value.confirm_new_password) {
        showToast('error', 'Validation Error', 'New passwords do not match');
        return;
    }
    submitting.value = true;
    const result = await authService.changePassword(passwordForm.value);
    if (result.success) {
        showToast('success', 'Success', 'Password updated successfully');
        passwordForm.value = { current_password: '', new_password: '', confirm_new_password: '' };
    } else {
        const errMsg = result.error?.message || 'Failed to update password';
        showToast('error', 'Error', errMsg);
    }
    submitting.value = false;
}
</script>

<template>
    <div class="space-y-8">
        <div v-if="loading" class="flex justify-center py-20">
            <i class="pi pi-spin pi-spinner text-4xl text-primary"></i>
        </div>

        <template v-else-if="user">
            <!-- Avatar + Identity Card -->
            <Card>
                <template #content>
                    <div class="flex items-center gap-6">
                        <div class="w-20 h-20 rounded-full bg-primary text-primary-contrast flex items-center justify-center text-2xl font-bold shrink-0">
                            {{ initials }}
                        </div>
                        <div>
                            <h2 class="text-2xl font-bold text-surface-900 dark:text-surface-0">{{ user.fullName || 'User' }}</h2>
                            <p class="text-surface-500 dark:text-surface-400">{{ user.email }}</p>
                            <div class="flex flex-wrap gap-2 mt-2">
                                <Tag v-for="role in user.roleNames" :key="role" :value="role" severity="info" />
                            </div>
                        </div>
                    </div>
                </template>
            </Card>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
                <!-- Account Details -->
                <Card>
                    <template #title>
                        <div class="flex items-center gap-2">
                            <i class="pi pi-user text-primary"></i>
                            <span>Account Details</span>
                        </div>
                    </template>
                    <template #content>
                        <div class="flex flex-col gap-4">
                            <div>
                                <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Full Name</span>
                                <p class="text-lg font-medium">{{ user.fullName || 'N/A' }}</p>
                            </div>
                            <div>
                                <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Email Address</span>
                                <p class="text-lg font-medium font-mono">{{ user.email }}</p>
                            </div>
                            <div>
                                <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Username</span>
                                <p class="text-lg font-medium">{{ user.userName || 'N/A' }}</p>
                            </div>
                            <div>
                                <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Joined On</span>
                                <p class="text-lg font-medium">{{ formatDate(user.createdAtUtc) }}</p>
                            </div>
                        </div>
                    </template>
                </Card>

                <div class="flex flex-col gap-8">
                    <!-- Change Password -->
                    <Card>
                        <template #title>
                            <div class="flex items-center gap-2">
                                <i class="pi pi-lock text-primary"></i>
                                <span>Change Password</span>
                            </div>
                        </template>
                        <template #content>
                            <form @submit.prevent="onChangePassword" class="flex flex-col gap-4">
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-sm">Current Password</label>
                                    <Password v-model="passwordForm.current_password" toggleMask class="w-full" inputClass="w-full" required />
                                </div>
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-sm">New Password</label>
                                    <Password v-model="passwordForm.new_password" toggleMask class="w-full" inputClass="w-full" required />
                                </div>
                                <div class="flex flex-col gap-2">
                                    <label class="font-bold text-sm">Confirm New Password</label>
                                    <Password v-model="passwordForm.confirm_new_password" toggleMask class="w-full" inputClass="w-full" :feedback="false" required />
                                </div>
                                <Button type="submit" label="Update Password" icon="pi pi-check" class="mt-2" :loading="submitting" />
                            </form>
                        </template>
                    </Card>

                    <!-- Notification Preferences -->
                    <Card>
                        <template #title>
                            <div class="flex items-center gap-2">
                                <i class="pi pi-bell text-primary"></i>
                                <span>Notifications</span>
                            </div>
                        </template>
                        <template #content>
                            <div class="flex flex-col gap-4">
                                <div class="flex items-center justify-between">
                                    <div>
                                        <p class="font-medium text-surface-900 dark:text-surface-0">Email Notifications</p>
                                        <p class="text-sm text-surface-500">Receive email updates about your account</p>
                                    </div>
                                    <InputSwitch v-model="notifications.email_notifications" />
                                </div>
                                <div class="flex items-center justify-between">
                                    <div>
                                        <p class="font-medium text-surface-900 dark:text-surface-0">Order Updates</p>
                                        <p class="text-sm text-surface-500">Get notified about order status changes</p>
                                    </div>
                                    <InputSwitch v-model="notifications.order_updates" />
                                </div>
                                <div class="flex items-center justify-between">
                                    <div>
                                        <p class="font-medium text-surface-900 dark:text-surface-0">Marketing</p>
                                        <p class="text-sm text-surface-500">Receive promotional offers and news</p>
                                    </div>
                                    <InputSwitch v-model="notifications.marketing" />
                                </div>
                                <div class="flex items-center justify-between">
                                    <div>
                                        <p class="font-medium text-surface-900 dark:text-surface-0">Security Alerts</p>
                                        <p class="text-sm text-surface-500">Important security notifications about your account</p>
                                    </div>
                                    <InputSwitch v-model="notifications.security_alerts" />
                                </div>
                            </div>
                        </template>
                    </Card>
                </div>
            </div>
        </template>
    </div>
</template>
