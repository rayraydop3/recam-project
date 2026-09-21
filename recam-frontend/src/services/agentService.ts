import { api } from '@/lib/axios'
import type { Agent } from '@/types'

export async function getCompanyAgents(): Promise<Agent[]> {
  return api.get('/agent/company') as unknown as Promise<Agent[]>
}
