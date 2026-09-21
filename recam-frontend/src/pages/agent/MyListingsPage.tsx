import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { format } from 'date-fns'

import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent } from '@/components/ui/card'

import { getListingCases } from '@/services/listingCaseService'
import { useAuthStore } from '@/stores/authStore'
import type { PropertyStatusName } from '@/types'

// ─── Status Badge ─────────────────────────────────────────────────────────────

function StatusBadge({ status }: { status: PropertyStatusName }) {
  if (status === 'Created') {
    return <Badge variant="secondary">Created</Badge>
  }
  if (status === 'Pending') {
    return <Badge variant="outline">Pending</Badge>
  }
  return <Badge>Delivered</Badge>
}

// ─── Component ────────────────────────────────────────────────────────────────

export default function MyListingsPage() {
  const navigate = useNavigate()
  const { user } = useAuthStore()

  const { data, isLoading, isError } = useQuery({
    queryKey: ['listingCases'],
    queryFn: () => getListingCases(),
  })

  if (isLoading) {
    return <div className="p-6 text-muted-foreground">Loading listings...</div>
  }

  if (isError) {
    return <div className="p-6 text-destructive">Failed to load listings.</div>
  }

  // GET /api/listingcase already scopes results to cases assigned to this
  // agent (backend filters by AgentId when the caller is an Agent) — no
  // client-side filtering needed.
  const myListings = data?.items ?? []

  return (
    <div className="p-6 space-y-4">
      <div>
        <h1 className="text-2xl font-bold">My Listings</h1>
        <p className="text-sm text-muted-foreground">Welcome, {user?.email}</p>
      </div>

      {myListings.length === 0 ? (
        <p className="text-muted-foreground">No listings assigned to you yet.</p>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {myListings.map((listing) => (
            <Card key={listing.id}>
              <CardContent className="p-4 space-y-3">
                <div className="flex items-start justify-between gap-2">
                  <p className="font-medium leading-tight">{listing.address}</p>
                  <StatusBadge status={listing.status} />
                </div>

                <div className="flex gap-2 text-sm text-muted-foreground">
                  <span>{listing.propertyType}</span>
                  <span>·</span>
                  <span>{listing.saleType}</span>
                </div>

                <div className="flex gap-3 text-sm">
                  <span>{listing.bedrooms} bed</span>
                  <span>{listing.bathrooms} bath</span>
                  <span>{listing.garages} garage</span>
                </div>

                <div className="flex items-center justify-between">
                  <span className="font-semibold">{listing.landSize} m&sup2;</span>
                  <span className="text-xs text-muted-foreground">
                    {format(new Date(listing.createdAt), 'dd MMM yyyy')}
                  </span>
                </div>

                <div className="flex gap-2 pt-1">
                  <Button
                    size="sm"
                    className="flex-1"
                    onClick={() => navigate(`/agent/listings/${listing.id}/select-media`)}
                  >
                    Select Media
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    className="flex-1"
                    onClick={() => navigate(`/agent/listings/${listing.id}/edit-display`)}
                  >
                    Edit Display
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    className="flex-1"
                    onClick={() => navigate(`/agent/listings/${listing.id}/preview`)}
                  >
                    Preview
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
