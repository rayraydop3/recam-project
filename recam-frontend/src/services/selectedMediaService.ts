import { api } from '@/lib/axios'
import type { MediaTypeName } from '@/types'

export interface SelectedMediaResult {
  id: number
  mediaAssetId: number
  fileName: string
  blobUrl: string
  mediaType: MediaTypeName
  isCoverImage: boolean
  isSelected: boolean
  selectedAt: string
}

export async function selectMedia(
  listingCaseId: number,
  mediaAssetId: number,
  isSelected: boolean
): Promise<SelectedMediaResult> {
  return api.put('/selectedmedia', {
    listingCaseId,
    mediaAssetId,
    isSelected,
  }) as unknown as Promise<SelectedMediaResult>
}

export async function getFinalSelection(listingCaseId: number): Promise<SelectedMediaResult[]> {
  return api.get(`/selectedmedia/${listingCaseId}/final`) as unknown as Promise<SelectedMediaResult[]>
}
