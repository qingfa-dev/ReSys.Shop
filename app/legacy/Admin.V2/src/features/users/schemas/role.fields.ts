import { z } from 'zod'
export type TFunction = (key: string) => string
export class RoleFields {
  constructor(private t: TFunction) {}
  name() { return z.string().min(1, 'Name is required') }
  description() { return z.string().optional() }
}
