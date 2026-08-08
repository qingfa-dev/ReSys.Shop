export class HttpError extends Error {
  constructor(
    public statusCode: number,
    public errors: Array<{ code: string; message: string; type: number; field?: string }> = [],
  ) {
    super(errors[0]?.message ?? `HTTP ${statusCode}`)
    this.name = 'HttpError'
  }
}
