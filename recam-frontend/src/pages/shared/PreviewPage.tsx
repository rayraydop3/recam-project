import { useState } from 'react'
import { useNavigate, useParams, useLocation } from 'react-router-dom'
import { useMutation, useQuery } from '@tanstack/react-query'

import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import { Input } from '@/components/ui/input'

import { getListingCase } from '@/services/listingCaseService'
import { getMediaAssets } from '@/services/mediaService'
import { getFinalSelection } from '@/services/selectedMediaService'
import { getCaseContacts } from '@/services/caseContactService'
import { generateShareToken } from '@/services/shareableLinkService'
import { useAuthStore } from '@/stores/authStore'

// ─── Component ────────────────────────────────────────────────────────────────
// Backend has no "description" field on ListingCase, so that section from the
// old mock UI is gone — only real fields are shown.

export default function PreviewPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const listingId = Number(id)
  const location = useLocation()
  const { user } = useAuthStore()

  const { data: listing, isLoading: listingLoading } = useQuery({
    queryKey: ['listingCase', listingId],
    queryFn: () => getListingCase(listingId),
  })

  const { data: allMedia = [] } = useQuery({
    queryKey: ['media', listingId],
    queryFn: () => getMediaAssets(listingId),
    enabled: !!listing,
  })

  const { data: selectedMedia = [] } = useQuery({
    queryKey: ['selectedMedia', listingId, 'final'],
    queryFn: () => getFinalSelection(listingId),
    enabled: !!listing,
  })

  const { data: contacts = [] } = useQuery({
    queryKey: ['caseContacts', listingId],
    queryFn: () => getCaseContacts(listingId),
    enabled: !!listing,
  })

  const backPath = location.pathname.includes('/admin')
    ? '/admin/listings'
    : '/agent/listings'

  const [shareUrl, setShareUrl] = useState<string | null>(null)
  const publishMutation = useMutation({
    mutationFn: () => generateShareToken(listingId),
    onSuccess: (token) => setShareUrl(`${window.location.origin}/listing/${token}/view`),
  })

  if (listingLoading) {
    return <div className="p-6 text-muted-foreground">Loading...</div>
  }

  if (!listing) {
    return (
      <div className="p-6">
        <p className="text-destructive">Listing not found.</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate(backPath)}>
          Back
        </Button>
      </div>
    )
  }

  const heroImage = allMedia.find((a) => a.isCoverImage)
  const photos = selectedMedia.filter((a) => a.mediaType === 'Picture')

  return (
    <div className="max-w-4xl mx-auto p-6 space-y-8">

      {/* Top Bar */}
      <div className="flex items-center justify-between">
        <Button variant="outline" onClick={() => navigate(backPath)}>
          ← Back
        </Button>
        <div className="flex items-center gap-2">
          <Badge variant="outline">Preview</Badge>
          <Button size="sm" onClick={() => publishMutation.mutate()} disabled={publishMutation.isPending}>
            {publishMutation.isPending ? 'Publishing...' : 'Publish & Get Link'}
          </Button>
        </div>
      </div>

      {shareUrl && (
        <div className="flex items-center gap-2">
          <Input readOnly value={shareUrl} className="flex-1" onFocus={(e) => e.target.select()} />
          <Button
            size="sm"
            variant="outline"
            onClick={() => navigator.clipboard.writeText(shareUrl)}
          >
            Copy
          </Button>
        </div>
      )}

      {publishMutation.isError && (
        <p className="text-sm text-destructive">
          {publishMutation.error instanceof Error ? publishMutation.error.message : 'Failed to publish'}
        </p>
      )}

      {/* Hero Image */}
      {heroImage ? (
        <img
          src={heroImage.blobUrl}
          alt="Cover"
          className="w-full h-72 object-cover rounded-xl"
        />
      ) : (
        <div className="w-full h-72 bg-muted rounded-xl flex items-center justify-center text-muted-foreground">
          No cover image set
        </div>
      )}

      {/* Title & Badges */}
      <div className="space-y-2">
        <div className="flex flex-wrap gap-2">
          <Badge>{listing.saleType}</Badge>
          <Badge variant="secondary">{listing.propertyType}</Badge>
        </div>
        <h1 className="text-3xl font-bold">{listing.address}</h1>
      </div>

      <Separator />

      {/* Specs */}
      <div className="grid grid-cols-4 gap-4 text-center">
        <div className="space-y-1">
          <p className="text-2xl font-bold">{listing.bedrooms}</p>
          <p className="text-sm text-muted-foreground">Bedrooms</p>
        </div>
        <div className="space-y-1">
          <p className="text-2xl font-bold">{listing.bathrooms}</p>
          <p className="text-sm text-muted-foreground">Bathrooms</p>
        </div>
        <div className="space-y-1">
          <p className="text-2xl font-bold">{listing.garages}</p>
          <p className="text-sm text-muted-foreground">Garages</p>
        </div>
        <div className="space-y-1">
          <p className="text-2xl font-bold">{listing.landSize}</p>
          <p className="text-sm text-muted-foreground">m&sup2;</p>
        </div>
      </div>

      {/* Photo Gallery */}
      {photos.length > 0 && (
        <>
          <Separator />
          <div className="space-y-3">
            <h2 className="text-lg font-semibold">Photos</h2>
            <div className="grid grid-cols-2 md:grid-cols-3 gap-3">
              {photos.map((photo) => (
                <img
                  key={photo.id}
                  src={photo.blobUrl}
                  alt=""
                  className="w-full h-40 object-cover rounded-lg"
                />
              ))}
            </div>
          </div>
        </>
      )}

      {/* Contacts */}
      {contacts.length > 0 && (
        <>
          <Separator />
          <div className="space-y-3">
            <h2 className="text-lg font-semibold">Contact</h2>
            <div className="flex flex-col gap-3">
              {contacts.map((contact) => (
                <div key={contact.id} className="flex items-center justify-between border rounded-lg p-4">
                  <div>
                    <p className="font-medium">{contact.firstName} {contact.lastName}</p>
                    <p className="text-sm text-muted-foreground">{contact.companyName}</p>
                  </div>
                  <div className="text-right text-sm text-muted-foreground">
                    <p>{contact.email}</p>
                    <p>{contact.phone}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </>
      )}

      {/* Admin only: show note if no media selected */}
      {user?.role === 'Admin' && selectedMedia.length === 0 && (
        <>
          <Separator />
          <p className="text-sm text-muted-foreground text-center">
            No media selected yet. Agent needs to select media first.
          </p>
        </>
      )}

    </div>
  )
}
