import { useParams, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'

import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'

import { getListingByShareToken } from '@/services/shareableLinkService'

// ─── Component ────────────────────────────────────────────────────────────────
// Reached via the real share link (GET /api/listingcase/view/{token},
// AllowAnonymous — no login required). The URL param is the share token, not
// a database id. Note the backend returns ALL uploaded media here, not just
// what an agent marked as selected — that's real current backend behavior,
// not a frontend bug.

export default function PropertyWebsitePage() {
  const { token } = useParams()
  const navigate = useNavigate()

  const { data: listing, isLoading, isError } = useQuery({
    queryKey: ['shareableListing', token],
    queryFn: () => getListingByShareToken(token!),
    enabled: !!token,
  })

  if (isLoading) {
    return <div className="min-h-screen flex items-center justify-center text-muted-foreground">Loading...</div>
  }

  if (isError || !listing) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center gap-4">
        <p className="text-muted-foreground">This listing is not available.</p>
        <Button variant="outline" onClick={() => navigate('/login')}>
          Go to Login
        </Button>
      </div>
    )
  }

  const heroImage = listing.mediaAssets.find((a) => a.isCoverImage)
  const photos = listing.mediaAssets.filter((a) => a.mediaType === 'Picture')

  return (
    <div className="min-h-screen bg-background">

      {/* Header */}
      <header className="border-b px-6 h-14 flex items-center justify-between sticky top-0 bg-background z-10">
        <span className="font-bold text-lg tracking-tight">Recam</span>
        <Badge variant="outline">{listing.saleType}</Badge>
      </header>

      <div className="max-w-5xl mx-auto px-6 py-10 space-y-10">

        {/* Hero Image */}
        {heroImage ? (
          <img
            src={heroImage.blobUrl}
            alt="Cover"
            className="w-full h-96 object-cover rounded-2xl"
          />
        ) : (
          <div className="w-full h-96 bg-muted rounded-2xl flex items-center justify-center text-muted-foreground">
            No cover image
          </div>
        )}

        {/* Title */}
        <div className="space-y-3">
          <div className="flex flex-wrap gap-2">
            <Badge>{listing.saleType}</Badge>
            <Badge variant="secondary">{listing.propertyType}</Badge>
          </div>
          <h1 className="text-4xl font-bold">{listing.address}</h1>
        </div>

        <Separator />

        {/* Specs */}
        <div className="grid grid-cols-4 gap-6 text-center">
          {[
            { value: listing.bedrooms, label: 'Bedrooms' },
            { value: listing.bathrooms, label: 'Bathrooms' },
            { value: listing.garages, label: 'Garages' },
            { value: `${listing.landSize} m²`, label: 'Land Size' },
          ].map((spec) => (
            <div key={spec.label} className="border rounded-xl p-4 space-y-1">
              <p className="text-3xl font-bold">{spec.value}</p>
              <p className="text-sm text-muted-foreground">{spec.label}</p>
            </div>
          ))}
        </div>

        {/* Photo Gallery */}
        {photos.length > 0 && (
          <>
            <Separator />
            <div className="space-y-4">
              <h2 className="text-2xl font-semibold">Gallery</h2>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {photos.map((photo) => (
                  <img
                    key={photo.id}
                    src={photo.blobUrl}
                    alt=""
                    className="w-full h-52 object-cover rounded-xl hover:opacity-90 transition-opacity cursor-pointer"
                  />
                ))}
              </div>
            </div>
          </>
        )}

        {/* Contacts */}
        {listing.caseContacts.length > 0 && (
          <>
            <Separator />
            <div className="space-y-4">
              <h2 className="text-2xl font-semibold">Contact Agent</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {listing.caseContacts.map((contact) => (
                  <div
                    key={contact.id}
                    className="border rounded-xl p-5 space-y-2"
                  >
                    <p className="font-semibold text-lg">
                      {contact.firstName} {contact.lastName}
                    </p>
                    <p className="text-muted-foreground">{contact.companyName}</p>
                    <Separator />
                    <div className="space-y-1 text-sm">
                      <p>📧 {contact.email}</p>
                      <p>📞 {contact.phone}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </>
        )}

        {/* Footer */}
        <Separator />
        <p className="text-center text-sm text-muted-foreground pb-6">
          Powered by Recam · Real Estate Media Delivery
        </p>

      </div>
    </div>
  )
}
