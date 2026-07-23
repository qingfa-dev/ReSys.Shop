export interface ProductResponse {
  id: string
  name: string
  slug: string
  description: string | null
  status: 'Draft' | 'Active' | 'Archived'
  styleCode: string | null
  seasonName: string | null
  department: string | null
  genderTarget: string | null
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  availableOn: string | null
  discontinueOn: string | null
  materialComposition: string | null
  careInstructions: string | null
  fitNotes: string | null
  createdAt: string
  updatedAt: string
}
