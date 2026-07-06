export interface EnvelopeError {
  code: string
  message: string
  field?: string
}

export interface Envelope<T> {
  isSuccess: boolean
  value: T | null
  errors: EnvelopeError[]
}
