import { get, post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { Result } from '@/shared/types/result'
import type { SessionInfo, SessionUser } from '../types/auth'
import * as tokenService from './tokenService'

/**
 * Session management for the storefront account area.
 *
 * CONTRACT NOTE — verified against the backend
 * (`service/Api/src/Module/Identity/Features/Storefront/Auth`):
 * - `GET api/store/identity/auth/sessions` returns the CURRENT user's session payload
 *   (`{ id, userName, email, roles, permissions }`, i.e. `SessionUser`) — NOT a list of
 *   active devices. There is no list-sessions endpoint.
 * - There is no `DELETE /sessions/{id}` (per-session revoke by id) and no `DELETE /sessions`.
 * - Device logout is `POST api/store/identity/auth/logout` with `{ refreshToken, revokeAll }`:
 *   `revokeAll: true` revokes every device; `revokeAll: false` + a refresh token revokes that
 *   single device.
 *
 * To honor the `SessionInfo[]` view shape, `getSessions()` returns a single entry describing
 * the current device. Device name is derived client-side from the browser user agent; IP and
 * last-activity are not exposed by the API (IP is left empty, last activity is "now").
 */

export async function getSessions(): Promise<Result<SessionInfo[]>> {
  const session = await get<Result<SessionUser>>(ENDPOINTS.sessions)
  if (!session.isSuccess) return { ...session, value: [] }
  return { ...session, value: [buildCurrentSession(session.value.id)] }
}

/** Revokes the current device's refresh token (`POST /logout` with `revokeAll: false`). */
export async function revokeCurrentDevice(): Promise<Result<unknown>> {
  return post<Result<unknown>>(ENDPOINTS.authLogout, {
    revokeAll: false,
    refreshToken: tokenService.getRefreshToken(),
  })
}

/** Revokes every active refresh token for the user (`POST /logout` with `revokeAll: true`). */
export async function revokeAll(): Promise<Result<unknown>> {
  return post<Result<unknown>>(ENDPOINTS.authLogout, { revokeAll: true })
}

function buildCurrentSession(id: string): SessionInfo {
  return {
    id,
    deviceName: describeDevice(),
    ipAddress: '',
    lastActivityAt: new Date().toISOString(),
    isCurrent: true,
  }
}

/** Describes the current browser + OS from the user agent, e.g. "Chrome · Linux". */
function describeDevice(): string {
  const ua = typeof navigator === 'undefined' ? '' : navigator.userAgent
  const browser = /Edg\//.test(ua)
    ? 'Edge'
    : /OPR\//.test(ua)
      ? 'Opera'
      : /Firefox\//.test(ua)
        ? 'Firefox'
        : /Chrome\//.test(ua)
          ? 'Chrome'
          : /Safari\//.test(ua)
            ? 'Safari'
            : 'Browser'
  const os = /Windows NT/.test(ua)
    ? 'Windows'
    : /Mac OS X/.test(ua)
      ? 'macOS'
      : /Android/.test(ua)
        ? 'Android'
        : /iPhone|iPad|iPod/.test(ua)
          ? 'iOS'
          : /Linux/.test(ua)
            ? 'Linux'
            : ''
  return [browser, os].filter(Boolean).join(' · ')
}
