import { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'
import { LogOut, Users, MessageSquare, Settings, ArrowLeft, Database, Server, TrendingUp, Activity, AlertCircle, BarChart3 } from 'lucide-react'

interface AdminStats {
  totalUsers: number
  totalConversations: number
  totalMessages: number
  systemHealth: {
    database: 'healthy' | 'warning' | 'error'
    lmStudio: 'connected' | 'disconnected' | 'error'
    api: 'healthy' | 'warning' | 'error'
  }
}

export default function AdminPage() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const [stats, setStats] = useState<AdminStats | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date())

  useEffect(() => {
    loadStats()
    const interval = setInterval(loadStats, 30000) // Refresh every 30 seconds
    return () => clearInterval(interval)
  }, [])

  const loadStats = async () => {
    try {
      // Load admin stats from the new endpoint
      const statsResponse = await fetch('/api/admin/stats')
      if (statsResponse.ok) {
        const statsData = await statsResponse.json()
        
        // Test LM Studio connection
        const lmStudioResponse = await fetch('/api/models')
        const lmStudioConnected = lmStudioResponse.ok
        
        // System health check
        const systemHealth = {
          database: 'healthy' as const,
          lmStudio: lmStudioConnected ? 'connected' as const : 'disconnected' as const,
          api: 'healthy' as const
        }

        setStats({
          totalUsers: statsData.totalUsers,
          totalConversations: statsData.totalConversations,
          totalMessages: statsData.totalMessages,
          systemHealth
        })
        
        setLastUpdated(new Date())
      } else {
        throw new Error('Failed to load admin stats')
      }
    } catch (error) {
      console.error('Failed to load admin stats:', error)
      // Set error state
      setStats({
        totalUsers: 0,
        totalConversations: 0,
        totalMessages: 0,
        systemHealth: {
          database: 'error',
          lmStudio: 'error',
          api: 'error'
        }
      })
    } finally {
      setIsLoading(false)
    }
  }



  const handleLogout = async () => {
    await logout()
  }

  const getHealthColor = (status: string) => {
    switch (status) {
      case 'healthy':
      case 'connected':
        return 'text-green-600 dark:text-green-400'
      case 'warning':
      case 'disconnected':
        return 'text-yellow-600 dark:text-yellow-400'
      case 'error':
        return 'text-red-600 dark:text-red-400'
      default:
        return 'text-gray-600 dark:text-gray-400'
    }
  }

  const getHealthIcon = (status: string) => {
    switch (status) {
      case 'healthy':
      case 'connected':
        return <Activity className="h-4 w-4 text-green-600 dark:text-green-400" />
      case 'warning':
      case 'disconnected':
        return <AlertCircle className="h-4 w-4 text-yellow-600 dark:text-yellow-400" />
      case 'error':
        return <AlertCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
      default:
        return <Activity className="h-4 w-4 text-gray-600 dark:text-gray-400" />
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading dashboard...</p>
        </div>
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
            <span className="text-sm text-gray-500 dark:text-gray-400">
              Last updated: {lastUpdated.toLocaleTimeString()}
            </span>
          </div>
          
          <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate('/admin/users')}
              className="flex items-center space-x-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100"
            >
              <Users className="h-5 w-5" />
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
        {/* Key Metrics */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          {/* Total Users */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6 hover:shadow-lg transition-shadow">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Users className="h-8 w-8 text-blue-600 dark:text-blue-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Total Users</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-gray-100">
                  {stats?.totalUsers ?? 0}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400">Registered users</p>
              </div>
            </div>
          </div>

          {/* Total Conversations */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6 hover:shadow-lg transition-shadow">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <MessageSquare className="h-8 w-8 text-purple-600 dark:text-purple-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Conversations</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-gray-100">
                  {stats?.totalConversations ?? 0}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400">Chat sessions</p>
              </div>
            </div>
          </div>

          {/* Total Messages */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6 hover:shadow-lg transition-shadow">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <TrendingUp className="h-8 w-8 text-green-600 dark:text-green-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500 dark:text-gray-400">Messages</p>
                <p className="text-2xl font-semibold text-gray-900 dark:text-gray-100">
                  {stats?.totalMessages ?? 0}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400">Total messages</p>
              </div>
            </div>
          </div>

          {/* System Status */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6 hover:shadow-lg transition-shadow">
            <div className="flex items-center">
              <div className="flex-shrink-0">
                <Server className="h-8 w-8 text-orange-600 dark:text-orange-400" />
              </div>
              <div className="ml-4">
                <p className="text-sm font-medium text-gray-500 dark:text-gray-400">System Status</p>
                <p className={`text-lg font-semibold ${getHealthColor(stats?.systemHealth.lmStudio || 'error')}`}>
                  {stats?.systemHealth.lmStudio === 'connected' ? 'Online' : 'Offline'}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400">LM Studio</p>
              </div>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-1 gap-8">
          {/* System Health */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
              <Database className="h-5 w-5 mr-2 text-blue-600" />
              System Health
            </h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                <div className="flex items-center">
                  {getHealthIcon(stats?.systemHealth.database || 'healthy')}
                  <span className="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">Database</span>
                </div>
                <span className={`text-sm font-medium ${getHealthColor(stats?.systemHealth.database || 'healthy')}`}>
                  {stats?.systemHealth.database || 'healthy'}
                </span>
              </div>
              
              <div className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                <div className="flex items-center">
                  {getHealthIcon(stats?.systemHealth.lmStudio || 'error')}
                  <span className="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">LM Studio</span>
                </div>
                <span className={`text-sm font-medium ${getHealthColor(stats?.systemHealth.lmStudio || 'error')}`}>
                  {stats?.systemHealth.lmStudio || 'error'}
                </span>
              </div>
              
              <div className="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg">
                <div className="flex items-center">
                  {getHealthIcon(stats?.systemHealth.api || 'healthy')}
                  <span className="ml-2 text-sm font-medium text-gray-700 dark:text-gray-300">API Gateway</span>
                </div>
                <span className={`text-sm font-medium ${getHealthColor(stats?.systemHealth.api || 'healthy')}`}>
                  {stats?.systemHealth.api || 'healthy'}
                </span>
              </div>
            </div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="mt-8 bg-white dark:bg-gray-800 rounded-lg shadow p-6">
          <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">Quick Actions</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <button
              onClick={() => navigate('/admin/users')}
              className="flex items-center justify-center p-4 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
            >
              <Users className="h-5 w-5 mr-2 text-blue-600" />
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Manage Users</span>
            </button>
            
            <button
              onClick={() => navigate('/debug')}
              className="flex items-center justify-center p-4 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
            >
              <Settings className="h-5 w-5 mr-2 text-purple-600" />
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">System Debug</span>
            </button>
            
            <button
              onClick={loadStats}
              className="flex items-center justify-center p-4 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
            >
              <Activity className="h-5 w-5 mr-2 text-green-600" />
              <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Refresh Data</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
