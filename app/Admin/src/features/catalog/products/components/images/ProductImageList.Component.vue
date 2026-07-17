<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { ProductImage } from '../../types/Product.Response.Type';

const { t } = useI18n();

const props = defineProps<{
    images: ProductImage[];
}>();

const emit = defineEmits(['update-image', 'delete']);

const viewMode = ref<'grid' | 'list'>('grid');
const editingImage = ref<ProductImage | null>(null);
const editForm = ref({ role: 3, alt: '' });

const viewOptions = [
    { icon: 'pi pi-th-large', value: 'grid' },
    { icon: 'pi pi-list', value: 'list' }
];

const roles = [
    { label: t('catalog.products.images.roles.primary'), value: 0, desc: t('catalog.products.images.roles.desc_primary') },
    { label: t('catalog.products.images.roles.thumbnail'), value: 1, desc: t('catalog.products.images.roles.desc_thumbnail') },
    { label: t('catalog.products.images.roles.square'), value: 2, desc: t('catalog.products.images.roles.desc_square') },
    { label: t('catalog.products.images.roles.gallery'), value: 3, desc: t('catalog.products.images.roles.desc_gallery') },
    { label: t('catalog.products.images.roles.search'), value: 4, desc: t('catalog.products.images.roles.desc_search') }
];

const getRoleLabel = (roleVal: any) => {
    if (typeof roleVal === 'string') return roleVal;
    const r = roles.find(x => x.value === roleVal);
    return r ? r.label : t('catalog.products.images.roles.gallery');
};

const openEdit = (image: ProductImage) => {
    editingImage.value = image;
    let rVal = image.role as unknown as number;
    if (typeof image.role === 'string') {
        const map: Record<string, number> = { 'Default': 0, 'Thumbnail': 1, 'Square': 2, 'Gallery': 3, 'Search': 4 };
        rVal = map[image.role] ?? 3;
    }
    
    editForm.value = {
        role: rVal,
        alt: image.alt || ''
    };
};

const saveEdit = () => {
    if (editingImage.value) {
        emit('update-image', { 
            id: editingImage.value.id, 
            role: editForm.value.role,
            alt: editForm.value.alt 
        });
        editingImage.value = null;
    }
};
</script>

<template>
    <div class="flex flex-col gap-4">
        <div class="flex justify-end">
            <SelectButton v-model="viewMode" :options="viewOptions" optionValue="value" :allowEmpty="false">
                <template #option="{ option }">
                    <i :class="option.icon"></i>
                </template>
            </SelectButton>
        </div>

        <!-- Grid View -->
        <div v-if="viewMode === 'grid'" class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            <div v-for="image in images" :key="image.id" class="group relative rounded-3xl overflow-hidden border border-surface-100 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 shadow-sm transition-transform hover:scale-[1.02]">
                <div class="aspect-square relative">
                    <Image :src="image.url" :alt="image.alt || ''" preview imageClass="w-full h-full object-cover" />
                </div>
                
                <div v-if="image.role === 0" class="absolute top-3 left-3 px-3 py-1 bg-primary text-white text-[10px] font-black uppercase tracking-widest rounded-full shadow-lg z-10 pointer-events-none">
                    {{ t('catalog.products.images.roles.primary') }}
                </div>
                <div v-else-if="image.role === 4" class="absolute top-3 left-3 px-3 py-1 bg-blue-500 text-white text-[10px] font-black uppercase tracking-widest rounded-full shadow-lg z-10 pointer-events-none">
                    {{ t('catalog.products.images.roles.search') }}
                </div>
                <div v-else-if="image.role !== 3" class="absolute top-3 left-3 px-3 py-1 bg-surface-900 text-white text-[10px] font-black uppercase tracking-widest rounded-full shadow-lg z-10 pointer-events-none">
                    {{ getRoleLabel(image.role) }}
                </div>

                <div class="absolute top-3 right-3 flex flex-col gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                    <Button icon="pi pi-pencil" severity="secondary" rounded size="small" @click="openEdit(image)" class="shadow-md bg-white/90 dark:bg-black/90 border-none" />
                    <Button icon="pi pi-trash" severity="danger" rounded size="small" @click="$emit('delete', image.id)" class="shadow-md" />
                </div>
            </div>
        </div>

        <!-- List View -->
        <div v-else class="border border-surface-200 dark:border-surface-700 rounded-xl overflow-hidden">
            <DataTable :value="images" stripedRows size="small">
                <Column header="Preview" class="w-24">
                    <template #body="{ data }">
                        <Image :src="data.url" preview imageClass="w-12 h-12 rounded object-cover border" />
                    </template>
                </Column>
                <Column field="role" :header="t('catalog.products.images.role_label')">
                    <template #body="{ data }">
                        <Tag v-if="data.role === 0" :value="t('catalog.products.images.roles.primary')" severity="primary" />
                        <Tag v-else-if="data.role === 4" :value="t('catalog.products.images.roles.search')" severity="info" />
                        <Tag v-else-if="data.role === 3" :value="t('catalog.products.images.roles.gallery')" severity="secondary" />
                        <Tag v-else :value="getRoleLabel(data.role)" severity="warning" />
                    </template>
                </Column>
                <Column field="alt" :header="t('catalog.products.images.alt_text')">
                    <template #body="{ data }">
                        <span v-if="data.alt">{{ data.alt }}</span>
                        <span v-else class="text-surface-400 italic">No alt text</span>
                    </template>
                </Column>
                <Column header="Actions" class="w-32 text-right">
                    <template #body="{ data }">
                        <div class="flex justify-end gap-2">
                            <Button icon="pi pi-pencil" text rounded severity="secondary" size="small" @click="openEdit(data)" />
                            <Button icon="pi pi-trash" text rounded severity="danger" size="small" @click="$emit('delete', data.id)" />
                        </div>
                    </template>
                </Column>
            </DataTable>
        </div>

        <!-- Edit Dialog -->
        <Dialog :visible="!!editingImage" :header="t('catalog.products.images.edit_title')" modal :style="{ width: '400px' }" @update:visible="val => !val && (editingImage = null)">
            <div class="flex flex-col gap-6" v-if="editingImage">
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-sm">{{ t('catalog.products.images.role_label') }}</label>
                    <SelectButton v-model="editForm.role" :options="roles" optionLabel="label" optionValue="value" :allowEmpty="false" />
                    <small class="text-surface-500">{{ roles.find(r => r.value === editForm.role)?.desc }}</small>
                </div>
                
                <div class="flex flex-col gap-2">
                    <label class="font-bold text-sm">{{ t('catalog.products.images.alt_text') }}</label>
                    <InputText v-model="editForm.alt" :placeholder="t('catalog.products.images.alt_placeholder')" />
                </div>
            </div>
            <template #footer>
                <Button :label="t('catalog.products.actions.cancel')" text severity="secondary" @click="editingImage = null" />
                <Button :label="t('catalog.products.actions.save')" icon="pi pi-check" @click="saveEdit" />
            </template>
        </Dialog>
    </div>
</template>
