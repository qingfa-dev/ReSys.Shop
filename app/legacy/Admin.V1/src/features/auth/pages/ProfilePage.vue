<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { authRepository } from '../api/auth.api';
import type { ChangePasswordParameters } from '../types/change-password.parameters';
import type { UserProfile } from '../types/login.response';
import { useToast } from '@/common/composables/toast.use';
import { useFormatter } from '@/common/composables/formatter.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/navigation/PageShell.vue';
import DetailField from '@/shared/components/data-display/DetailField.vue';
import FormField from '@/shared/components/form/FormField.vue';

const { t } = useI18n();
const { showToast } = useToast();
const { formatDate } = useFormatter();

interface ProfileViewFields {
  roleNames?: string[]
  userName?: string
  createdAtUtc?: string
}
const user = ref<Partial<UserProfile & ProfileViewFields> | null>(null);
const loading = ref(false);
const submitting = ref(false);

const passwordForm = ref<ChangePasswordParameters>({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
});

const notifications = ref({
    email_notifications: true,
    order_updates: true,
    marketing: false,
    security_alerts: true
});

onMounted(async () => {
    loading.value = true;
    const result = await authRepository.getProfile();
    if (result.isSuccess && result.value) {
        user.value = result.value;
    } else {
        showToast('error', t('common.error'), t('profile.messages.load_error'));
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
    if (passwordForm.value.newPassword !== passwordForm.value.confirmNewPassword) {
        showToast('error', t('common.error'), t('auth.messages.password_mismatch'));
        return;
    }
    submitting.value = true;
    const result = await authRepository.changePassword(passwordForm.value);
    if (result.isSuccess) {
        showToast('success', t('common.success'), t('profile.messages.password_updated'));
        passwordForm.value = { currentPassword: '', newPassword: '', confirmNewPassword: '' };
    } else {
        const errMsg = result.errors?.[0]?.message || 'Failed to update password';
        showToast('error', t('common.error'), errMsg);
    }
    submitting.value = false;
}
</script>

<template>
    <PageShell :card="false" gap maxWidth="7xl">
        <div v-if="loading" class="flex justify-center py-20">
            <ProgressSpinner style="width: 40px; height: 40px" />
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
                            {{ t('auth.labels.account_details') }}
                        </div>
                    </template>
                    <template #content>
                        <div class="flex flex-col gap-4">
                            <DetailField :label="t('profile.labels.full_name')" :value="user.fullName" />
                            <DetailField :label="t('profile.labels.email')" :value="user.email" />
                            <DetailField :label="t('profile.labels.username')" :value="user.userName" />
                            <DetailField :label="t('profile.labels.joined')" :value="formatDate(user.createdAtUtc)" />
                        </div>
                    </template>
                </Card>

                <div class="flex flex-col gap-8">
                    <!-- Change Password -->
                    <Card>
                        <template #title>
                            <div class="flex items-center gap-2">
                                <i class="pi pi-lock text-primary"></i>
                                {{ t('profile.titles.password') }}
                            </div>
                        </template>
                        <template #content>
                            <form @submit.prevent="onChangePassword" class="flex flex-col gap-4">
                                <FormField :label="t('auth.labels.current_password')" name="currentPassword">
                                    <Password v-model="passwordForm.currentPassword" toggleMask class="w-full" inputClass="w-full" required />
                                </FormField>
                                <FormField :label="t('auth.labels.new_password')" name="newPassword">
                                    <Password v-model="passwordForm.newPassword" toggleMask class="w-full" inputClass="w-full" required />
                                </FormField>
                                <FormField :label="t('auth.labels.confirm_password')" name="confirmNewPassword">
                                    <Password v-model="passwordForm.confirmNewPassword" toggleMask class="w-full" inputClass="w-full" :feedback="false" required />
                                </FormField>
                                <Button type="submit" :label="t('auth.actions.update_password')" icon="pi pi-check" class="mt-2" :loading="submitting" />
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
                                    <ToggleSwitch v-model="notifications.email_notifications" />
                                </div>
                                <div class="flex items-center justify-between">
                                    <div>
                                        <p class="font-medium text-surface-900 dark:text-surface-0">Order Updates</p>
                                        <p class="text-sm text-surface-500">Get notified about order status changes</p>
                                    </div>
                                    <ToggleSwitch v-model="notifications.order_updates" />
                                </div>
                                <div class="flex items-center justify-between">
                                    <div>
                                        <p class="font-medium text-surface-900 dark:text-surface-0">Marketing</p>
                                        <p class="text-sm text-surface-500">Receive promotional offers and news</p>
                                    </div>
                                    <ToggleSwitch v-model="notifications.marketing" />
                                </div>
                                <div class="flex items-center justify-between">
                                    <div>
                                        <p class="font-medium text-surface-900 dark:text-surface-0">Security Alerts</p>
                                        <p class="text-sm text-surface-500">Important security notifications about your account</p>
                                    </div>
                                    <ToggleSwitch v-model="notifications.security_alerts" />
                                </div>
                            </div>
                        </template>
                    </Card>
                </div>
            </div>
        </template>
    </PageShell>
</template>
