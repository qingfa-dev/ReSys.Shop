import type { AuthenticationResponse } from '../types/auth.response.types'

export interface AuthDto {
  access_token: string
  access_token_expires_in: number
  refresh_token: string
  refresh_token_expires_in: number
}

export function mapAuthResponse(dto: AuthDto): AuthenticationResponse {
  return {
    accessToken: dto.access_token,
    accessTokenExpiresIn: dto.access_token_expires_in,
    refreshToken: dto.refresh_token,
    refreshTokenExpiresIn: dto.refresh_token_expires_in,
  }
}

export function mapJwtToProfile(jwt: Record<string, unknown>): { id: string; email: string; roles: string[] } {
  return {
    id: String(jwt.sub || jwt.nameid || ''),
    email: String(jwt.email || ''),
    roles: Array.isArray(jwt.role) ? jwt.role : jwt.role ? [String(jwt.role)] : [],
  }
}
