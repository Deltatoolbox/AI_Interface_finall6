import { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

interface ProtectedRouteProps {
  children: ReactNode
  requireAdmin?: boolean
}

export function ProtectedRoute({ children, requireAdmin = false }: ProtectedRouteProps) {
  const { user, isLoading } = useAuth()

  console.log('ProtectedRoute - isLoading:', isLoading, 'user:', user)

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  if (!user) {
    console.log('ProtectedRoute - No user, redirecting to login')
    return <Navigate to="/login" replace />
  }

  if (requireAdmin && user.role !== 'Admin') {
    console.log('ProtectedRoute - User not admin, redirecting to chat')
    return <Navigate to="/chat" replace />
  }

  console.log('ProtectedRoute - User authenticated, rendering children')
  return <>{children}</>
}
