<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth.store';
import { storeToRefs } from 'pinia';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { LoginSchema } from '../schemas/auth.schema';
import { authLocales } from '../locales/auth.locales';
import { useApiErrorHandler } from '@/shared/composables/api-error-handler.use';

const router = useRouter();
const authStore = useAuthStore();
const { loading } = storeToRefs(authStore);
const { handleApiResult } = useApiErrorHandler();

const { defineField, handleSubmit, errors, setErrors, values, setValues } = useForm({
    validationSchema: toTypedSchema(LoginSchema),
    initialValues: {
        credential: '',
        password: '',
        rememberMe: false
    },
});

const [credential] = defineField('credential');
const [password] = defineField('password');
const [rememberMe] = defineField('rememberMe');

const onSubmit = handleSubmit(async (formValues) => {
    const result = await authStore.login(formValues);

    const handled = handleApiResult(result, {
        setErrors,
        fieldNames: Object.keys(values),
        successTitle: authLocales.titles?.welcome,
        successMessage: authLocales.messages?.login_success,
        errorTitle: authLocales.common?.error,
        genericError: authLocales.messages?.login_failed
    });

    if (handled && result.success) {
        router.push('/');
    }
});

const fillSeedCredentials = () => {
    setValues({
        credential: 'admin@resys.shop',
        password: 'Admin@1234!'
    });
};
</script>

<template>
    <div class="flex items-center justify-center min-h-screen bg-surface-50 dark:bg-surface-950 p-4">
        <div class="w-full max-w-md">
            <div class="text-center mb-6">
                <div class="text-primary text-3xl font-bold mb-2">{{ authLocales.titles?.app_name }}</div>
                <div class="text-surface-500 dark:text-surface-400">{{ authLocales.titles?.app_subtitle }}</div>
            </div>

            <div class="bg-surface-0 dark:bg-surface-900 p-8 rounded-xl shadow-lg border border-surface-200 dark:border-surface-800">
                <h2 class="text-2xl font-semibold mb-6 text-surface-900 dark:text-surface-0">{{ authLocales.titles?.login }}</h2>
                
                <form @submit="onSubmit" class="flex flex-col gap-4">
                    <div class="flex flex-col gap-2">
                        <label for="credential" class="font-medium text-surface-900 dark:text-surface-0">{{ authLocales.labels?.credential }}</label>
                        <InputText 
                            id="credential" 
                            v-model="credential" 
                            type="text" 
                            :placeholder="authLocales.placeholders?.credential" 
                            class="w-full" 
                            :disabled="loading" 
                            :invalid="!!errors.credential"
                        />
                        <small v-if="errors.credential" class="text-red-500">{{ errors.credential }}</small>
                    </div>

                    <div class="flex flex-col gap-2">
                        <label for="password" class="font-medium text-surface-900 dark:text-surface-0">{{ authLocales.labels?.password }}</label>
                        <Password 
                            id="password" 
                            v-model="password" 
                            :feedback="false" 
                            toggleMask 
                            :placeholder="authLocales.placeholders?.password" 
                            inputClass="w-full" 
                            class="w-full" 
                            :disabled="loading" 
                            :invalid="!!errors.password"
                        />
                        <small v-if="errors.password" class="text-red-500">{{ errors.password }}</small>
                    </div>

                    <div class="flex items-center justify-between mt-2">
                        <div class="flex items-center gap-2">
                            <Checkbox id="rememberMe" v-model="rememberMe" :binary="true" :disabled="loading" />
                            <label for="rememberMe" class="text-surface-900 dark:text-surface-0 cursor-pointer">{{ authLocales.labels?.remember_me }}</label>
                        </div>
                        <a href="#" class="text-primary hover:text-primary-600 text-sm font-medium transition-colors">{{ authLocales.labels?.forgot_password }}</a>
                    </div>

                    <Button type="submit" :label="authLocales.labels?.sign_in" icon="pi pi-sign-in" :loading="loading" class="w-full mt-4" />
                </form>

                <div class="mt-6 pt-6 border-t border-surface-200 dark:border-surface-800">
                    <div class="text-sm text-surface-500 dark:text-surface-400 mb-3 text-center">Development Helpers</div>
                    <Button 
                        type="button" 
                        label="Quick Login (Seed Admin)" 
                        icon="pi pi-bolt" 
                        severity="secondary" 
                        outlined 
                        class="w-full" 
                        @click="fillSeedCredentials"
                        :disabled="loading"
                    />
                </div>
            </div>
            
            <div class="text-center mt-6 text-sm text-surface-500 dark:text-surface-400">
                {{ authLocales.messages?.copyright?.replace('{year}', new Date().getFullYear().toString()) }}
            </div>
        </div>
    </div>
</template>

<style scoped>
:deep(.p-password-input) {
    width: 100%;
}
</style>