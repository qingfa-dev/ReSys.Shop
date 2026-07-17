import type { UserParameters } from '../schemas/User.Schema'
export type CreateAdminUserRequest = UserParameters
export type UpdateAdminUserRequest = Partial<CreateAdminUserRequest>
