import { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'
import { ArrowLeft, Bug, Key, Cookie, Database, Monitor, RefreshCw, Copy, Check } from 'lucide-react'

interface JWTToken {
  header: any
  payload: any
  signature: string
}

export default function DebugPage() {
  const { user, isLoading, logout } = useAuth()
  const navigate = useNavigate()
  const [jwtToken, setJwtToken] = useState<JWTToken | null>(null)
  const [copiedStates, setCopiedStates] = useState<Record<string, boolean>>({})
  const [systemInfo, setSystemInfo] = useState<any>(null)
  const [apiStatus, setApiStatus] = useState<any>(null)

  useEffect(() => {
    loadJWTToken()
    loadSystemInfo()
    loadApiStatus()
  }, [])

  const loadJWTToken = () => {
    const token = localStorage.getItem('access_token') || document.cookie
      .split('; ')
      .find(row => row.startsWith('access_token='))
      ?.split('=')[1]

    if (token) {
      try {
        const parts = token.split('.')
        const header = JSON.parse(atob(parts[0]))
        const payload = JSON.parse(atob(parts[1]))
        setJwtToken({
          header,
          payload,
          signature: parts[2]
        })
      } catch (error) {
        console.error('Error parsing JWT:', error)
      }
    }
  }

  const loadSystemInfo = () => {
    setSystemInfo({
      userAgent: navigator.userAgent,
      language: navigator.language,
      platform: navigator.platform,
      cookieEnabled: navigator.cookieEnabled,
      onLine: navigator.onLine,
      screenResolution: `${screen.width}x${screen.height}`,
      colorDepth: screen.colorDepth,
      timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      timestamp: new Date().toISOString()
    })
  }

  const loadApiStatus = async () => {
    try {
      // Test multiple endpoints to see which ones work
      const endpoints = [
        '/api/admin/users',
        '/api/conversations', 
        '/api/models',
        '/health'
      ]
      
      let workingEndpoint = null
      let lastError = null
      
      for (const endpoint of endpoints) {
        try {
          const response = await fetch(endpoint)
          
          if (response.ok) {
            workingEndpoint = endpoint
            const contentType = response.headers.get('content-type')
            
            if (contentType && contentType.includes('application/json')) {
              try {
                const data = await response.json()
                setApiStatus({
                  status: response.status,
                  statusText: response.statusText,
                  ok: true,
                  url: response.url,
                  timestamp: new Date().toISOString(),
                  data: data,
                  testedEndpoint: endpoint
                })
                return
              } catch (jsonError) {
                // Continue to next endpoint
                lastError = `JSON Parse Error: ${jsonError instanceof Error ? jsonError.message : 'Unknown error'}`
                continue
              }
            } else {
              const text = await response.text()
              setApiStatus({
                status: response.status,
                statusText: response.statusText,
                ok: true,
                url: response.url,
                timestamp: new Date().toISOString(),
                data: text,
                testedEndpoint: endpoint
              })
              return
            }
          } else {
            lastError = `HTTP ${response.status}: ${response.statusText}`
          }
        } catch (endpointError) {
          lastError = endpointError instanceof Error ? endpointError.message : 'Unknown error'
          continue
        }
      }
      
      // If no endpoint worked, show the last error
      setApiStatus({
        error: `All endpoints failed. Last error: ${lastError}`,
        timestamp: new Date().toISOString(),
        status: 'Connection Failed',
        ok: false,
        testedEndpoints: endpoints
      })
    } catch (error) {
      setApiStatus({
        error: error instanceof Error ? error.message : 'Unknown error',
        timestamp: new Date().toISOString(),
        status: 'Connection Failed',
        ok: false
      })
    }
  }

  const copyToClipboard = async (text: string, key: string) => {
    try {
      await navigator.clipboard.writeText(text)
      setCopiedStates(prev => ({ ...prev, [key]: true }))
      setTimeout(() => {
        setCopiedStates(prev => ({ ...prev, [key]: false }))
      }, 2000)
    } catch (error) {
      console.error('Failed to copy:', error)
    }
  }

  const formatJSON = (obj: any) => {
    return JSON.stringify(obj, null, 2)
  }

  const handleLogout = async () => {
    await logout()
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 p-8">
      <div className="max-w-6xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <div className="flex items-center space-x-4">
            <button
              onClick={() => navigate('/settings')}
              className="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Settings
            </button>
            <Bug className="h-8 w-8 text-red-600 dark:text-red-400" />
            <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">Debug Tools</h1>
          </div>
          <button
            onClick={handleLogout}
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700"
          >
            Logout
          </button>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Auth State */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 flex items-center">
                <Key className="h-5 w-5 mr-2 text-blue-600 dark:text-blue-400" />
                Auth State
              </h2>
              <button
                onClick={() => copyToClipboard(formatJSON({ isLoading, user }), 'auth')}
                className="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                {copiedStates.auth ? <Check className="h-4 w-4 text-green-500" /> : <Copy className="h-4 w-4" />}
              </button>
            </div>
            <div className="space-y-3">
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">isLoading:</span>
                <span className={`px-2 py-1 rounded text-xs font-mono ${
                  isLoading 
                    ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-300' 
                    : 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-300'
                }`}>
                  {isLoading ? 'true' : 'false'}
                </span>
              </div>
              <div>
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">user:</span>
                <pre className="mt-2 p-3 bg-gray-100 dark:bg-gray-700 rounded text-xs font-mono text-gray-800 dark:text-gray-200 overflow-x-auto">
                  {user ? formatJSON(user) : 'null'}
                </pre>
              </div>
            </div>
          </div>

          {/* JWT Token */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 flex items-center">
                <Key className="h-5 w-5 mr-2 text-purple-600 dark:text-purple-400" />
                JWT Token
              </h2>
              <button
                onClick={loadJWTToken}
                className="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <RefreshCw className="h-4 w-4" />
              </button>
            </div>
            {jwtToken ? (
              <div className="space-y-3">
                <div>
                  <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Header:</span>
                  <pre className="mt-1 p-2 bg-gray-100 dark:bg-gray-700 rounded text-xs font-mono text-gray-800 dark:text-gray-200 overflow-x-auto">
                    {formatJSON(jwtToken.header)}
                  </pre>
                </div>
                <div>
                  <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Payload:</span>
                  <pre className="mt-1 p-2 bg-gray-100 dark:bg-gray-700 rounded text-xs font-mono text-gray-800 dark:text-gray-200 overflow-x-auto">
                    {formatJSON(jwtToken.payload)}
                  </pre>
                </div>
                <div>
                  <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Signature:</span>
                  <code className="mt-1 block p-2 bg-gray-100 dark:bg-gray-700 rounded text-xs font-mono text-gray-800 dark:text-gray-200 break-all">
                    {jwtToken.signature}
                  </code>
                </div>
              </div>
            ) : (
              <p className="text-gray-500 dark:text-gray-400">No JWT token found</p>
            )}
          </div>

          {/* Cookies */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 flex items-center">
                <Cookie className="h-5 w-5 mr-2 text-orange-600 dark:text-orange-400" />
                Cookies
              </h2>
              <button
                onClick={() => copyToClipboard(document.cookie, 'cookies')}
                className="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                {copiedStates.cookies ? <Check className="h-4 w-4 text-green-500" /> : <Copy className="h-4 w-4" />}
              </button>
            </div>
            <div className="space-y-2">
              {document.cookie ? (
                document.cookie.split('; ').map((cookie, index) => (
                  <div key={index} className="flex items-center justify-between p-2 bg-gray-100 dark:bg-gray-700 rounded">
                    <code className="text-xs font-mono text-gray-800 dark:text-gray-200 break-all">{cookie}</code>
                    <button
                      onClick={() => copyToClipboard(cookie, `cookie-${index}`)}
                      className="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                    >
                      {copiedStates[`cookie-${index}`] ? <Check className="h-3 w-3 text-green-500" /> : <Copy className="h-3 w-3" />}
                    </button>
                  </div>
                ))
              ) : (
                <p className="text-gray-500 dark:text-gray-400">No cookies found</p>
              )}
            </div>
          </div>

          {/* System Info */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 flex items-center">
                <Monitor className="h-5 w-5 mr-2 text-green-600 dark:text-green-400" />
                System Info
              </h2>
              <button
                onClick={loadSystemInfo}
                className="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <RefreshCw className="h-4 w-4" />
              </button>
            </div>
            {systemInfo && (
              <div className="space-y-2">
                {Object.entries(systemInfo).map(([key, value]) => (
                  <div key={key} className="flex justify-between items-center py-1">
                    <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{key}:</span>
                    <span className="text-sm text-gray-600 dark:text-gray-400 font-mono">{String(value)}</span>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* API Status */}
          <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6 lg:col-span-2">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 flex items-center">
                <Database className="h-5 w-5 mr-2 text-indigo-600 dark:text-indigo-400" />
                API Status
              </h2>
              <button
                onClick={loadApiStatus}
                className="p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <RefreshCw className="h-4 w-4" />
              </button>
            </div>
            {apiStatus && (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Status:</span>
                    <span className={`px-2 py-1 rounded text-xs font-mono ${
                      apiStatus.ok 
                        ? 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-300'
                        : 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-300'
                    }`}>
                      {apiStatus.status || 'Error'}
                    </span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Status Text:</span>
                    <span className="text-sm text-gray-600 dark:text-gray-400">{apiStatus.statusText || 'N/A'}</span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium text-gray-700 dark:text-gray-300">OK:</span>
                    <span className={`px-2 py-1 rounded text-xs font-mono ${
                      apiStatus.ok 
                        ? 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-300'
                        : 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-300'
                    }`}>
                      {apiStatus.ok ? 'true' : 'false'}
                    </span>
                  </div>
                </div>
                <div className="space-y-2">
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium text-gray-700 dark:text-gray-300">URL:</span>
                    <span className="text-sm text-gray-600 dark:text-gray-400 font-mono">{apiStatus.url || 'N/A'}</span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Timestamp:</span>
                    <span className="text-sm text-gray-600 dark:text-gray-400 font-mono">{apiStatus.timestamp}</span>
                  </div>
                  {apiStatus.testedEndpoint && (
                    <div className="flex justify-between items-center">
                      <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Tested Endpoint:</span>
                      <span className="text-sm text-blue-600 dark:text-blue-400 font-mono">{apiStatus.testedEndpoint}</span>
                    </div>
                  )}
                  {apiStatus.testedEndpoints && (
                    <div className="mt-2">
                      <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Tested Endpoints:</span>
                      <div className="mt-1 space-y-1">
                        {apiStatus.testedEndpoints.map((endpoint: string, index: number) => (
                          <div key={index} className="text-xs text-gray-600 dark:text-gray-400 font-mono bg-gray-100 dark:bg-gray-700 px-2 py-1 rounded">
                            {endpoint}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                  {apiStatus.error && (
                    <div className="flex justify-between items-center">
                      <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Error:</span>
                      <span className="text-sm text-red-600 dark:text-red-400 font-mono">{apiStatus.error}</span>
                    </div>
                  )}
                  {apiStatus.data && (
                    <div className="mt-4">
                      <span className="text-sm font-medium text-gray-700 dark:text-gray-300">Response Data:</span>
                      <pre className="mt-2 p-3 bg-gray-100 dark:bg-gray-700 rounded text-xs font-mono text-gray-800 dark:text-gray-200 overflow-x-auto">
                        {formatJSON(apiStatus.data)}
                      </pre>
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}
