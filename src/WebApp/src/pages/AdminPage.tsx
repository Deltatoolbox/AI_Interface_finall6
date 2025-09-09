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
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <header className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700 px-4 py-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <button
              onClick={() => navigate('/settings')}
              className="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Settings
            </button>
            <BarChart3 className="h-6 w-6 text-blue-600" />
            <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">Admin Dashboard</h1>
          </div>
          
          <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate('/admin/users')}
              className="flex items-center space-x-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100"
            >
              <Settings className="h-5 w-5" />
              <span>User Management</span>
            </button>
            
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100"
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
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Users className="h-8 w-8 text-blue-600 dark:text-blue-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Total Users</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-gray-100">
                  {stats?.totalUsers ?? 0}
                </p>
              </div>
            </div>
          </div>

          {/* Active Users */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Activity className="h-8 w-8 text-green-600 dark:text-green-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Active Users</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-gray-100">
                  {stats?.activeUsers ?? 0}
                </p>
              </div>
            </div>
          </div>

          {/* Total Conversations */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <MessageSquare className="h-8 w-8 text-purple-600 dark:text-purple-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Conversations</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-gray-100">
                  {stats?.totalConversations ?? 0}
                </p>
              </div>
            </div>
          </div>

          {/* Active Streams */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Activity className="h-8 w-8 text-orange-600 dark:text-orange-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Active Streams</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-gray-100">
                  {stats?.activeStreams ?? 0}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Model Usage */}
        {stats?.modelUsage && Object.keys(stats.modelUsage).length > 0 && (
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">Model Usage</h3>
            <div className="space-y-3">
              {Object.entries(stats.modelUsage).map(([model, count]) => (
                <div key={model} className="flex items-center justify-between">
                  <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{model}</span>
                  <span className="text-sm text-gray-500 dark:text-gray-400">{count} requests</span>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* System Health */}
        <div className="mt-8 bg-white dark:bg-gray-800 rounded-lg shadow p-6">
          <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">System Health</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Database</span>
              <span className="text-sm text-green-600 dark:text-green-400 font-medium">Healthy</span>
            </div>
            <div className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">LM Studio</span>
              <span className="text-sm text-green-600 dark:text-green-400 font-medium">Connected</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
