import { api } from '@/lib/axios'
import type { ListingCase, ListingCasePayload, ListingCaseQuery, PagedResult } from '@/types'

export async function getListingCases(query: ListingCaseQuery = {}): Promise<PagedResult<ListingCase>> {
  return api.get('/listingcase', { params: query }) as unknown as Promise<PagedResult<ListingCase>>
}

export async function getListingCase(id: number): Promise<ListingCase> {
  return api.get(`/listingcase/${id}`) as unknown as Promise<ListingCase>
}

export async function createListingCase(payload: ListingCasePayload): Promise<ListingCase> {
  return api.post('/listingcase', payload) as unknown as Promise<ListingCase>
}

export async function updateListingCase(id: number, payload: ListingCasePayload): Promise<ListingCase> {
  return api.put(`/listingcase/${id}`, payload) as unknown as Promise<ListingCase>
}

export async function deleteListingCase(id: number): Promise<void> {
  await api.delete(`/listingcase/${id}`)
}

export async function assignAgent(id: number, agentId: string | null): Promise<ListingCase> {
  return api.patch(`/listingcase/${id}/assign-agent`, { agentId }) as unknown as Promise<ListingCase>
}
