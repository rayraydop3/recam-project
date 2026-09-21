// .NET's JwtSecurityTokenHandler writes claims using their full long-form URIs
// by default (not short names like "sub" or "role") — confirmed by decoding a
// real token issued by the backend.
const CLAIM_USER_ID = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'
const CLAIM_EMAIL = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
const CLAIM_ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

export interface DecodedToken {
  userId: string
  email: string
  role: 'Admin' | 'Agent'
}

function base64UrlDecode(segment: string): string {
  const base64 = segment.replace(/-/g, '+').replace(/_/g, '/')
  const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4)
  return atob(padded)
}

export function decodeToken(token: string): DecodedToken {
  const payloadSegment = token.split('.')[1]
  const claims = JSON.parse(base64UrlDecode(payloadSegment))

  return {
    userId: claims[CLAIM_USER_ID],
    email: claims[CLAIM_EMAIL],
    role: claims[CLAIM_ROLE],
  }
}
