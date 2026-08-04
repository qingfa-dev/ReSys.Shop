import type { ProfileSchemaType as Profile } from '../types/schemas/profile.schema'

export const mockProfiles: Profile[] = [
  {
    id: 'profile-1',
    email: 'john.doe@example.com',
    firstName: 'John',
    lastName: 'Doe',
    displayName: 'John Doe',
    phone: '+1234567890',
    avatar: 'https://picsum.photos/seed/profile1/200/200',
    dateOfBirth: '1990-05-15',
    gender: 'male',
    createdAt: '2025-01-15T10:00:00Z',
    updatedAt: '2026-04-01T10:00:00Z',
  },
  {
    id: 'profile-2',
    email: 'jane.smith@example.com',
    firstName: 'Jane',
    lastName: 'Smith',
    displayName: 'Jane Smith',
    phone: '+1234567891',
    avatar: 'https://picsum.photos/seed/profile2/200/200',
    dateOfBirth: '1992-08-22',
    gender: 'female',
    createdAt: '2025-02-20T10:00:00Z',
    updatedAt: '2026-03-15T10:00:00Z',
  },
  {
    id: 'profile-3',
    email: 'mike.wilson@example.com',
    firstName: 'Mike',
    lastName: 'Wilson',
    displayName: 'Mike Wilson',
    phone: '+1234567893',
    avatar: 'https://picsum.photos/seed/profile3/200/200',
    dateOfBirth: '1988-11-03',
    gender: 'male',
    createdAt: '2025-03-10T10:00:00Z',
    updatedAt: '2026-02-28T10:00:00Z',
  },
]

export function getProfileById(id: string): Profile | undefined {
  return mockProfiles.find(p => p.id === id)
}
