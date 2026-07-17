<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useToast } from '@/shared/composables/toast.use';
import { useFormatter } from '@/shared/composables/formatter.use';
import { userService } from '../services/user.service';
import type { AdminUserSummary } from '../types/User.Response.Type';
import { useI18n } from 'vue-i18n';
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue';
import UserRoleManager from '../components/UserRoleManager.Component.vue';
import UserPermissionManager from '../components/UserPermissionManager.Component.vue';
import UserSecurityManager from '../components/UserSecurityManager.Component.vue';

const route = useRoute();
const router = useRouter();
const { showToast } = useToast();
const { formatDate } = useFormatter();
const { t } = useI18n();

const userId = computed(() => route.params.id as string);
const user = ref<AdminUserSummary | null>(null);
const loading = ref(false);
const activeTab = ref(0);

const permissionList = ref<string[]>([]); 

onMounted(async () => {
    await loadData();
});

async function loadData() {
    loading.value = true;
    try {
        const res = await userService.getById(userId.value);
        if (res.isSuccess && res.value) {
            user.value = res.value;
            await loadPermissions();
        } else {
            showToast('error', t('common.error'), t('users.messages.load_error'));
            router.push({ name: 'admin-users' });
        }
    } finally {
        loading.value = false;
    }
}

async function loadPermissions() {
    const res = await userService.getUserPermissions(userId.value);
    if (res.isSuccess && res.value) {
        permissionList.value = res.value;
    }
}

function onEdit() {
    router.push({ name: 'admin-user-edit', params: { id: userId.value } });
}

async function onToggleStatus() {
    if (!user.value) return;
    const newStatus = !user.value.isActive;
    const res = await userService.updateAdminStatus(userId.value, newStatus);
    if (res.isSuccess) {
        user.value.isActive = newStatus;
        showToast('success', t('common.saved'), t('users.messages.status_updated', { status: newStatus ? 'active' : 'inactive' }));
    }
}
</script>

<template>
    <div class="p-6 max-w-6xl mx-auto">
        <AppBreadcrumb />
        
        <div v-if="user" class="flex flex-col md:flex-row md:items-center justify-between gap-4 mt-4 mb-8">
            <div class="flex items-center gap-4">
                <Button icon="pi pi-arrow-left" text rounded severity="secondary" @click="router.back()" class="bg-surface-100 dark:bg-surface-800" />
                <div class="flex flex-col">
                    <div class="flex items-center gap-3">
                        <h2 class="text-4xl font-black tracking-tighter text-surface-900 dark:text-surface-50 m-0">
                            {{ user.fullName || 'Staff Member' }}
                        </h2>
                        <Tag :value="user.isActive ? 'Active' : 'Inactive'" :severity="user.isActive ? 'success' : 'secondary'" rounded class="font-bold px-3" />
                    </div>
                    <p class="text-sm text-surface-500 m-0 font-mono">{{ user.email }}</p>
                </div>
            </div>
            <div class="flex items-center gap-3">
                <Button :label="user.isActive ? 'Deactivate' : 'Activate'" :severity="user.isActive ? 'danger' : 'success'" outlined icon="pi pi-power-off" @click="onToggleStatus" class="rounded-xl px-6" />
                <Button :label="t('users.actions.edit')" icon="pi pi-pencil" class="rounded-xl px-8 shadow-xl shadow-primary/20" @click="onEdit" />
            </div>
        </div>

        <Card class="border-none shadow-sm rounded-3xl bg-surface-0 dark:bg-surface-900 overflow-hidden" v-if="user">
            <template #content>
                <Tabs v-model:value="activeTab">
                    <TabList>
                        <Tab :value="0">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-user"></i>
                                <span>{{ t('users.tabs.details') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="1">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-shield"></i>
                                <span>{{ t('users.tabs.roles') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="2">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-key"></i>
                                <span>{{ t('users.tabs.permissions') }}</span>
                            </div>
                        </Tab>
                        <Tab :value="3">
                            <div class="flex items-center gap-2">
                                <i class="pi pi-lock"></i>
                                <span>{{ t('users.tabs.security') }}</span>
                            </div>
                        </Tab>
                    </TabList>

                    <TabPanels class="p-6">
                        <!-- Details Panel -->
                        <TabPanel :value="0">
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-12">
                                <div class="flex flex-col gap-6">
                                    <h3 class="text-lg font-bold uppercase tracking-wide text-surface-500 m-0">Basic Information</h3>
                                    <div class="flex flex-col gap-4">
                                        <div class="flex flex-col">
                                            <label class="text-xs text-surface-400 uppercase font-bold mb-1">{{ t('users.labels.username') }}</label>
                                            <span class="text-lg font-medium">{{ user.userName || '-' }}</span>
                                        </div>
                                        <div class="flex flex-col">
                                            <label class="text-xs text-surface-400 uppercase font-bold mb-1">First Name</label>
                                            <span class="text-lg font-medium">{{ user.firstName || '-' }}</span>
                                        </div>
                                        <div class="flex flex-col">
                                            <label class="text-xs text-surface-400 uppercase font-bold mb-1">Last Name</label>
                                            <span class="text-lg font-medium">{{ user.lastName || '-' }}</span>
                                        </div>
                                    </div>
                                </div>
                                <div class="flex flex-col gap-6">
                                    <h3 class="text-lg font-bold uppercase tracking-wide text-surface-500 m-0">Account Lifecycle</h3>
                                    <div class="flex flex-col gap-4">
                                        <div class="flex flex-col">
                                            <label class="text-xs text-surface-400 uppercase font-bold mb-1">Joined Date</label>
                                            <span class="text-lg font-medium">{{ formatDate(user.createdAtUtc) }}</span>
                                        </div>
                                        <div class="flex flex-col">
                                            <label class="text-xs text-surface-400 uppercase font-bold mb-1">Last Sign In</label>
                                            <span class="text-lg font-medium">{{ user.lastSignInAtUtc ? formatDate(user.lastSignInAtUtc) : 'Never' }}</span>
                                        </div>
                                        <div class="flex flex-col">
                                            <label class="text-xs text-surface-400 uppercase font-bold mb-1">Last Known IP</label>
                                            <span class="text-lg font-medium font-mono text-surface-600">{{ user.lastIpAddress || '-' }}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </TabPanel>

                        <!-- Roles Panel -->
                        <TabPanel :value="1">
                            <UserRoleManager :userId="user.id" :assignedRoles="user.roleNames" @updated="loadData" />
                        </TabPanel>

                        <!-- Permissions Panel -->
                        <TabPanel :value="2">
                            <UserPermissionManager :userId="user.id" :initialPermissions="permissionList" @updated="loadData" />
                        </TabPanel>

                        <!-- Security Panel -->
                        <TabPanel :value="3">
                            <UserSecurityManager :user="user" @updated="loadData" />
                        </TabPanel>
                    </TabPanels>
                </Tabs>
            </template>
        </Card>

        <div v-else-if="loading" class="flex flex-col items-center justify-center p-20">
            <ProgressSpinner />
            <p class="mt-4 text-surface-500">{{ t('users.messages.loading') }}</p>
        </div>
    </div>
</template>

<style scoped>
:deep(.p-tabs-list) {
    border-bottom: 1px solid var(--p-surface-100);
}
.dark :deep(.p-tabs-list) {
    border-bottom-color: var(--p-surface-800);
}
</style>
