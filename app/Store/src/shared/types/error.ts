export enum ErrorType {
  Validation = 422,
  NotFound = 404,
  Unauthorized = 401,
  Forbidden = 403,
  Conflict = 409,
  ServerError = 500,
  NetworkError = 0,
}

export interface ApiError {
  code: string
  message: string
  type: number
  field?: string
}

export enum StatusCode {
  Ok = 200,
  Created = 201,
  NoContent = 204,
  BadRequest = 400,
  Unauthorized = 401,
  Forbidden = 403,
  NotFound = 404,
  Conflict = 409,
  InternalServerError = 500,
}
