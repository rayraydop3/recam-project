import { api } from '@/lib/axios'
import { decodeToken } from '@/utils/jwt'
import type { User } from '@/types'

export interface LoginPayload {
  email: string
  password: string
}

export async function login(payload: LoginPayload): Promise<{ token: string; user: User }> {
  // api.post resolves directly to the unwrapped `Data` field (a raw token string here) —
  // see the comment in src/lib/axios.ts for why the cast is needed.
  const token = (await api.post('/auth/login', payload)) as unknown as string
  const decoded = decodeToken(token)

  const user: User = {
    id: decoded.userId,
    email: decoded.email,
    role: decoded.role,
  }

  return { token, user }
}
