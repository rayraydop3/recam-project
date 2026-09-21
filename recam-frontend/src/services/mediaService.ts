import { api } from '@/lib/axios'
import type { MediaAsset, MediaType } from '@/types'

export async function getMediaAssets(listingCaseId: number): Promise<MediaAsset[]> {
  return api.get(`/media/${listingCaseId}`) as unknown as Promise<MediaAsset[]>
}

export async function uploadMedia(
  files: File[],
  listingCaseId: number,
  mediaType: MediaType
): Promise<MediaAsset[]> {
  const formData = new FormData()
  files.forEach((file) => formData.append('files', file))
  formData.append('listingCaseId', String(listingCaseId))
  formData.append('mediaType', String(mediaType))

  return api.post('/media/upload', formData) as unknown as Promise<MediaAsset[]>
}

export async function setCoverImage(id: number): Promise<void> {
  await api.patch(`/media/${id}/cover`)
}

export async function deleteMedia(id: number): Promise<void> {
  await api.delete(`/media/${id}`)
}
