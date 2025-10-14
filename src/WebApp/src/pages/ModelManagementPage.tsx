import { useState, useEffect } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { useNavigate } from 'react-router-dom'
import { ArrowLeft, Cpu, Settings, RefreshCw, CheckCircle, AlertCircle, XCircle } from 'lucide-react'

interface Model {
  id: string
  object: string
  created: number
  ownedBy: string
}

interface ModelStatus {
  isConnected: boolean
  lastCheck: string
  errorMessage?: string
}

export default function ModelManagementPage() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const [models, setModels] = useState<Model[]>([])
  const [selectedModel, setSelectedModel] = useState<string>('')
  const [modelStatus, setModelStatus] = useState<ModelStatus | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isRefreshing, setIsRefreshing] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  useEffect(() => {
    loadModels()
    loadModelStatus()
  }, [])

  const loadModels = async () => {
    try {
      const response = await fetch('/api/models', {
        credentials: 'include'
      })
      if (response.ok) {
        const data = await response.json()
        const chatModels = (data.data || []).filter((model: any) => !model.id.includes('embedding'))
        setModels(chatModels.map((model: any) => ({
          id: model.id,
          object: model.object,
          created: model.created || 0,
          ownedBy: model.owned_by
        })))
        
        if (chatModels.length > 0 && !selectedModel) {
          setSelectedModel(chatModels[0].id)
        }
      }
    } catch (error) {
      console.error('Failed to load models:', error)
    }
  }

  const loadModelStatus = async () => {
    try {
      const response = await fetch('/api/models/status', {
        credentials: 'include'
      })
      if (response.ok) {
        const data = await response.json()
        setModelStatus({
          isConnected: data.connected || false,
          lastCheck: data.lastCheck || new Date().toISOString(),
          errorMessage: data.errorMessage
        })
      }
    } catch (error) {
      console.error('Failed to load model status:', error)
      setModelStatus({
        isConnected: false,
        lastCheck: new Date().toISOString(),
        errorMessage: 'Failed to connect to LM Studio'
      })
    } finally {
      setIsLoading(false)
    }
  }

  const handleRefresh = async () => {
    setIsRefreshing(true)
    await Promise.all([loadModels(), loadModelStatus()])
    setIsRefreshing(false)
  }

  const handleSaveModel = async () => {
    if (!selectedModel) return
    
    setIsSaving(true)
    try {
      const response = await fetch('/api/admin/models/set-default', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({ modelId: selectedModel })
      })
      
      if (response.ok) {
        alert('Default model updated successfully!')
      } else {
        alert('Failed to update default model')
      }
    } catch (error) {
      console.error('Failed to save model:', error)
      alert('Failed to save model settings')
    } finally {
      setIsSaving(false)
    }
  }

  const handleLogout = async () => {
    await logout()
  }

  const getStatusIcon = (isConnected: boolean) => {
    if (isConnected) {
      return <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400" />
    } else {
      return <XCircle className="h-5 w-5 text-red-600 dark:text-red-400" />
    }
  }


  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading model management...</p>
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
              onClick={() => navigate('/admin')}
              className="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Admin Dashboard
            </button>
            <Cpu className="h-6 w-6 text-blue-600" />
            <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">Model Management</h1>
          </div>
          
          <div className="flex items-center space-x-4">
            <button
              onClick={handleRefresh}
              disabled={isRefreshing}
              className="flex items-center space-x-2 px-3 py-2 bg-blue-600 dark:bg-blue-700 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 disabled:opacity-50 transition-colors"
            >
              <RefreshCw className={`h-4 w-4 ${isRefreshing ? 'animate-spin' : ''}`} />
              <span>Refresh</span>
            </button>
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100"
            >
              <span>Logout</span>
            </button>
          </div>
        </div>
      </header>

      <div className="max-w-4xl mx-auto px-4 py-8">
        {/* Connection Status */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6 mb-8">
          <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
            <Cpu className="h-5 w-5 mr-2 text-blue-600" />
            LM Studio Connection Status
          </h3>
          
          <div className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
            <div className="flex items-center space-x-3">
              {modelStatus && getStatusIcon(modelStatus.isConnected)}
              <div>
                <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">
                  LM Studio API
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-400">
                  Last checked: {modelStatus ? new Date(modelStatus.lastCheck).toLocaleString() : 'Never'}
                </p>
                {modelStatus?.errorMessage && (
                  <p className="text-xs text-red-600 dark:text-red-400 mt-1">
                    {modelStatus.errorMessage}
                  </p>
                )}
              </div>
            </div>
            <span className={`px-2 py-1 rounded-full text-xs font-medium ${modelStatus?.isConnected ? 'bg-green-100 dark:bg-green-900/20 text-green-800 dark:text-green-200' : 'bg-red-100 dark:bg-red-900/20 text-red-800 dark:text-red-200'}`}>
              {modelStatus?.isConnected ? 'Connected' : 'Disconnected'}
            </span>
          </div>
        </div>

        {/* Model Selection */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6 mb-8">
          <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
            <Settings className="h-5 w-5 mr-2 text-purple-600" />
            Default Model Configuration
          </h3>
          
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Select Default Model
              </label>
              <div className="relative">
                <select
                  value={selectedModel}
                  onChange={(e) => setSelectedModel(e.target.value)}
                  className="appearance-none bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg px-3 py-2 pr-8 text-sm text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent w-full"
                >
                  {models.map((model) => (
                    <option key={model.id} value={model.id}>
                      {model.id} ({model.ownedBy})
                    </option>
                  ))}
                </select>
                <div className="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
                  <ArrowLeft className="h-4 w-4 text-gray-400 dark:text-gray-500 rotate-90" />
                </div>
              </div>
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                This model will be used for all new conversations. Make sure LM Studio is running with this model loaded.
              </p>
            </div>

            <div className="flex justify-end">
              <button
                onClick={handleSaveModel}
                disabled={isSaving || !selectedModel}
                className="flex items-center space-x-2 px-4 py-2 bg-blue-600 dark:bg-blue-700 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 disabled:opacity-50 transition-colors"
              >
                {isSaving ? (
                  <RefreshCw className="h-4 w-4 animate-spin" />
                ) : (
                  <CheckCircle className="h-4 w-4" />
                )}
                <span>{isSaving ? 'Saving...' : 'Save Default Model'}</span>
              </button>
            </div>
          </div>
        </div>

        {/* Available Models */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
          <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
            <Cpu className="h-5 w-5 mr-2 text-green-600" />
            Available Models ({models.length})
          </h3>
          
          {models.length === 0 ? (
            <div className="text-center py-8">
              <AlertCircle className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-500 dark:text-gray-400">No models found. Make sure LM Studio is running and has models loaded.</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {models.map((model) => (
                <div
                  key={model.id}
                  className={`p-4 border rounded-lg transition-colors ${
                    selectedModel === model.id
                      ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
                      : 'border-gray-200 dark:border-gray-600 bg-gray-50 dark:bg-gray-700'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <h4 className="text-sm font-medium text-gray-900 dark:text-gray-100">
                        {model.id}
                      </h4>
                      <p className="text-xs text-gray-500 dark:text-gray-400">
                        {model.ownedBy}
                      </p>
                    </div>
                    {selectedModel === model.id && (
                      <CheckCircle className="h-5 w-5 text-blue-600" />
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Instructions */}
        <div className="mt-8 bg-blue-50 dark:bg-blue-900/20 rounded-lg p-6">
          <h3 className="text-lg font-medium text-blue-900 dark:text-blue-100 mb-2">
            📋 Instructions
          </h3>
          <div className="text-sm text-blue-800 dark:text-blue-200 space-y-2">
            <p>1. <strong>Start LM Studio</strong> and load your desired model</p>
            <p>2. <strong>Select the model</strong> from the dropdown above</p>
            <p>3. <strong>Save the configuration</strong> to set it as default</p>
            <p>4. <strong>All new conversations</strong> will use this model</p>
            <p className="text-xs text-blue-600 dark:text-blue-300 mt-3">
              Note: Model changes require LM Studio to be restarted with the new model loaded.
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
