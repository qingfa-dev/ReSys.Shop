export interface FileUploadResponse {
  fileId: string
  fileName: string
  fileUrl: string
  mimeType: string
  size: number
  uploadedAt: string
}

export interface FileUploadOptions {
  fieldName?: string
  metadata?: Record<string, unknown>
}

export function createFileUploadOptions(fieldName: string, metadata?: Record<string, unknown>): FileUploadOptions {
  return { fieldName, metadata }
}