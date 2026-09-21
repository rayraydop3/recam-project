import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { format } from 'date-fns'

import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

import { getListingCases } from '@/services/listingCaseService'
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

export default function ListingsPage() {
  const navigate = useNavigate()

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

  const listings = data?.items ?? []

  return (
    <div className="p-6 space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Listings</h1>
        <Button onClick={() => navigate('/admin/listings/create')}>
          + New Listing
        </Button>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Address</TableHead>
            <TableHead>Type</TableHead>
            <TableHead>Category</TableHead>
            <TableHead>Land Size</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Created</TableHead>
            <TableHead>Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {listings.map((listing) => (
            <TableRow key={listing.id}>
              <TableCell>
                <div className="font-medium">{listing.address}</div>
                <div className="text-sm text-muted-foreground">
                  {listing.bedrooms} bed &middot; {listing.bathrooms} bath &middot; {listing.garages} garage
                </div>
              </TableCell>
              <TableCell>{listing.propertyType}</TableCell>
              <TableCell>{listing.saleType}</TableCell>
              <TableCell>{listing.landSize} m&sup2;</TableCell>
              <TableCell>
                <StatusBadge status={listing.status} />
              </TableCell>
              <TableCell className="text-muted-foreground">
                {format(new Date(listing.createdAt), 'dd MMM yyyy')}
              </TableCell>
              <TableCell>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => navigate(`/admin/listings/${listing.id}/edit`)}
                  >
                    Edit
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => navigate(`/admin/listings/${listing.id}/media`)}
                  >
                    Media
                  </Button>
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
