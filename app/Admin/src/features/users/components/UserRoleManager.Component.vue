<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { useI18n } from 'vue-i18n';
import { roleService } from '../roles/services/role.service';
import { userService } from '../services/user.service';
import type { RoleSummary } from '../roles/types/role.response.type';

const props = defineProps<{
    userId: string;
    assignedRoles: string[];
}>();

const emit = defineEmits(['updated']);

const { showToast } = useToast();
const { t } = useI18n();
const loading = ref(false);
const saving = ref(false);

const selection = ref<[RoleSummary[], RoleSummary[]]>([[], []]);

onMounted(async () => {
    await loadRoles();
});

async function loadRoles() {
    loading.value = true;
    try {
        const res = await roleService.list({ pageSize: 100 });
        if (res.isSuccess && res.items) {
            const allRoles = res.items;
            
            // Map names to objects
            selection.value[1] = allRoles.filter((r: RoleSummary) => props.assignedRoles.includes(r.name));
            selection.value[0] = allRoles.filter((r: RoleSummary) => !props.assignedRoles.includes(r.name));
        }
    } finally {
        loading.value = false;
    }
}

async function onSave() {
    saving.value = true;
    try {
        const roleNames = selection.value[1].map((r: RoleSummary) => r.name);
        const res = await userService.syncUserRoles(props.userId, { roleNames });
        if (res.isSuccess) {
            showToast('success', t('common.success'), t('users.messages.roles_updated'));
            emit('updated');
        }
    } finally {
        saving.value = false;
    }
}
</script>

<template>
    <div class="flex flex-col gap-6">
        <div class="flex items-center justify-between">
            <div>
                <h3 class="text-xl font-bold m-0">Role Assignment</h3>
                <p class="text-sm text-surface-500 m-0">Manage roles that define this user's primary permissions.</p>
            </div>
            <Button :label="t('users.actions.save_roles')" icon="pi pi-check" @click="onSave" :loading="saving" />
        </div>

        <div v-if="loading" class="flex justify-center p-12">
            <ProgressSpinner />
        </div>

        <PickList v-else v-model="selection" dataKey="id" breakpoint="1400px" 
                  :showSourceControls="false" :showTargetControls="false">
            <template #sourceheader> Available Roles </template>
            <template #targetheader> Assigned Roles </template>
            <template #item="slotProps">
                <div class="flex flex-col p-2">
                    <span class="font-bold text-sm">{{ slotProps.item.displayName || slotProps.item.name }}</span>
                    <small class="text-surface-500">{{ slotProps.item.name }}</small>
                </div>
            </template>
        </PickList>
    </div>
</template>
