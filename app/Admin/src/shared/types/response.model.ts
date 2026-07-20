export interface Response {
  id: string
}

export interface AuditableResponse extends Response {
  createdAtUtc: string
  modifiedAtUtc?: string
  createdBy?: string
  modifiedBy?: string
}
