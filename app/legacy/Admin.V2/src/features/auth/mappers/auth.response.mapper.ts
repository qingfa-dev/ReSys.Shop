import type { SessionResponse } from "../types";
import type { CurrentUser } from "@/shared/types/user";

export class AuthResponseMapper {
  static fromJwt(payload: Record<string, unknown>): CurrentUser {
    return {
      id: (payload.sub as string) ?? "",
      email: (payload.email as string) ?? "",
      name: (payload.name as string) ?? "",
      role: (payload.role as string) ?? "",
      permissions: (payload.permissions as string[]) ?? [],
    };
  }

  static fromSession(session: SessionResponse): CurrentUser {
    return {
      id: session.id,
      email: "",
      name: "",
      role: session.roles[0] ?? "",
      permissions: Array.isArray(session.permissions) ? session.permissions : [],
    };
  }
}
