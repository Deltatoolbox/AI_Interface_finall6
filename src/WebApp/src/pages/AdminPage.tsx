import { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'
import { LogOut, Users, MessageSquare, Settings, ArrowLeft, Database, Server, TrendingUp, Activity, AlertCircle, BarChart3, Cpu, MemoryStick, HardDrive, Wifi, Clock, CheckCircle, XCircle, RefreshCw } from 'lucide-react'
import { api } from '../api'

interface AdminStats {
  totalUsers: number
  totalConversations: number
  totalMessages: number
}

interface SystemHealth {
  status: string
  timestamp: string
  metrics: SystemMetrics
  services: ServiceStatus[]
}

interface SystemMetrics {
  cpuUsage: number
  memoryUsage: number
  diskUsage: number
  activeConnections: number
  totalRequests: number
  responseTime: number
}

interface ServiceStatus {
  name: string
  status: string
  lastCheck: string
  errorMessage?: string
}

export default function AdminPage() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const [stats, setStats] = useState<AdminStats | null>(null)
  const [systemHealth, setSystemHealth] = useState<SystemHealth | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [lastUpdated, setLastUpdated] = useState<Date>(new Date())
  const [isRefreshing, setIsRefreshing] = useState(false)

  useEffect(() => {
    loadStats()
    const interval = setInterval(loadStats, 30000) // Refresh every 30 seconds
    return () => clearInterval(interval)
  }, [])

  const loadStats = async () => {
    setIsRefreshing(true)
    try {
      // Load admin stats and system health in parallel
      const [statsResponse, healthResponse] = await Promise.all([
        fetch('/api/admin/stats'),
        api.getSystemHealth()
      ])
      
      if (statsResponse.ok) {
        const statsData = await statsResponse.json()
        setStats({
          totalUsers: statsData.totalUsers,
          totalConversations: statsData.totalConversations,
          totalMessages: statsData.totalMessages
        })
      }
      
      setSystemHealth(healthResponse)
      setLastUpdated(new Date())
    } catch (error) {
      console.error('Failed to load admin stats:', error)
      // Set error state
      setStats({
        totalUsers: 0,
        totalConversations: 0,
        totalMessages: 0
      })
    } finally {
      setIsLoading(false)
      setIsRefreshing(false)
    }
  }



  const handleLogout = async () => {
    await logout()
  }

  const formatBytes = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleString()
  }

  const getStatusColor = (status: string): string => {
    switch (status.toLowerCase()) {
      case 'healthy':
        return 'text-green-600 dark:text-green-400'
      case 'degraded':
        return 'text-yellow-600 dark:text-yellow-400'
      case 'unhealthy':
        return 'text-red-600 dark:text-red-400'
      default:
        return 'text-gray-600 dark:text-gray-400'
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'healthy':
        return <CheckCircle className="h-4 w-4 text-green-600 dark:text-green-400" />
      case 'degraded':
        return <AlertCircle className="h-4 w-4 text-yellow-600 dark:text-yellow-400" />
      case 'unhealthy':
        return <XCircle className="h-4 w-4 text-red-600 dark:text-red-400" />
      default:
        return <Clock className="h-4 w-4 text-gray-600 dark:text-gray-400" />
    }
  }

  const getServiceStatusColor = (status: string): string => {
    switch (status.toLowerCase()) {
      case 'healthy':
        return 'bg-green-100 dark:bg-green-900/20 text-green-800 dark:text-green-200'
      case 'unhealthy':
        return 'bg-red-100 dark:bg-red-900/20 text-red-800 dark:text-red-200'
      case 'error':
        return 'bg-red-100 dark:bg-red-900/20 text-red-800 dark:text-red-200'
      default:
        return 'bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200'
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
              onClick={loadStats}
              disabled={isRefreshing}
              className="flex items-center space-x-2 px-3 py-2 bg-blue-600 dark:bg-blue-700 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 disabled:opacity-50 transition-colors"
            >
              <RefreshCw className={`h-4 w-4 ${isRefreshing ? 'animate-spin' : ''}`} />
              <span>Refresh</span>
            </button>
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
                <p className={`text-lg font-semibold ${getStatusColor(systemHealth?.status || 'unknown')}`}>
                  {systemHealth?.status || 'Unknown'}
                </p>
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  {systemHealth ? formatDate(systemHealth.timestamp) : 'Not available'}
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* System Metrics */}
        {systemHealth && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center space-x-2">
                  <Cpu className="h-5 w-5 text-blue-600" />
                  <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">CPU Usage</h3>
                </div>
              </div>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {systemHealth.metrics.cpuUsage.toFixed(1)}%
              </div>
              <div className="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2 mt-2">
                <div 
                  className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                  style={{ width: `${Math.min(systemHealth.metrics.cpuUsage, 100)}%` }}
                ></div>
              </div>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center space-x-2">
                  <MemoryStick className="h-5 w-5 text-green-600" />
                  <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">Memory Usage</h3>
                </div>
              </div>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {formatBytes(systemHealth.metrics.memoryUsage)}
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Working Set
              </div>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center space-x-2">
                  <HardDrive className="h-5 w-5 text-purple-600" />
                  <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">Disk Usage</h3>
                </div>
              </div>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {formatBytes(systemHealth.metrics.diskUsage)}
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Used Space
              </div>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center space-x-2">
                  <Wifi className="h-5 w-5 text-orange-600" />
                  <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">Active Connections</h3>
                </div>
              </div>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {systemHealth.metrics.activeConnections}
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Current
              </div>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center space-x-2">
                  <Activity className="h-5 w-5 text-indigo-600" />
                  <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">Total Requests</h3>
                </div>
              </div>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {systemHealth.metrics.totalRequests.toLocaleString()}
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Lifetime
              </div>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center space-x-2">
                  <Clock className="h-5 w-5 text-teal-600" />
                  <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">Response Time</h3>
                </div>
              </div>
              <div className="text-2xl font-bold text-gray-900 dark:text-gray-100">
                {systemHealth.metrics.responseTime.toFixed(1)}ms
              </div>
              <div className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                Average
              </div>
            </div>
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-1 gap-8">
          {/* System Health */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
              <Database className="h-5 w-5 mr-2 text-blue-600" />
              Service Status
            </h3>
            <div className="space-y-3">
              {systemHealth?.services.map((service) => (
                <div
                  key={service.name}
                  className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg"
                >
                  <div className="flex items-center space-x-3">
                    {getStatusIcon(service.status)}
                    <div>
                      <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">
                        {service.name}
                      </h3>
                      <p className="text-xs text-gray-500 dark:text-gray-400">
                        Last checked: {formatDate(service.lastCheck)}
                      </p>
                      {service.errorMessage && (
                        <p className="text-xs text-red-600 dark:text-red-400 mt-1">
                          {service.errorMessage}
                        </p>
                      )}
                    </div>
                  </div>
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${getServiceStatusColor(service.status)}`}>
                    {service.status}
                  </span>
                </div>
              ))}
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
