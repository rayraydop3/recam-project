import { useState } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Separator } from '@/components/ui/separator'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

import { PropertyType, SaleType } from '@/types'
import type { ListingCasePayload } from '@/types'
import { assignAgent, getListingCase, updateListingCase } from '@/services/listingCaseService'
import { getCompanyAgents } from '@/services/agentService'

const UNASSIGNED = 'unassigned'

// ─── Schema ───────────────────────────────────────────────────────────────────
// Same shape as CreateListingPage — mirrors the real ListingCasePayload.

const editListingSchema = z.object({
  address: z.string().min(1, 'Address is required'),
  propertyType: z.number(),
  saleType: z.number(),
  bedrooms: z.number().min(0),
  bathrooms: z.number().min(0),
  garages: z.number().min(0),
  landSize: z.number().min(1, 'Land size is required'),
})

type EditListingFormData = z.infer<typeof editListingSchema>

// ─── Component ────────────────────────────────────────────────────────────────

export default function EditListingPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { id } = useParams()
  const listingId = Number(id)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const { data: listing, isLoading, isError } = useQuery({
    queryKey: ['listingCase', listingId],
    queryFn: () => getListingCase(listingId),
  })

  const { data: agents = [] } = useQuery({
    queryKey: ['agents'],
    queryFn: getCompanyAgents,
  })

  const assignMutation = useMutation({
    mutationFn: (agentId: string | null) => assignAgent(listingId, agentId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['listingCase', listingId] })
      queryClient.invalidateQueries({ queryKey: ['listingCases'] })
    },
  })

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm<EditListingFormData>({
    resolver: zodResolver(editListingSchema),
    // `values` (not `defaultValues`) keeps the form in sync once the async
    // fetch above resolves — defaultValues only apply on first render.
    values: listing
      ? {
          address: listing.address,
          propertyType: PropertyType[listing.propertyType],
          saleType: SaleType[listing.saleType],
          bedrooms: listing.bedrooms,
          bathrooms: listing.bathrooms,
          garages: listing.garages,
          landSize: listing.landSize,
        }
      : undefined,
  })

  if (isLoading) {
    return <div className="p-6 text-muted-foreground">Loading listing...</div>
  }

  if (isError || !listing) {
    return (
      <div className="p-6">
        <p className="text-destructive">Listing not found.</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate('/admin/listings')}>
          Back to Listings
        </Button>
      </div>
    )
  }

  const onSubmit = async (data: EditListingFormData) => {
    setSubmitError(null)
    try {
      // Same cast rationale as CreateListingPage — zod widens to `number`.
      await updateListingCase(listingId, data as ListingCasePayload)
      await queryClient.invalidateQueries({ queryKey: ['listingCases'] })
      await queryClient.invalidateQueries({ queryKey: ['listingCase', listingId] })
      navigate('/admin/listings')
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : 'Failed to update listing')
    }
  }

  return (
    <div className="p-6 max-w-2xl space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Edit Listing</h1>
        <Button variant="outline" onClick={() => navigate('/admin/listings')}>
          Cancel
        </Button>
      </div>

      <div className="space-y-1 border rounded-lg p-4">
        <Label>Assigned Agent</Label>
        <Select
          value={listing.agentId ?? UNASSIGNED}
          onValueChange={(val) => assignMutation.mutate(val === UNASSIGNED ? null : val)}
        >
          <SelectTrigger>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={UNASSIGNED}>Unassigned</SelectItem>
            {agents.map((agent) => (
              <SelectItem key={agent.id} value={agent.id}>
                {agent.firstName} {agent.lastName}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {assignMutation.isPending && <p className="text-xs text-muted-foreground">Saving...</p>}
        {assignMutation.isError && (
          <p className="text-xs text-destructive">
            {assignMutation.error instanceof Error ? assignMutation.error.message : 'Failed to assign agent'}
          </p>
        )}
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">

        {/* Property Details */}
        <div className="space-y-4">
          <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
            Property Details
          </h2>

          <div className="space-y-1">
            <Label htmlFor="address">Address</Label>
            <Input id="address" {...register('address')} />
            {errors.address && <p className="text-sm text-destructive">{errors.address.message}</p>}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1">
              <Label>Property Type</Label>
              <Controller
                name="propertyType"
                control={control}
                render={({ field }) => (
                  <Select
                    value={String(field.value)}
                    onValueChange={(val) => field.onChange(Number(val))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={String(PropertyType.House)}>House</SelectItem>
                      <SelectItem value={String(PropertyType.Unit)}>Unit</SelectItem>
                      <SelectItem value={String(PropertyType.Townhouse)}>Townhouse</SelectItem>
                      <SelectItem value={String(PropertyType.Villa)}>Villa</SelectItem>
                      <SelectItem value={String(PropertyType.Others)}>Others</SelectItem>
                    </SelectContent>
                  </Select>
                )}
              />
            </div>

            <div className="space-y-1">
              <Label>Sale Type</Label>
              <Controller
                name="saleType"
                control={control}
                render={({ field }) => (
                  <Select
                    value={String(field.value)}
                    onValueChange={(val) => field.onChange(Number(val))}
                  >
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value={String(SaleType.ForSale)}>For Sale</SelectItem>
                      <SelectItem value={String(SaleType.ForRent)}>For Rent</SelectItem>
                      <SelectItem value={String(SaleType.Auction)}>Auction</SelectItem>
                    </SelectContent>
                  </Select>
                )}
              />
            </div>
          </div>
        </div>

        <Separator />

        {/* Specs */}
        <div className="space-y-4">
          <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
            Specs
          </h2>

          <div className="space-y-1 max-w-[240px]">
            <Label htmlFor="landSize">Land Size (m&sup2;)</Label>
            <Input id="landSize" type="number" {...register('landSize', { valueAsNumber: true })} />
            {errors.landSize && <p className="text-sm text-destructive">{errors.landSize.message}</p>}
          </div>

          <div className="grid grid-cols-3 gap-4">
            <div className="space-y-1">
              <Label htmlFor="bedrooms">Bedrooms</Label>
              <Input id="bedrooms" type="number" min={0} {...register('bedrooms', { valueAsNumber: true })} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="bathrooms">Bathrooms</Label>
              <Input id="bathrooms" type="number" min={0} {...register('bathrooms', { valueAsNumber: true })} />
            </div>
            <div className="space-y-1">
              <Label htmlFor="garages">Garages</Label>
              <Input id="garages" type="number" min={0} {...register('garages', { valueAsNumber: true })} />
            </div>
          </div>
        </div>

        <Separator />

        {submitError && <p className="text-sm text-destructive">{submitError}</p>}

        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Saving...' : 'Save Changes'}
        </Button>

      </form>
    </div>
  )
}
