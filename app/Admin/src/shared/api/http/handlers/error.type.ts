export interface ParsedApiError {
  statusCode: number
  title: string | null
  message: string | null
  detail: string | null
  isSuccess: boolean
  errors: Record<string, string[]>
  errorCode: string | undefined
}
