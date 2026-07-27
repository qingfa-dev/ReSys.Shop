export class ValidationError extends Error {
  constructor(
    message: string,
    public readonly fieldErrors: Record<string, string[]>,
  ) {
    super(message)
    this.name = 'ValidationError'
  }

  getFieldError(field: string): string | undefined {
    const messages = this.fieldErrors[field]
    return messages?.[0]
  }
}
