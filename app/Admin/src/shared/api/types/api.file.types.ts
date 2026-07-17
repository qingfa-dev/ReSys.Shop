export interface FileMetadata {
    fileId: string;
    fileName: string;
    originalFileName: string;
    fileSize: number;
    contentType: string;
    hash: string;
    subdirectory: string;
    createdAt: string;
    extension?: string;
    isEncrypted: boolean;
    modifiedAt?: string;
    customMetadata?: Record<string, string>;
}

export interface FileUploadResponse {
    width: number;
    height: number;
    format: string;
    sizeBytes: number;
    savedName: string;
    url: string;
    hash: string;
}
