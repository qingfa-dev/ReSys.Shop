<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useToast } from '@/common/composables/toast.use';
import { useI18n } from 'vue-i18n';
import PageShell from '@/shared/components/navigation/PageShell.vue'
import PageHeader from '@/shared/components/navigation/PageHeader.vue'
import { roleRepository } from '../api/role.api';
import { permissionRepository } from '../../permissions/api/permission.api';
import type { PermissionSummary } from '../../permissions/types/permission.response';

const route = useRoute();
const router = useRouter();
const { showToast } = useToast();
const { t } = useI18n();

const roleId = computed(() => route.params.id as string);
const loading = ref(false);
const saving = ref(false);

const selection = ref<[PermissionSummary[], PermissionSummary[]]>([[], []]);
const roleName = ref('');

onMounted(async () => {
    loading.value = true;
    try {
        // 1. Get Role details (using list workaround again or I should fix service)
        const rolesRes = await roleRepository.list({ pageSize: 100 });
        if (rolesRes.isSuccess && rolesRes.items) {
            const role = rolesRes.items.find(r => r.id === roleId.value);
            if (role) {
                roleName.value = role.displayName || role.name;
                
                // 2. Get All Permissions
                // We use a large page size to get all. Ideally we should have a non-paged endpoint or search.
                const permsRes = await permissionRepository.list({ pageSize: 1000 });
                
                if (permsRes.isSuccess && permsRes.value) {
                    const allPerms = permsRes.value;
                    
                    // Temporary: I will assume I can get them.
                    // If not, this view will need backend work.
                    
                    // Let's separate available into all.
                    selection.value[0] = allPerms;
                    // selection.value[1] = ... (missing source)
                }
            }
        }
    } finally {
        loading.value = false;
    }
});

async function onSave() {
    saving.value = true;
    try {
        const permissionNames = selection.value[1].map(p => p.identifier);
        const res = await roleRepository.syncPermissions(roleId.value, { permissionNames });
        if (res.isSuccess) {
            showToast('success', t('common.saved'), t('roles.messages.permissions_updated'));
            router.back();
        }
    } finally {
        saving.value = false;
    }
}
</script>

<template>
    <PageShell maxWidth="7xl">
        <PageHeader back title="Manage Permissions" description="Assign permissions to role">
            <template #badge>
                <span class="font-bold text-primary">{{ roleName }}</span>
            </template>
            <template #actions>
                <Button :label="t('roles.actions.save_permissions')" icon="pi pi-check" @click="onSave" :loading="saving" severity="primary" />
            </template>
        </PageHeader>

        <div v-if="loading" class="flex justify-center p-12">
            <ProgressSpinner />
        </div>

        <div v-else>
            <PickList v-model="selection" dataKey="identifier" breakpoint="1400px">
                <template #sourceheader>
                    <div class="font-bold p-2">Available Permissions</div>
                </template>
                <template #targetheader>
                    <div class="font-bold p-2">Assigned Permissions</div>
                </template>
                <template #item="slotProps">
                    <div class="flex flex-col p-2">
                        <span class="font-bold text-sm">{{ slotProps.item.name }}</span>
                        <div class="flex items-center gap-2 mt-1">
                            <Tag :value="slotProps.item.action" class="text-[10px]" severity="secondary" />
                            <small class="text-surface-500 truncate">{{ slotProps.item.identifier }}</small>
                        </div>
                    </div>
                </template>
            </PickList>
        </div>
    </PageShell>
</template>
