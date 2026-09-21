// ─── Enums ────────────────────────────────────────────────────────────────────
// Numeric values here MUST match Common/Enums/*.cs on the backend exactly —
// these are what you send in POST/PUT bodies (e.g. CreateListingCaseDto.PropertyType).
// GET responses do NOT send these numbers back — the backend DTOs call
// .ToString() on the enum, so reads come back as plain strings (see
// PropertyTypeName / SaleTypeName / PropertyStatusName below).

export const PropertyType = {
  House: 0,
  Unit: 1,
  Townhouse: 2,
  Villa: 3,
  Others: 4,
} as const
export type PropertyType = (typeof PropertyType)[keyof typeof PropertyType]
export type PropertyTypeName = keyof typeof PropertyType

export const SaleType = {
  ForSale: 0,
  ForRent: 1,
  Auction: 2,
} as const
export type SaleType = (typeof SaleType)[keyof typeof SaleType]
export type SaleTypeName = keyof typeof SaleType

export const PropertyStatus = {
  Created: 0,
  Pending: 1,
  Delivered: 2,
} as const
export type PropertyStatus = (typeof PropertyStatus)[keyof typeof PropertyStatus]
export type PropertyStatusName = keyof typeof PropertyStatus

export const MediaType = {
  Picture: 1,
  Video: 2,
  FloorPlan: 3,
} as const
export type MediaType = (typeof MediaType)[keyof typeof MediaType]
export type MediaTypeName = keyof typeof MediaType

// ─── User & Auth ──────────────────────────────────────────────────────────────

export interface User {
  id: string
  email: string
  role: 'Admin' | 'Agent'
}

export interface Agent {
  id: string
  firstName: string
  lastName: string
  email: string
  phone?: string | null
  profileImageUrl?: string | null
  photographyCompanyId: string
  createdAt: string
}

// ─── Listing ──────────────────────────────────────────────────────────────────
// Mirrors backend DTOs/ListingCase/ListingCaseDto.cs. Note there is no `price`
// field — the backend doesn't track one, only `landSize`.

export interface ListingCase {
  id: number
  address: string
  status: PropertyStatusName
  propertyType: PropertyTypeName
  saleType: SaleTypeName
  bedrooms: number
  bathrooms: number
  garages: number
  landSize: number
  createdAt: string
  photographyCompanyId: string
  agentId?: string | null
}

// Mirrors CreateListingCaseDto/UpdateListingCaseDto.cs — enums as numbers here.
export interface ListingCasePayload {
  address: string
  propertyType: PropertyType
  saleType: SaleType
  bedrooms: number
  bathrooms: number
  garages: number
  landSize: number
}

export interface ListingCaseQuery {
  status?: PropertyType
  propertyType?: PropertyType
  saleType?: SaleType
  address?: string
  page?: number
  pageSize?: number
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

// ─── Media ────────────────────────────────────────────────────────────────────
// Mirrors DTOs/Media/MediaAssetDto.cs

export interface MediaAsset {
  id: number
  fileName: string
  blobUrl: string
  mediaType: MediaTypeName
  isCoverImage: boolean
  createdAt: string
}

// ─── Contact ──────────────────────────────────────────────────────────────────
// Mirrors DTOs/CaseContact/CaseContactDto.cs

export interface CaseContact {
  id: number
  firstName: string
  lastName: string
  email: string
  phone: string
  companyName?: string | null
  profileImageUrl?: string | null
  createdAt: string
}
