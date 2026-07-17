<script setup lang="ts">
import { ref } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { useConfirm } from 'primevue/useconfirm';
import { useFormatter } from '@/shared/composables/formatter.use';
import { userService } from '../services/user.service';
import type { AdminUserSummary } from '../types/user.types';
import { userLocales as t } from '../locales/user.locales';

const props = defineProps<{
    user: AdminUserSummary;
}>();

const emit = defineEmits(['updated']);

const { showToast } = useToast();
const confirm = useConfirm();
const { formatDate } = useFormatter();

const loading = ref(false);

async function onResetPassword() {
    confirm.require({
        message: t.confirm.reset_password_message,
        header: t.confirm.reset_password_header,
        icon: 'pi pi-exclamation-triangle',
        acceptProps: { label: 'Reset', severity: 'danger' },
        accept: async () => {
            const newPassword = Math.random().toString(36).slice(-10); // Generate simple temp password
            const res = await userService.resetPassword(props.user.id, { new_password: newPassword });
            if (res.success) {
                confirm.require({
                    message: `Password has been reset to: ${newPassword}. Please provide this to the user.`,
                    header: 'New Password',
                    icon: 'pi pi-info-circle',
                    acceptLabel: 'Close',
                    rejectProps: { style: 'display: none' }
                });
            }
        }
    });
}

async function onUnlock() {
    loading.value = true;
    try {
        const res = await userService.unlockAccount(props.user.id);
        if (res.success) {
            showToast('success', 'Success', t.messages.unlock_success || 'Account unlocked');
            emit('updated');
        }
    } finally {
        loading.value = false;
    }
}

async function onVerify() {
    loading.value = true;
    try {
        const res = await userService.verifyAccount(props.user.id, { verifyEmail: true, verifyPhone: true });
        if (res.success) {
            showToast('success', 'Success', t.messages.verify_success || 'Account verified');
            emit('updated');
        }
    } finally {
        loading.value = false;
    }
}
</script>

<template>
    <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
        <!-- Status Section -->
        <div class="flex flex-col gap-6">
            <h3 class="text-xl font-bold m-0">{{ t.security.status_title }}</h3>
            
            <div class="bg-surface-50 dark:bg-surface-900 p-6 rounded-2xl border border-surface-100 dark:border-surface-800 flex flex-col gap-4">
                <div class="flex justify-between items-center pb-4 border-b border-surface-200 dark:border-surface-700">
                    <span class="text-surface-500 font-medium">{{ t.security.lockout_end }}</span>
                    <span class="font-bold" v-if="user.lockoutEnd">{{ formatDate(user.lockoutEnd) }}</span>
                    <Tag v-else value="None" severity="success" rounded />
                </div>
                
                <div class="flex justify-between items-center pb-4 border-b border-surface-200 dark:border-surface-700">
                    <span class="text-surface-500 font-medium">{{ t.security.failed_attempts }}</span>
                    <Badge :value="user.accessFailedCount || 0" :severity="(user.accessFailedCount || 0) > 0 ? 'warning' : 'secondary'" />
                </div>

                <div class="flex justify-between items-center pb-4 border-b border-surface-200 dark:border-surface-700">
                    <span class="text-surface-500 font-medium">{{ t.security.email_verified }}</span>
                    <Tag :value="user.emailConfirmed ? 'Verified' : 'Pending'" :severity="user.emailConfirmed ? 'success' : 'warning'" rounded />
                </div>

                <div class="flex justify-between items-center">
                    <span class="text-surface-500 font-medium">{{ t.security.phone_verified }}</span>
                    <Tag :value="user.phoneNumberConfirmed ? 'Verified' : 'Pending'" :severity="user.phoneNumberConfirmed ? 'success' : 'warning'" rounded />
                </div>
            </div>
        </div>

        <!-- Actions Section -->
        <div class="flex flex-col gap-6">
            <h3 class="text-xl font-bold m-0">{{ t.security.actions_title }}</h3>
            <div class="flex flex-col gap-3">
                <Button :label="t.actions.reset_password" icon="pi pi-key" severity="danger" outlined class="w-full justify-start rounded-xl" @click="onResetPassword" />
                <Button :label="t.actions.unlock" icon="pi pi-lock-open" severity="warning" outlined class="w-full justify-start rounded-xl" @click="onUnlock" :disabled="!user.lockoutEnd" />
                <Button :label="t.actions.verify" icon="pi pi-check-circle" severity="success" outlined class="w-full justify-start rounded-xl" @click="onVerify" :disabled="user.emailConfirmed && user.phoneNumberConfirmed" />
            </div>
        </div>
    </div>
</template>
