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

onMounted(async () => {
    loading.value = true;
    try {
        const res = await authService.getProfile();
        if (res.success && res.data) {
            user.value = res.data;
        }
    } finally {
        loading.value = false;
    }
});

async function onChangePassword() {
    if (passwordForm.value.new_password !== passwordForm.value.confirm_new_password) {
        showToast('error', 'Error', 'New passwords do not match');
        return;
    }

    submitting.value = true;
    try {
        const res = await authService.changePassword(passwordForm.value);
        if (res.success) {
            showToast('success', 'Success', 'Password changed successfully');
            passwordForm.value = {
                current_password: '',
                new_password: '',
                confirm_new_password: ''
            };
        }
    } finally {
        submitting.value = false;
    }
}
</script>

<template>
    <div class="p-6 max-w-4xl mx-auto">
        <div class="mb-8">
            <h1 class="text-3xl font-black uppercase tracking-tighter text-surface-900 dark:text-surface-0">My Profile</h1>
            <p class="text-surface-500">Manage your account settings and security.</p>
        </div>

        <div v-if="loading" class="flex justify-center p-20">
            <ProgressSpinner />
        </div>

        <div v-else-if="user" class="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <!-- Profile Info -->
            <div class="flex flex-col gap-6">
                <div class="bg-surface-0 dark:bg-surface-900 p-6 rounded-3xl border border-surface-100 dark:border-surface-800 shadow-sm">
                    <h3 class="text-lg font-bold mb-6 flex items-center gap-2">
                        <i class="pi pi-user text-primary"></i>
                        Account Details
                    </h3>
                    
                    <div class="flex flex-col gap-4">
                        <div class="flex flex-col">
                            <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Full Name</span>
                            <span class="text-lg font-medium">{{ user.full_name || 'N/A' }}</span>
                        </div>
                        <div class="flex flex-col">
                            <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Email Address</span>
                            <span class="text-lg font-medium font-mono">{{ user.email }}</span>
                        </div>
                        <div class="flex flex-col">
                            <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Username</span>
                            <span class="text-lg font-medium">{{ user.user_name || 'N/A' }}</span>
                        </div>
                        <div class="flex flex-col">
                            <span class="text-xs font-bold uppercase tracking-widest text-surface-400">Joined On</span>
                            <span class="text-lg font-medium">{{ formatDate(user.created_at) }}</span>
                        </div>
                    </div>
                </div>

                <div class="bg-surface-0 dark:bg-surface-900 p-6 rounded-3xl border border-surface-100 dark:border-surface-800 shadow-sm">
                    <h3 class="text-lg font-bold mb-6 flex items-center gap-2">
                        <i class="pi pi-shield text-primary"></i>
                        Roles & Permissions
                    </h3>
                    
                    <div class="flex flex-wrap gap-2 mb-4">
                        <Tag v-for="role in user.role_names" :key="role" :value="role" severity="info" class="font-bold" />
                    </div>
                    <p v-if="!user.role_names?.length" class="text-surface-500 italic text-sm">No roles assigned.</p>
                </div>
            </div>

            <!-- Password Change -->
            <div class="flex flex-col gap-6">
                <div class="bg-surface-0 dark:bg-surface-900 p-6 rounded-3xl border border-surface-100 dark:border-surface-800 shadow-sm">
                    <h3 class="text-lg font-bold mb-6 flex items-center gap-2">
                        <i class="pi pi-lock text-primary"></i>
                        Change Password
                    </h3>

                    <form @submit.prevent="onChangePassword" class="flex flex-col gap-4">
                        <div class="flex flex-col gap-2">
                            <label class="font-bold text-sm">Current Password</label>
                            <Password v-model="passwordForm.current_password" toggleMask class="w-full" inputClass="w-full rounded-xl" required />
                        </div>
                        
                        <div class="flex flex-col gap-2">
                            <label class="font-bold text-sm">New Password</label>
                            <Password v-model="passwordForm.new_password" toggleMask class="w-full" inputClass="w-full rounded-xl" required />
                        </div>

                        <div class="flex flex-col gap-2">
                            <label class="font-bold text-sm">Confirm New Password</label>
                            <Password v-model="passwordForm.confirm_new_password" toggleMask class="w-full" inputClass="w-full rounded-xl" required :feedback="false" />
                        </div>

                        <Button type="submit" label="Update Password" icon="pi pi-check" class="mt-4 rounded-xl" :loading="submitting" />
                    </form>
                </div>
            </div>
        </div>
    </div>
</template>
