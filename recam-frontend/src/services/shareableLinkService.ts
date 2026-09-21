import { api } from '@/lib/axios'
import type { CaseContact, MediaAsset, PropertyStatusName, PropertyTypeName, SaleTypeName } from '@/types'

export interface ShareableListing {
  id: number
  address: string
  status: PropertyStatusName
  propertyType: PropertyTypeName
  saleType: SaleTypeName
  bedrooms: number
  bathrooms: number
  garages: number
  landSize: number
  mediaAssets: MediaAsset[]
  caseContacts: CaseContact[]
}

// GET /api/listingcase/view/{token} is [AllowAnonymous] — no auth needed.
// Note: returns ALL non-deleted media for the case, not just agent-selected ones.
export async function getListingByShareToken(token: string): Promise<ShareableListing> {
  return api.get(`/listingcase/view/${token}`) as unknown as Promise<ShareableListing>
}

export async function generateShareToken(listingCaseId: number): Promise<string> {
  return api.post(`/listingcase/${listingCaseId}/publish`) as unknown as Promise<string>
}
