<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useToast } from '@/shared/composables/toast.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/PageShell.Component.vue'
import PageHeader from '@/shared/components/PageHeader.Component.vue'
import { userService } from '../services/user.service';
import { roleService } from '../roles/services/role.service';
import type { CreateAdminUserRequest, UpdateAdminUserRequest } from '../types/user.request.type';

const route = useRoute();
const router = useRouter();
const { showToast } = useToast();
const { t } = useI18n();

const isEditMode = computed(() => !!route.params.id);
const userId = computed(() => route.params.id as string);
const loading = ref(false);
const submitting = ref(false);
const roleOptions = ref<{ label: string, value: string }[]>([]);

const form = ref<CreateAdminUserRequest & UpdateAdminUserRequest>({
    email: '',
    firstName: '',
    lastName: '',
    role: [],
    isActive: true, // Only for update/display
    password: '' // Only for create
});

onMounted(async () => {
    loading.value = true;
    try {
        await fetchRoles();
        if (isEditMode.value) {
            await loadUser();
        }
    } finally {
        loading.value = false;
    }
});

async function fetchRoles() {
    const res = await roleService.list({ pageSize: 100 });
    if (res.isSuccess && res.items) {
        roleOptions.value = res.items.map(r => ({
            label: r.displayName || r.name,
            value: r.name
        }));
    }
}

async function loadUser() {
    const res = await userService.getById(userId.value);
    if (res.isSuccess && res.value) {
        const user = res.value;
        form.value = {
            email: user.email,
            firstName: user.firstName || '',
            lastName: user.lastName || '',
            role: [],
            isActive: user.isActive
        };
    } else {
        showToast('error', t('common.error'), t('users.messages.load_error'));
        router.push({ name: 'users.staff.list' });
    }
}

async function onSubmit() {
    submitting.value = true;
    try {
        if (isEditMode.value) {
            const updateData: UpdateAdminUserRequest = {
                firstName: form.value.firstName,
                lastName: form.value.lastName,
                role: form.value.role,
                isActive: form.value.isActive
            };
            const res = await userService.update(userId.value, updateData);
            if (res.isSuccess) {
                showToast('success', t('common.success'), t('users.messages.update_success'));
                router.push({ name: 'users.staff.list' });
            }
        } else {
            const createData: CreateAdminUserRequest = {
                email: form.value.email,
                firstName: form.value.firstName,
                lastName: form.value.lastName,
                role: form.value.role,
                password: form.value.password,
                isActive: true
            };
            const res = await userService.create(createData);
            if (res.isSuccess) {
                showToast('success', t('common.success'), t('users.messages.create_success'));
                router.push({ name: 'users.staff.list' });
            }
        }
    } finally {
        submitting.value = false;
    }
}
</script>

<template>
    <PageShell maxWidth="7xl">
        <PageHeader
            :title="isEditMode ? 'Edit Staff' : 'Invite Staff'"
            :description="isEditMode ? 'Update staff member details and permissions.' : 'Create a new staff account.'"
            back
        >
            <template #actions>
                <Button :label="t('common.cancel')" severity="secondary" text @click="router.back()" />
                <Button type="submit" form="staffForm" :label="isEditMode ? 'Save Changes' : 'Send Invitation'" :loading="submitting" icon="pi pi-check" />
            </template>
        </PageHeader>

        <div v-if="loading" class="flex justify-center p-12">
            <ProgressSpinner />
        </div>

        <form v-else id="staffForm" @submit.prevent="onSubmit" class="flex flex-col gap-6">
            <!-- Identity Info -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="field">
                    <label for="email" class="font-bold block mb-2">Email Address</label>
                    <InputText id="email" v-model="form.email" class="w-full" :disabled="isEditMode" required type="email" />
                    <small v-if="isEditMode" class="text-surface-500">Email cannot be changed.</small>
                </div>

                <div v-if="!isEditMode" class="field">
                    <label for="password" class="font-bold block mb-2">Initial Password</label>
                    <Password id="password" v-model="form.password" class="w-full" :feedback="true" toggleMask required />
                </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div class="field">
                    <label for="firstName" class="font-bold block mb-2">First Name</label>
                    <InputText id="firstName" v-model="form.firstName" class="w-full" required />
                </div>
                <div class="field">
                    <label for="lastName" class="font-bold block mb-2">Last Name</label>
                    <InputText id="lastName" v-model="form.lastName" class="w-full" required />
                </div>
            </div>

            <Divider />

            <!-- Roles & Permissions -->
            <div class="field">
                <label class="font-bold block mb-2">Assigned Roles</label>
                <div class="bg-surface-50 dark:bg-surface-900 p-4 rounded-xl border border-surface-200 dark:border-surface-700">
                    <div class="flex flex-wrap gap-3">
                        <div v-for="role in roleOptions" :key="role.value" class="flex align-items-center">
                            <Checkbox v-model="form.role" :inputId="role.value" :name="role.value" :value="role.value" />
                            <label :for="role.value" class="ml-2 cursor-pointer select-none">{{ role.label }}</label>
                        </div>
                    </div>
                </div>
                <small class="block mt-2 text-surface-500">Select roles to define user permissions.</small>
            </div>

            <!-- Status (Edit Only) -->
            <div v-if="isEditMode" class="field">
                <label class="font-bold block mb-2">Account Status</label>
                <div class="flex items-center gap-3">
                    <ToggleSwitch v-model="form.isActive" inputId="isActive" />
                    <label for="isActive" class="cursor-pointer">{{ form.isActive ? 'Active' : 'Inactive' }}</label>
                </div>
            </div>
        </form>
    </PageShell>
</template>
