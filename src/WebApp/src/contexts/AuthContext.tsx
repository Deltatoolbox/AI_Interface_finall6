import { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { api } from '../api'

interface User {
  id: string
  username: string
  email: string
  role: string
  createdAt: string
}

interface AuthContextType {
  user: User | null
  login: (username: string, password: string) => Promise<boolean>
  register: (username: string, password: string, email: string) => Promise<boolean>
  logout: () => Promise<void>
  isLoading: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    checkAuthStatus()
  }, [])

  const checkAuthStatus = async () => {
    try {
      console.log('Checking auth status...')
      const token = getCookie('access_token')
      console.log('Found token:', token)
      
      if (token) {
        // Decode JWT token to get user info
        try {
          const payload = JSON.parse(atob(token.split('.')[1]))
          console.log('JWT Payload:', payload)
          
          // Check if token is expired
          const now = Math.floor(Date.now() / 1000)
          if (payload.exp && payload.exp < now) {
            console.log('Token expired, clearing user')
            setUser(null)
            return
          }
          
          const userData = {
            id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload.sub || '',
            username: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload.name || payload.unique_name || '',
            email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload.email || '',
            role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || 'User',
            createdAt: payload.iat ? new Date(payload.iat * 1000).toISOString() : new Date().toISOString()
          }
          console.log('Extracted user data:', userData)
          console.log('Setting user from JWT:', userData)
          setUser(userData)
          console.log('User set successfully')
        } catch (error) {
          console.error('Failed to decode JWT token:', error)
          setUser(null)
        }
      } else {
        console.log('No token found, user not logged in')
        setUser(null)
      }
    } catch (error) {
      console.error('Auth check failed:', error)
      setUser(null)
    } finally {
      setIsLoading(false)
    }
  }

  const login = async (username: string, password: string): Promise<boolean> => {
    try {
      console.log('Attempting login with:', username)
      const response = await api.login(username, password)
      console.log('Login response:', response)
      
      // Prüfe ob die Antwort success: true enthält
      if (response && response.success === true) {
        // Use user data from response if available, otherwise check auth status
        if (response.user) {
          setUser(response.user)
        } else {
          await checkAuthStatus()
        }
        return true
      } else {
        console.error('Login failed: Invalid response', response)
        return false
      }
    } catch (error) {
      console.error('Login failed:', error)
      return false
    }
  }

  const register = async (username: string, password: string, email: string): Promise<boolean> => {
    try {
      console.log('Attempting registration with:', username)
      const response = await api.register(username, password, email)
      console.log('Registration response:', response)
      
      if (response && response.success === true) {
        return true
      } else {
        console.error('Registration failed:', response?.message || 'Unknown error')
        return false
      }
    } catch (error) {
      console.error('Registration failed:', error)
      return false
    }
  }

  const logout = async (): Promise<void> => {
    try {
      await api.logout()
      setUser(null)
    } catch (error) {
      console.error('Logout failed:', error)
    }
  }

  return (
    <AuthContext.Provider value={{ user, login, register, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

function getCookie(name: string): string | null {
  console.log('All cookies:', document.cookie)
  const value = `; ${document.cookie}`
  const parts = value.split(`; ${name}=`)
  if (parts.length === 2) {
    const cookieValue = parts.pop()?.split(';').shift() || null
    console.log(`Cookie ${name}:`, cookieValue)
    return cookieValue
  }
  console.log(`Cookie ${name} not found`)
  return null
}
