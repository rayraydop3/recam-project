import { api } from '@/lib/axios'
import type { CaseContact } from '@/types'

export interface CaseContactPayload {
  firstName: string
  lastName: string
  email: string
  phone: string
  companyName?: string
  profileImageUrl?: string
}

export async function getCaseContacts(listingCaseId: number): Promise<CaseContact[]> {
  return api.get(`/casecontact/${listingCaseId}`) as unknown as Promise<CaseContact[]>
}

export async function addCaseContact(
  listingCaseId: number,
  payload: CaseContactPayload
): Promise<CaseContact> {
  return api.post(`/casecontact/${listingCaseId}`, payload) as unknown as Promise<CaseContact>
}

export async function deleteCaseContact(id: number): Promise<void> {
  await api.delete(`/casecontact/${id}`)
}
