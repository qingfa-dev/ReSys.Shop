import apiClient from './api.client';
import type { ServerResult } from '../types/result.types';
import type { FileMetadata, FileUploadResponse } from '../types/api.file.types';

export const fileService = {
    async getFileMetadata(path: string): Promise<ServerResult<FileMetadata>> {
        return await apiClient.get(`/files/meta/${path}`).then(res => res.data as ServerResult<FileMetadata>);
    },

    async uploadImage(file: File): Promise<ServerResult<FileUploadResponse>> {
        const formData = new FormData();
        formData.append('file', file);
        return await apiClient.post('/files/image', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        }).then(res => res.data as ServerResult<FileUploadResponse>);
    }
};

export default fileService;
