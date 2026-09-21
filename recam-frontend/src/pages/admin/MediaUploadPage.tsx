import { useRef, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'

import { getListingCase } from '@/services/listingCaseService'
import { deleteMedia, getMediaAssets, setCoverImage, uploadMedia } from '@/services/mediaService'
import { MediaType } from '@/types'
import type { MediaAsset, MediaTypeName } from '@/types'

// ─── Media Type Tab Config ────────────────────────────────────────────────────
// `mediaType` (number) is what the upload endpoint expects; `mediaTypeName`
// (string) is what GET /api/media/{id} actually returns per asset, since the
// backend DTO serializes the enum via .ToString().

const TABS: { label: string; value: string; mediaType: MediaType; mediaTypeName: MediaTypeName; accept: string }[] = [
  { label: 'Photos', value: 'photo', mediaType: MediaType.Picture, mediaTypeName: 'Picture', accept: 'image/*' },
  { label: 'Videos', value: 'video', mediaType: MediaType.Video, mediaTypeName: 'Video', accept: 'video/*' },
  { label: 'Floor Plans', value: 'floorplan', mediaType: MediaType.FloorPlan, mediaTypeName: 'FloorPlan', accept: 'image/*,.pdf' },
]

// ─── Media Card ───────────────────────────────────────────────────────────────

function MediaCard({
  asset,
  onSetCover,
  onDelete,
}: {
  asset: MediaAsset
  onSetCover: (id: number) => void
  onDelete: (id: number) => void
}) {
  return (
    <div className="relative border rounded-lg overflow-hidden group">
      <img
        src={asset.blobUrl}
        alt=""
        className="w-full h-40 object-cover"
      />
      {asset.isCoverImage && (
        <Badge className="absolute top-2 left-2">Cover</Badge>
      )}
      <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
        {!asset.isCoverImage && (
          <Button size="sm" variant="secondary" onClick={() => onSetCover(asset.id)}>
            Set Cover
          </Button>
        )}
        <Button size="sm" variant="destructive" onClick={() => onDelete(asset.id)}>
          Delete
        </Button>
      </div>
    </div>
  )
}

// ─── Component ────────────────────────────────────────────────────────────────

export default function MediaUploadPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { id } = useParams()
  const listingId = Number(id)
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [activeTab, setActiveTab] = useState('photo')
  const [uploadError, setUploadError] = useState<string | null>(null)

  const { data: listing, isLoading: listingLoading } = useQuery({
    queryKey: ['listingCase', listingId],
    queryFn: () => getListingCase(listingId),
  })

  const { data: assets = [], isLoading: assetsLoading } = useQuery({
    queryKey: ['media', listingId],
    queryFn: () => getMediaAssets(listingId),
  })

  const invalidateAssets = () => queryClient.invalidateQueries({ queryKey: ['media', listingId] })

  const uploadMutation = useMutation({
    mutationFn: ({ files, mediaType }: { files: File[]; mediaType: MediaType }) =>
      uploadMedia(files, listingId, mediaType),
    onSuccess: invalidateAssets,
  })

  const coverMutation = useMutation({
    mutationFn: (assetId: number) => setCoverImage(assetId),
    onSuccess: invalidateAssets,
  })

  const deleteMutation = useMutation({
    mutationFn: (assetId: number) => deleteMedia(assetId),
    onSuccess: invalidateAssets,
  })

  if (listingLoading || assetsLoading) {
    return <div className="p-6 text-muted-foreground">Loading...</div>
  }

  if (!listing) {
    return (
      <div className="p-6">
        <p className="text-destructive">Listing not found.</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate('/admin/listings')}>
          Back to Listings
        </Button>
      </div>
    )
  }

  const currentTab = TABS.find((t) => t.value === activeTab)!
  const filteredAssets = assets.filter((a) => a.mediaType === currentTab.mediaTypeName)

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? [])
    e.target.value = ''
    if (files.length === 0) return

    setUploadError(null)
    try {
      await uploadMutation.mutateAsync({ files, mediaType: currentTab.mediaType })
    } catch (err) {
      setUploadError(err instanceof Error ? err.message : 'Upload failed')
    }
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Media</h1>
          <p className="text-sm text-muted-foreground">{listing.address}</p>
        </div>
        <Button variant="outline" onClick={() => navigate('/admin/listings')}>
          Back
        </Button>
      </div>

      {uploadError && <p className="text-sm text-destructive">{uploadError}</p>}

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <div className="flex items-center justify-between">
          <TabsList>
            {TABS.map((tab) => (
              <TabsTrigger key={tab.value} value={tab.value}>
                {tab.label}
              </TabsTrigger>
            ))}
          </TabsList>

          <Button onClick={() => fileInputRef.current?.click()} disabled={uploadMutation.isPending}>
            {uploadMutation.isPending ? 'Uploading...' : `+ Upload ${currentTab.label}`}
          </Button>
        </div>

        <input
          ref={fileInputRef}
          type="file"
          accept={currentTab.accept}
          multiple
          className="hidden"
          onChange={handleFileChange}
        />

        {TABS.map((tab) => (
          <TabsContent key={tab.value} value={tab.value} className="mt-4">
            {filteredAssets.length === 0 ? (
              <div
                className="border-2 border-dashed rounded-lg p-16 text-center text-muted-foreground cursor-pointer hover:border-primary transition-colors"
                onClick={() => fileInputRef.current?.click()}
              >
                <p className="text-sm">Click to upload {tab.label.toLowerCase()}</p>
              </div>
            ) : (
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {filteredAssets.map((asset) => (
                  <MediaCard
                    key={asset.id}
                    asset={asset}
                    onSetCover={(assetId) => coverMutation.mutate(assetId)}
                    onDelete={(assetId) => deleteMutation.mutate(assetId)}
                  />
                ))}
              </div>
            )}
          </TabsContent>
        ))}
      </Tabs>
    </div>
  )
}
