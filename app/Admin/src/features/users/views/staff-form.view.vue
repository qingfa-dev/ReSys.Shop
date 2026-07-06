<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useToast } from '@/shared/composables/toast.use';
import { userService } from '../services/user.service';
import { roleService } from '../services/role.service';
import type { CreateAdminUserRequest, UpdateAdminUserRequest } from '../types/user.types';

const route = useRoute();
const router = useRouter();
const { showToast } = useToast();

const isEditMode = computed(() => !!route.params.id);
const userId = computed(() => route.params.id as string);
const loading = ref(false);
const submitting = ref(false);
const roleOptions = ref<{ label: string, value: string }[]>([]);

const form = ref<CreateAdminUserRequest & UpdateAdminUserRequest>({
    email: '',
    first_name: '',
    last_name: '',
    role: [],
    is_active: true, // Only for update/display
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
    const res = await roleService.listRoles({ page_size: 100 });
    if (res.success && res.data) {
        roleOptions.value = res.data.items.map(r => ({
            label: r.display_name || r.name,
            value: r.name
        }));
    }
}

async function loadUser() {
    const res = await userService.getAdminDetail(userId.value);
    if (res.success && res.data) {
        const user = res.data;
        form.value = {
            email: user.email,
            first_name: user.first_name || '',
            last_name: user.last_name || '',
            role: user.role_names || [],
            is_active: user.is_active
        };
    } else {
        showToast('error', 'Error', 'Failed to load user details');
        router.push({ name: 'admin-users' });
    }
}

async function onSubmit() {
    submitting.value = true;
    try {
        if (isEditMode.value) {
            const updateData: UpdateAdminUserRequest = {
                first_name: form.value.first_name,
                last_name: form.value.last_name,
                role: form.value.role,
                is_active: form.value.is_active
            };
            const res = await userService.updateAdmin(userId.value, updateData);
            if (res.success) {
                showToast('success', 'Success', 'Staff member updated successfully');
                router.push({ name: 'admin-users' });
            }
        } else {
            const createData: CreateAdminUserRequest = {
                email: form.value.email,
                first_name: form.value.first_name,
                last_name: form.value.last_name,
                role: form.value.role,
                password: form.value.password
            };
            const res = await userService.createAdmin(createData);
            if (res.success) {
                showToast('success', 'Success', 'Staff member invited successfully');
                router.push({ name: 'admin-users' });
            }
        }
    } finally {
        submitting.value = false;
    }
}
</script>

<template>
    <Card>
        <div class="flex items-center gap-4 mb-8">
            <Button icon="pi pi-arrow-left" text rounded @click="router.back()" />
            <div>
                <h1 class="text-3xl font-black uppercase tracking-tighter text-surface-900 dark:text-surface-0">
                    {{ isEditMode ? 'Edit Staff' : 'Invite Staff' }}
                </h1>
                <p class="text-surface-500">
                    {{ isEditMode ? 'Update staff member details and permissions.' : 'Create a new staff account.' }}
                </p>
            </div>
        </div>

        <div v-if="loading" class="flex justify-center p-12">
            <ProgressSpinner />
        </div>

        <form v-else @submit.prevent="onSubmit" class="flex flex-col gap-6">
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
                    <label for="first_name" class="font-bold block mb-2">First Name</label>
                    <InputText id="first_name" v-model="form.first_name" class="w-full" required />
                </div>
                <div class="field">
                    <label for="last_name" class="font-bold block mb-2">Last Name</label>
                    <InputText id="last_name" v-model="form.last_name" class="w-full" required />
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
                    <InputSwitch v-model="form.is_active" inputId="is_active" />
                    <label for="is_active" class="cursor-pointer">{{ form.is_active ? 'Active' : 'Inactive' }}</label>
                </div>
            </div>

            <div class="flex justify-end gap-3 mt-4">
                <Button label="Cancel" severity="secondary" text @click="router.back()" />
                <Button type="submit" :label="isEditMode ? 'Save Changes' : 'Send Invitation'" :loading="submitting" icon="pi pi-check" />
            </div>
        </form>
    </template>
</Card>
</template>