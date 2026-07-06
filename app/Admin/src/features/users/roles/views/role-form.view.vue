<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useToast } from '@/shared/composables/toast.use';
import { roleService } from '../../services/role.service';
import type { CreateRoleRequest, UpdateRoleRequest } from '../../types/user.types';

const route = useRoute();
const router = useRouter();
const { showToast } = useToast();

const isEditMode = computed(() => !!route.params.id);
const roleId = computed(() => route.params.id as string);
const loading = ref(false);
const submitting = ref(false);

const form = ref<CreateRoleRequest & UpdateRoleRequest>({
    name: '',
    display_name: '',
    description: '',
    priority: 0
});

const isSystemRole = ref(false);

onMounted(async () => {
    if (isEditMode.value) {
        loading.value = true;
        try {
            await loadRole();
        } finally {
            loading.value = false;
        }
    }
});

async function loadRole() {
    const res = await roleService.getRole(roleId.value);
    if (res.success && res.data) {
        const role = res.data;
        form.value = {
            name: role.name,
            display_name: role.display_name || '',
            description: role.description || '',
            priority: role.priority
        };
        isSystemRole.value = role.is_system_role;
    } else {
        showToast('error', 'Error', 'Failed to load role details');
        router.push({ name: 'roles-list' });
    }
}

async function onSubmit() {
    submitting.value = true;
    try {
        if (isEditMode.value) {
            const updateData: UpdateRoleRequest = {
                display_name: form.value.display_name,
                description: form.value.description,
                priority: form.value.priority
            };
            const res = await roleService.updateRole(roleId.value, updateData);
            if (res.success) {
                showToast('success', 'Success', 'Role updated successfully');
                router.push({ name: 'roles-list' });
            }
        } else {
            const createData: CreateRoleRequest = {
                name: form.value.name,
                display_name: form.value.display_name,
                description: form.value.description,
                priority: form.value.priority
            };
            const res = await roleService.createRole(createData);
            if (res.success) {
                showToast('success', 'Success', 'Role created successfully');
                router.push({ name: 'roles-list' });
            }
        }
    } finally {
        submitting.value = false;
    }
}
</script>

<template>
    <div class="card p-6 max-w-2xl mx-auto">
        <div class="flex items-center gap-4 mb-8">
            <Button icon="pi pi-arrow-left" text rounded @click="router.back()" />
            <div>
                <h1 class="text-3xl font-black uppercase tracking-tighter text-surface-900 dark:text-surface-0">
                    {{ isEditMode ? 'Edit Role' : 'Create Role' }}
                </h1>
                <p class="text-surface-500">
                    {{ isEditMode ? 'Update role details.' : 'Define a new system role.' }}
                </p>
            </div>
        </div>

        <div v-if="loading" class="flex justify-center p-12">
            <ProgressSpinner />
        </div>

        <form v-else @submit.prevent="onSubmit" class="flex flex-col gap-6">
            <div class="field">
                <label for="name" class="font-bold block mb-2">Role Name (Key)</label>
                <InputText id="name" v-model="form.name" class="w-full" :disabled="isEditMode || isSystemRole" required />
                <small class="text-surface-500">Unique identifier for the role (e.g. "Storefront.Manager").</small>
            </div>

            <div class="field">
                <label for="display_name" class="font-bold block mb-2">Display Name</label>
                <InputText id="display_name" v-model="form.display_name" class="w-full" required />
            </div>

            <div class="field">
                <label for="description" class="font-bold block mb-2">Description</label>
                <Textarea id="description" v-model="form.description" class="w-full" rows="3" />
            </div>

            <div class="field">
                <label for="priority" class="font-bold block mb-2">Priority</label>
                <InputNumber id="priority" v-model="form.priority" class="w-full" :min="0" showButtons />
                <small class="text-surface-500">Higher priority roles override lower ones in some contexts.</small>
            </div>

            <div v-if="isSystemRole" class="bg-yellow-50 dark:bg-yellow-900/20 p-4 rounded-xl border border-yellow-200 dark:border-yellow-800 flex items-start gap-3">
                <i class="pi pi-lock text-yellow-600 dark:text-yellow-400 mt-1"></i>
                <div class="text-sm">
                    <span class="font-bold text-yellow-800 dark:text-yellow-200 block">System Role</span>
                    <span class="text-yellow-700 dark:text-yellow-300">This is a built-in system role. Some properties cannot be modified.</span>
                </div>
            </div>

            <div class="flex justify-end gap-3 mt-4">
                <Button label="Cancel" severity="secondary" text @click="router.back()" />
                <Button type="submit" :label="isEditMode ? 'Save Changes' : 'Create Role'" :loading="submitting" icon="pi pi-check" />
            </div>
        </form>
    </div>
</template>