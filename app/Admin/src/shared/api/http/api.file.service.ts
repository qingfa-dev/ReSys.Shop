import apiClient from './api.client';
import type { ApiResult } from '../types/api.types';
import type { FileMetadata, FileUploadResponse } from '../types/api.file.types';

export const fileService = {
    async getFileMetadata(path: string): Promise<ApiResult<FileMetadata>> {
        return await apiClient.get(`/files/meta/${path}`);
    },

    async uploadImage(file: File): Promise<ApiResult<FileUploadResponse>> {
        const formData = new FormData();
        formData.append('file', file);
        return await apiClient.post('/files/image', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
    }
};

export default fileService;
