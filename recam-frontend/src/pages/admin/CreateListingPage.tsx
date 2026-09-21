import { useState } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useNavigate } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'

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
import { createListingCase } from '@/services/listingCaseService'

// ─── Schema ───────────────────────────────────────────────────────────────────
// Mirrors backend ListingCasePayload — no title/description/price/floorArea/
// street-city-state-postcode split, because the backend doesn't have them.
// Address is one free-text field; the only "size" concept the backend tracks
// is landSize.

const createListingSchema = z.object({
  address: z.string().min(1, 'Address is required'),
  propertyType: z.number(),
  saleType: z.number(),
  bedrooms: z.number().min(0),
  bathrooms: z.number().min(0),
  garages: z.number().min(0),
  landSize: z.number().min(1, 'Land size is required'),
})

type CreateListingFormData = z.infer<typeof createListingSchema>

// ─── Component ────────────────────────────────────────────────────────────────

export default function CreateListingPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [submitError, setSubmitError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm<CreateListingFormData>({
    resolver: zodResolver(createListingSchema),
    defaultValues: {
      bedrooms: 1,
      bathrooms: 1,
      garages: 0,
      propertyType: PropertyType.House,
      saleType: SaleType.ForSale,
    },
  })

  const onSubmit = async (data: CreateListingFormData) => {
    setSubmitError(null)
    try {
      // zod widens propertyType/saleType to `number`; the Select options only ever
      // produce valid PropertyType/SaleType values, so this cast is safe.
      await createListingCase(data as ListingCasePayload)
      await queryClient.invalidateQueries({ queryKey: ['listingCases'] })
      navigate('/admin/listings')
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : 'Failed to create listing')
    }
  }

  return (
    <div className="p-6 max-w-2xl space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">New Listing</h1>
        <Button variant="outline" onClick={() => navigate('/admin/listings')}>
          Cancel
        </Button>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">

        {/* Property Details */}
        <div className="space-y-4">
          <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
            Property Details
          </h2>

          <div className="space-y-1">
            <Label htmlFor="address">Address</Label>
            <Input id="address" placeholder="e.g. 12 Crown Street, Surry Hills NSW 2010" {...register('address')} />
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
            <Input id="landSize" type="number" placeholder="e.g. 500" {...register('landSize', { valueAsNumber: true })} />
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
          {isSubmitting ? 'Creating...' : 'Create Listing'}
        </Button>

      </form>
    </div>
  )
}
