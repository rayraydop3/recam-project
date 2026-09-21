import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from '@/stores/authStore'
import type { User } from '@/types'

interface ProtectedRouteProps {
  allowedRole: User['role']
}

export default function ProtectedRoute({ allowedRole }: ProtectedRouteProps) {
  const { user, isAuthenticated } = useAuthStore()

  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />
  }

  if (user?.role !== allowedRole) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}
