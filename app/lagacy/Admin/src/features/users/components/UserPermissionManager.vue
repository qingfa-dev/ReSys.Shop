<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { permissionService } from '../services/permission.service';
import { userService } from '../services/user.service';
import type { PermissionSummary } from '../types/user.types';

const props = defineProps<{
    userId: string;
    initialPermissions: string[]; // These are names/keys
}>();

const emit = defineEmits(['updated']);

const { showToast } = useToast();
const loading = ref(false);
const saving = ref(false);

const selection = ref<[PermissionSummary[], PermissionSummary[]]>([[], []]);

onMounted(async () => {
    await loadPermissions();
});

async function loadPermissions() {
    loading.value = true;
    try {
        const res = await permissionService.list({ pageSize: 1000 });
        if (res.success && res.data) {
            const allPerms = res.data;
            
            selection.value[1] = allPerms.filter((p: PermissionSummary) => props.initialPermissions.includes(p.identifier));
            selection.value[0] = allPerms.filter((p: PermissionSummary) => !props.initialPermissions.includes(p.identifier));
        }
    } finally {
        loading.value = false;
    }
}

async function onSave() {
    saving.value = true;
    try {
        const currentNames = selection.value[1].map((p: PermissionSummary) => p.identifier);
        
        // 1. Find to add
        const toAdd = currentNames.filter((name: string) => !props.initialPermissions.includes(name));
        // 2. Find to remove
        const toRemove = props.initialPermissions.filter((name: string) => !currentNames.includes(name));

        // Note: We execute sequentially for now. Better to use Promise.all if backend handles it.
        for (const name of toAdd) {
            await userService.assignPermission(props.userId, name);
        }
        for (const name of toRemove) {
            await userService.unassignPermission(props.userId, name);
        }

        showToast('success', 'Success', 'Direct permissions updated');
        emit('updated');
    } finally {
        saving.value = false;
    }
}
</script>

<template>
    <div class="flex flex-col gap-6">
        <div class="flex items-center justify-between">
            <div>
                <h3 class="text-xl font-bold m-0">Direct Permissions</h3>
                <p class="text-sm text-surface-500 m-0">Assign specific overrides that apply regardless of assigned roles.</p>
            </div>
            <Button label="Save Changes" icon="pi pi-check" @click="onSave" :loading="saving" />
        </div>

        <div v-if="loading" class="flex justify-center p-12">
            <ProgressSpinner />
        </div>

        <PickList v-else v-model="selection" dataKey="identifier" breakpoint="1400px" 
                  :showSourceControls="false" :showTargetControls="false">
            <template #sourceheader> Available </template>
            <template #targetheader> Assigned </template>
            <template #item="slotProps">
                <div class="flex flex-col p-2">
                    <span class="font-bold text-xs">{{ slotProps.item.name }}</span>
                    <div class="flex items-center gap-2 mt-1">
                        <Tag :value="slotProps.item.action" class="text-[9px]" severity="secondary" />
                        <small class="text-[10px] text-surface-500 font-mono truncate">{{ slotProps.item.identifier }}</small>
                    </div>
                </div>
            </template>
        </PickList>
    </div>
</template>
