import { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'
import { LogOut, Users, MessageSquare, Activity, BarChart3, Settings, ArrowLeft } from 'lucide-react'

interface AdminStats {
  totalUsers: number
  activeUsers: number
  totalConversations: number
  totalMessages: number
  activeStreams: number
  modelUsage: Record<string, number>
}

export default function AdminPage() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const [stats, setStats] = useState<AdminStats | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    loadStats()
    const interval = setInterval(loadStats, 30000) // Refresh every 30 seconds
    return () => clearInterval(interval)
  }, [])

  const loadStats = async () => {
    try {
      const response = await fetch('/api/admin/stats')
      if (response.ok) {
        const data = await response.json()
        setStats(data)
      }
    } catch (error) {
      console.error('Failed to load admin stats:', error)
    } finally {
      setIsLoading(false)
    }
  }

  const handleLogout = async () => {
    await logout()
  }

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200 px-4 py-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <button
              onClick={() => navigate('/chat')}
              className="inline-flex items-center px-3 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Chat
            </button>
            <BarChart3 className="h-6 w-6 text-blue-600" />
            <h1 className="text-xl font-semibold text-gray-900">Admin Dashboard</h1>
          </div>
          
          <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate('/admin/users')}
              className="flex items-center space-x-2 text-gray-600 hover:text-gray-900"
            >
              <Settings className="h-5 w-5" />
              <span>User Management</span>
            </button>
            
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 text-gray-600 hover:text-gray-900"
            >
              <LogOut className="h-5 w-5" />
              <span>Logout</span>
            </button>
          </div>
        </div>
      </header>

      <div className="max-w-7xl mx-auto px-4 py-8">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          {/* Total Users */}
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Users className="h-8 w-8 text-blue-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Total Users</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {stats?.totalUsers ?? 0}
                </p>
              </div>
            </div>
          </div>

          {/* Active Users */}
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Activity className="h-8 w-8 text-green-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Active Users</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {stats?.activeUsers ?? 0}
                </p>
              </div>
            </div>
          </div>

          {/* Total Conversations */}
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <MessageSquare className="h-8 w-8 text-purple-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Conversations</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {stats?.totalConversations ?? 0}
                </p>
              </div>
            </div>
          </div>

          {/* Active Streams */}
          <div className="bg-white rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Activity className="h-8 w-8 text-orange-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500">Active Streams</p>
                <p className="text-2xl font-semibold text-gray-900">
                  {stats?.activeStreams ?? 0}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Model Usage */}
        {stats?.modelUsage && Object.keys(stats.modelUsage).length > 0 && (
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-medium text-gray-900 mb-4">Model Usage</h3>
            <div className="space-y-3">
              {Object.entries(stats.modelUsage).map(([model, count]) => (
                <div key={model} className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700">{model}</span>
                  <span className="text-sm text-gray-500">{count} requests</span>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* System Health */}
        <div className="mt-8 bg-white rounded-lg shadow p-6">
          <h3 className="text-lg font-medium text-gray-900 mb-4">System Health</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
              <span className="text-sm font-medium text-gray-700">Database</span>
              <span className="text-sm text-green-600 font-medium">Healthy</span>
            </div>
            <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
              <span className="text-sm font-medium text-gray-700">LM Studio</span>
              <span className="text-sm text-green-600 font-medium">Connected</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
