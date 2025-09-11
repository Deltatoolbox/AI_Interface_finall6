import { useState, useEffect } from 'react'
import { ArrowLeft, Activity, Cpu, HardDrive, MemoryStick, Wifi, RefreshCw, AlertTriangle, CheckCircle, XCircle, Clock } from 'lucide-react'
import { api } from '../api'

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

export default function HealthMonitoringPage() {
  const [systemHealth, setSystemHealth] = useState<SystemHealth | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [lastUpdate, setLastUpdate] = useState<Date | null>(null)
  const [isRefreshing, setIsRefreshing] = useState(false)

  useEffect(() => {
    loadSystemHealth()
    
    // Auto-refresh every 30 seconds
    const interval = setInterval(loadSystemHealth, 30000)
    return () => clearInterval(interval)
  }, [])

  const loadSystemHealth = async () => {
    setIsRefreshing(true)
    setError('')

    try {
      const health = await api.getSystemHealth()
      setSystemHealth(health)
      setLastUpdate(new Date())
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load system health')
    } finally {
      setIsLoading(false)
      setIsRefreshing(false)
    }
  }

  const handleRefresh = () => {
    loadSystemHealth()
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
        return <AlertTriangle className="h-4 w-4 text-yellow-600 dark:text-yellow-400" />
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
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <div className="max-w-7xl mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <button
                onClick={() => window.history.back()}
                className="p-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 transition-colors"
              >
                <ArrowLeft className="h-5 w-5" />
              </button>
              <div>
                <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100 flex items-center">
                  <Activity className="h-5 w-5 mr-2 text-blue-600" />
                  System Health Monitoring
                </h1>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  Real-time system performance and service status
                </p>
              </div>
            </div>
            <div className="flex items-center space-x-4">
              {lastUpdate && (
                <div className="text-sm text-gray-500 dark:text-gray-400">
                  Last updated: {lastUpdate.toLocaleTimeString()}
                </div>
              )}
              <button
                onClick={handleRefresh}
                disabled={isRefreshing}
                className="flex items-center space-x-2 px-4 py-2 bg-blue-600 dark:bg-blue-700 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 disabled:opacity-50 transition-colors"
              >
                <RefreshCw className={`h-4 w-4 ${isRefreshing ? 'animate-spin' : ''}`} />
                <span>Refresh</span>
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-7xl mx-auto px-4 py-6">
        {error && (
          <div className="mb-6 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
            <p className="text-red-800 dark:text-red-200">{error}</p>
          </div>
        )}

        {systemHealth && (
          <>
            {/* Overall System Status */}
            <div className="mb-6 bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
                  Overall System Status
                </h2>
                <div className="flex items-center space-x-2">
                  {getStatusIcon(systemHealth.status)}
                  <span className={`font-medium ${getStatusColor(systemHealth.status)}`}>
                    {systemHealth.status}
                  </span>
                </div>
              </div>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                Last checked: {formatDate(systemHealth.timestamp)}
              </p>
            </div>

            {/* System Metrics */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-6">
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
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

              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
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

              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
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

              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
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

              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
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

              <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
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

            {/* Service Status */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
                Service Status
              </h2>
              <div className="space-y-3">
                {systemHealth.services.map((service) => (
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
          </>
        )}
      </div>
    </div>
  )
}
