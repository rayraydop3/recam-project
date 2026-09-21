import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'

import { getMediaAssets } from '@/services/mediaService'
import { getFinalSelection, selectMedia } from '@/services/selectedMediaService'
import type { MediaAsset, MediaTypeName } from '@/types'

// ─── Tabs Config ──────────────────────────────────────────────────────────────

const TABS: { label: string; value: string; mediaTypeName: MediaTypeName }[] = [
  { label: 'Photos', value: 'photo', mediaTypeName: 'Picture' },
  { label: 'Videos', value: 'video', mediaTypeName: 'Video' },
  { label: 'Floor Plans', value: 'floorplan', mediaTypeName: 'FloorPlan' },
]

// ─── Selectable Media Card ────────────────────────────────────────────────────

function SelectableCard({
  asset,
  selected,
  onToggle,
}: {
  asset: MediaAsset
  selected: boolean
  onToggle: (id: number, nextSelected: boolean) => void
}) {
  return (
    <div
      className={`relative border-2 rounded-lg overflow-hidden cursor-pointer transition-all ${
        selected ? 'border-primary' : 'border-transparent'
      }`}
      onClick={() => onToggle(asset.id, !selected)}
    >
      <img src={asset.blobUrl} alt="" className="w-full h-40 object-cover" />

      {asset.isCoverImage && (
        <Badge className="absolute top-2 left-2">Cover</Badge>
      )}

      <div
        className={`absolute top-2 right-2 w-6 h-6 rounded-full border-2 flex items-center justify-center transition-colors ${
          selected
            ? 'bg-primary border-primary text-primary-foreground'
            : 'bg-white/80 border-gray-300'
        }`}
      >
        {selected && (
          <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
          </svg>
        )}
      </div>
    </div>
  )
}

// ─── Component ────────────────────────────────────────────────────────────────

export default function SelectMediaPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { id } = useParams()
  const listingId = Number(id)

  const { data: assets = [], isLoading: assetsLoading } = useQuery({
    queryKey: ['media', listingId],
    queryFn: () => getMediaAssets(listingId),
  })

  const { data: finalSelection = [], isLoading: selectionLoading } = useQuery({
    queryKey: ['selectedMedia', listingId, 'final'],
    queryFn: () => getFinalSelection(listingId),
  })

  // Every click below calls PUT /api/selectedmedia immediately (not batched) —
  // the backend logs each toggle as history (see MediaSelectionHistory), so
  // there's no separate "save" step, selections persist as you click.
  const toggleMutation = useMutation({
    mutationFn: ({ mediaAssetId, isSelected }: { mediaAssetId: number; isSelected: boolean }) =>
      selectMedia(listingId, mediaAssetId, isSelected),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['selectedMedia', listingId, 'final'] }),
  })

  if (assetsLoading || selectionLoading) {
    return <div className="p-6 text-muted-foreground">Loading media...</div>
  }

  const selectedIds = new Set(finalSelection.map((s) => s.mediaAssetId))
  const selectedCount = selectedIds.size

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Select Media</h1>
          <p className="text-sm text-muted-foreground">Listing #{listingId}</p>
        </div>
        <div className="flex items-center gap-3">
          <span className="text-sm text-muted-foreground">
            {selectedCount} selected
          </span>
          <Button onClick={() => navigate('/agent/listings')}>
            Done
          </Button>
        </div>
      </div>

      <Tabs defaultValue="photo">
        <TabsList>
          {TABS.map((tab) => {
            const count = assets.filter(
              (a) => a.mediaType === tab.mediaTypeName && selectedIds.has(a.id)
            ).length
            return (
              <TabsTrigger key={tab.value} value={tab.value}>
                {tab.label}
                {count > 0 && (
                  <Badge variant="secondary" className="ml-2 text-xs">
                    {count}
                  </Badge>
                )}
              </TabsTrigger>
            )
          })}
        </TabsList>

        {TABS.map((tab) => {
          const tabAssets = assets.filter((a) => a.mediaType === tab.mediaTypeName)
          return (
            <TabsContent key={tab.value} value={tab.value} className="mt-4">
              {tabAssets.length === 0 ? (
                <p className="text-muted-foreground text-sm py-8 text-center">
                  No {tab.label.toLowerCase()} available for this listing.
                </p>
              ) : (
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                  {tabAssets.map((asset) => (
                    <SelectableCard
                      key={asset.id}
                      asset={asset}
                      selected={selectedIds.has(asset.id)}
                      onToggle={(mediaAssetId, nextSelected) =>
                        toggleMutation.mutate({ mediaAssetId, isSelected: nextSelected })
                      }
                    />
                  ))}
                </div>
              )}
            </TabsContent>
          )
        })}
      </Tabs>
    </div>
  )
}
