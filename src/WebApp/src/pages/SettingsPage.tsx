import { useState } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { useAppearance } from '../contexts/AppearanceContext'
import { useNavigate } from 'react-router-dom'
import { Settings, User, Shield, LogOut, ArrowLeft, Palette, Database, Key, Trash2, Lock, AlertTriangle } from 'lucide-react'
import { api } from '../api'

export default function SettingsPage() {
  const { user, logout } = useAuth()
  const { settings, updateSettings, isDarkMode } = useAppearance()
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState('profile')
  
  // Privacy Settings State
  const [showDeleteChatsModal, setShowDeleteChatsModal] = useState(false)
  const [showChangePasswordModal, setShowChangePasswordModal] = useState(false)
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  })
  const [isDeletingChats, setIsDeletingChats] = useState(false)
  const [isChangingPassword, setIsChangingPassword] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const handleLogout = async () => {
    await logout()
  }

  // Privacy Settings Handlers
  const handleDeleteAllChats = async () => {
    setIsDeletingChats(true)
    setError('')
    try {
      const response = await fetch('/api/conversations', {
        method: 'DELETE',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json'
        }
      })
      
      if (response.ok) {
        setSuccess('All your chats have been deleted successfully.')
        setShowDeleteChatsModal(false)
        // Refresh the page to update the conversation list
        window.location.reload()
      } else {
        const errorData = await response.json()
        setError(errorData.message || 'Failed to delete chats')
      }
    } catch (err) {
      setError('An error occurred while deleting chats')
    } finally {
      setIsDeletingChats(false)
    }
  }

  const handleChangePassword = async () => {
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setError('New passwords do not match')
      return
    }
    
    if (passwordData.newPassword.length < 6) {
      setError('New password must be at least 6 characters long')
      return
    }

    setIsChangingPassword(true)
    setError('')
    try {
      const response = await fetch('/api/auth/change-password', {
        method: 'POST',
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          currentPassword: passwordData.currentPassword,
          newPassword: passwordData.newPassword
        })
      })
      
      if (response.ok) {
        setSuccess('Password changed successfully!')
        setShowChangePasswordModal(false)
        setPasswordData({ currentPassword: '', newPassword: '', confirmPassword: '' })
      } else {
        const errorData = await response.json()
        setError(errorData.message || 'Failed to change password')
      }
    } catch (err) {
      setError('An error occurred while changing password')
    } finally {
      setIsChangingPassword(false)
    }
  }

  // Export/Import Handlers
  const handleExportConversations = async () => {
    try {
      setError('')
      const conversations = await api.exportConversations()
      
      // Create a blob with the JSON data
      const jsonString = JSON.stringify(conversations, null, 2)
      const blob = new Blob([jsonString], { type: 'application/json' })
      
      // Create download link
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `conversations-export-${new Date().toISOString().split('T')[0]}.json`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      URL.revokeObjectURL(url)
      
      setSuccess(`Successfully exported ${conversations.length} conversations`)
    } catch (err) {
      setError('Failed to export conversations')
      console.error('Export error:', err)
    }
  }

  const handleImportFile = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (!file) return

    try {
      setError('')
      const text = await file.text()
      const conversations = JSON.parse(text)
      
      if (!Array.isArray(conversations)) {
        setError('Invalid file format. Please select a valid JSON export file.')
        return
      }

      const result = await api.importConversations(conversations)
      
      if (result.errors && result.errors.length > 0) {
        setError(`Import completed with ${result.errors.length} errors. Check console for details.`)
        console.error('Import errors:', result.errors)
      } else {
        setSuccess(`Successfully imported ${result.importedCount} conversations`)
      }
      
      // Clear the file input
      event.target.value = ''
      
      // Refresh the page to show imported conversations
      setTimeout(() => {
        window.location.reload()
      }, 2000)
      
    } catch (err) {
      setError('Failed to import conversations. Please check the file format.')
      console.error('Import error:', err)
      event.target.value = ''
    }
  }

  const tabs = [
    { id: 'profile', label: 'Profile', icon: User },
    { id: 'appearance', label: 'Appearance', icon: Palette },
    { id: 'privacy', label: 'Privacy', icon: Shield },
    ...(user?.role === 'Admin' ? [{ id: 'admin', label: 'Admin Dashboard', icon: Database }] : [])
  ]

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <header className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700 px-4 py-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <button
              onClick={() => navigate('/chat')}
              className="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Chat
            </button>
            <Settings className="h-6 w-6 text-blue-600" />
            <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">Settings</h1>
          </div>
          
          <div className="flex items-center space-x-4">
            <span className="text-sm text-gray-600 dark:text-gray-400">Welcome, {user?.username}</span>
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

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="bg-white dark:bg-gray-800 shadow rounded-lg">
          <div className="px-4 py-5 sm:p-6">
            <div className="flex">
              {/* Sidebar */}
              <div className="w-64 pr-8">
                <nav className="space-y-1">
                  {tabs.map((tab) => {
                    const Icon = tab.icon
                    return (
                      <button
                        key={tab.id}
                        onClick={() => setActiveTab(tab.id)}
                        className={`w-full flex items-center px-3 py-2 text-sm font-medium rounded-md ${
                          activeTab === tab.id
                            ? 'bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300'
                            : 'text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100 hover:bg-gray-50 dark:hover:bg-gray-700'
                        }`}
                      >
                        <Icon className="h-5 w-5 mr-3" />
                        {tab.label}
                      </button>
                    )
                  })}
                </nav>
              </div>

              {/* Content */}
              <div className="flex-1">
                {activeTab === 'profile' && (
                  <div>
                    <h2 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-6">Profile Settings</h2>
                    <div className="space-y-6">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Username</label>
                        <div className="mt-1 flex items-center">
                          <input
                            type="text"
                            value={user?.username || ''}
                            disabled
                            className="block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm bg-gray-50 dark:bg-gray-600 text-gray-500 dark:text-gray-400 sm:text-sm"
                          />
                          <span className="ml-2 text-sm text-gray-500 dark:text-gray-400">(Cannot be changed)</span>
                        </div>
                      </div>
                      
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Email</label>
                        <div className="mt-1">
                          <input
                            type="email"
                            value={user?.email || ''}
                            disabled
                            className="block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm bg-gray-50 dark:bg-gray-600 text-gray-500 dark:text-gray-400 sm:text-sm"
                          />
                        </div>
                      </div>
                      
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Role</label>
                        <div className="mt-1">
                          <div className="flex items-center">
                            <Shield className="h-5 w-5 text-blue-600 mr-2" />
                            <span className="text-sm font-medium text-gray-900 dark:text-gray-100">{user?.role}</span>
                          </div>
                        </div>
                      </div>
                      
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Member Since</label>
                        <div className="mt-1">
                          <span className="text-sm text-gray-500 dark:text-gray-400">
                            {user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : 'Unknown'}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                )}

                {activeTab === 'appearance' && (
                  <div>
                    <h2 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-6">Appearance Settings</h2>
                    <div className="space-y-6">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">Theme</label>
                        <div className="space-y-2">
                          <label className="flex items-center">
                            <input 
                              type="radio" 
                              name="theme" 
                              value="light" 
                              checked={settings.theme === 'light'}
                              onChange={(e) => updateSettings({ theme: e.target.value as 'light' | 'dark' | 'auto' })}
                              className="h-4 w-4 text-blue-600" 
                            />
                            <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Light</span>
                          </label>
                          <label className="flex items-center">
                            <input 
                              type="radio" 
                              name="theme" 
                              value="dark" 
                              checked={settings.theme === 'dark'}
                              onChange={(e) => updateSettings({ theme: e.target.value as 'light' | 'dark' | 'auto' })}
                              className="h-4 w-4 text-blue-600" 
                            />
                            <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Dark</span>
                          </label>
                          <label className="flex items-center">
                            <input 
                              type="radio" 
                              name="theme" 
                              value="auto" 
                              checked={settings.theme === 'auto'}
                              onChange={(e) => updateSettings({ theme: e.target.value as 'light' | 'dark' | 'auto' })}
                              className="h-4 w-4 text-blue-600" 
                            />
                            <span className="ml-2 text-sm text-gray-700 dark:text-gray-300">Auto (System)</span>
                          </label>
                        </div>
                        {settings.theme === 'auto' && (
                          <p className="mt-2 text-xs text-gray-500 dark:text-gray-400">
                            Currently using: {isDarkMode ? 'Dark' : 'Light'} mode
                          </p>
                        )}
                      </div>
                      
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">Font Size</label>
                        <select 
                          value={settings.fontSize}
                          onChange={(e) => updateSettings({ fontSize: e.target.value as 'small' | 'medium' | 'large' })}
                          className="block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                        >
                          <option value="small">Small</option>
                          <option value="medium">Medium</option>
                          <option value="large">Large</option>
                        </select>
                      </div>
                    </div>
                  </div>
                )}

                {activeTab === 'privacy' && (
                  <div>
                    <h2 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-6">Privacy & Security Settings</h2>
                    
                    {/* Success/Error Messages */}
                    {success && (
                      <div className="mb-4 p-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-700 rounded-md">
                        <p className="text-sm text-green-800 dark:text-green-200">{success}</p>
                      </div>
                    )}
                    {error && (
                      <div className="mb-4 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-700 rounded-md">
                        <p className="text-sm text-red-800 dark:text-red-200">{error}</p>
                      </div>
                    )}

                    <div className="space-y-6">
                      {/* Security Section */}
                      <div className="border-b border-gray-200 dark:border-gray-700 pb-6">
                        <h3 className="text-md font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
                          <Lock className="h-5 w-5 mr-2 text-blue-600" />
                          Security
                        </h3>
                        <div className="space-y-4">
                          <div className="flex items-center justify-between">
                            <div>
                              <h4 className="text-sm font-medium text-gray-900 dark:text-gray-100">Change Password</h4>
                              <p className="text-sm text-gray-500 dark:text-gray-400">Update your account password</p>
                            </div>
                            <button
                              onClick={() => setShowChangePasswordModal(true)}
                              className="px-4 py-2 bg-blue-600 dark:bg-blue-700 text-white text-sm font-medium rounded-md hover:bg-blue-700 dark:hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            >
                              Change Password
                            </button>
                          </div>
                        </div>
                      </div>

                      {/* Export/Import Section */}
                      <div className="border-b border-gray-200 dark:border-gray-700 pb-6">
                        <h3 className="text-md font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
                          <Database className="h-5 w-5 mr-2 text-green-600" />
                          Data Export/Import
                        </h3>
                        <div className="space-y-4">
                          <div className="flex items-center justify-between">
                            <div>
                              <h4 className="text-sm font-medium text-gray-900 dark:text-gray-100">Export Conversations</h4>
                              <p className="text-sm text-gray-500 dark:text-gray-400">Download all your conversations as JSON file</p>
                            </div>
                            <button
                              onClick={handleExportConversations}
                              className="px-4 py-2 bg-green-600 dark:bg-green-700 text-white text-sm font-medium rounded-md hover:bg-green-700 dark:hover:bg-green-600 focus:outline-none focus:ring-2 focus:ring-green-500"
                            >
                              Export
                            </button>
                          </div>
                          
                          <div className="flex items-center justify-between">
                            <div>
                              <h4 className="text-sm font-medium text-gray-900 dark:text-gray-100">Import Conversations</h4>
                              <p className="text-sm text-gray-500 dark:text-gray-400">Upload and restore conversations from JSON file</p>
                            </div>
                            <div className="flex items-center space-x-2">
                              <input
                                type="file"
                                accept=".json"
                                onChange={handleImportFile}
                                className="hidden"
                                id="import-file"
                              />
                              <label
                                htmlFor="import-file"
                                className="px-4 py-2 bg-blue-600 dark:bg-blue-700 text-white text-sm font-medium rounded-md hover:bg-blue-700 dark:hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer"
                              >
                                Choose File
                              </label>
                            </div>
                          </div>
                        </div>
                      </div>

                      {/* Data Management Section */}
                      <div className="border-b border-gray-200 dark:border-gray-700 pb-6">
                        <h3 className="text-md font-medium text-gray-900 dark:text-gray-100 mb-4 flex items-center">
                          <Trash2 className="h-5 w-5 mr-2 text-red-600" />
                          Data Management
                        </h3>
                        <div className="space-y-4">
                          <div className="flex items-center justify-between">
                            <div>
                              <h4 className="text-sm font-medium text-gray-900 dark:text-gray-100">Delete All Chats</h4>
                              <p className="text-sm text-gray-500 dark:text-gray-400">Permanently delete all your chat conversations</p>
                            </div>
                            <button
                              onClick={() => setShowDeleteChatsModal(true)}
                              className="px-4 py-2 bg-red-600 dark:bg-red-700 text-white text-sm font-medium rounded-md hover:bg-red-700 dark:hover:bg-red-600 focus:outline-none focus:ring-2 focus:ring-red-500"
                            >
                              Delete All Chats
                            </button>
                          </div>
                        </div>
                      </div>

                    </div>
                  </div>
                )}

                {activeTab === 'admin' && user?.role === 'Admin' && (
                  <div>
                    <h2 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-6">Admin Dashboard</h2>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                      <button
                        onClick={() => navigate('/admin')}
                        className="p-6 bg-blue-50 dark:bg-blue-900/20 rounded-lg border border-blue-200 dark:border-blue-700 hover:bg-blue-100 dark:hover:bg-blue-900/30 transition-colors"
                      >
                        <Database className="h-8 w-8 text-blue-600 dark:text-blue-400 mb-3" />
                        <h3 className="text-lg font-medium text-blue-900 dark:text-blue-100">System Dashboard</h3>
                        <p className="text-sm text-blue-700 dark:text-blue-300 mt-2">View system statistics and monitoring</p>
                      </button>
                      
                      <button
                        onClick={() => navigate('/admin/users')}
                        className="p-6 bg-green-50 dark:bg-green-900/20 rounded-lg border border-green-200 dark:border-green-700 hover:bg-green-100 dark:hover:bg-green-900/30 transition-colors"
                      >
                        <User className="h-8 w-8 text-green-600 dark:text-green-400 mb-3" />
                        <h3 className="text-lg font-medium text-green-900 dark:text-green-100">User Management</h3>
                        <p className="text-sm text-green-700 dark:text-green-300 mt-2">Manage users and permissions</p>
                      </button>
                      
                      <button
                        onClick={() => navigate('/debug')}
                        className="p-6 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg border border-yellow-200 dark:border-yellow-700 hover:bg-yellow-100 dark:hover:bg-yellow-900/30 transition-colors"
                      >
                        <Key className="h-8 w-8 text-yellow-600 dark:text-yellow-400 mb-3" />
                        <h3 className="text-lg font-medium text-yellow-900 dark:text-yellow-100">Debug Tools</h3>
                        <p className="text-sm text-yellow-700 dark:text-yellow-300 mt-2">Access debugging and diagnostics</p>
                      </button>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Delete All Chats Confirmation Modal */}
      {showDeleteChatsModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white dark:bg-gray-800">
            <div className="mt-3 text-center">
              <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100 dark:bg-red-900/20">
                <AlertTriangle className="h-6 w-6 text-red-600 dark:text-red-400" />
              </div>
              <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mt-4">Delete All Chats</h3>
              <div className="mt-2 px-7 py-3">
                <p className="text-sm text-gray-500 dark:text-gray-400">
                  Are you sure you want to delete all your chat conversations? This action cannot be undone.
                </p>
              </div>
              <div className="flex justify-center space-x-4 mt-4">
                <button
                  onClick={() => setShowDeleteChatsModal(false)}
                  className="px-4 py-2 bg-gray-300 dark:bg-gray-600 text-gray-800 dark:text-gray-200 text-sm font-medium rounded-md hover:bg-gray-400 dark:hover:bg-gray-500"
                >
                  Cancel
                </button>
                <button
                  onClick={handleDeleteAllChats}
                  disabled={isDeletingChats}
                  className="px-4 py-2 bg-red-600 dark:bg-red-700 text-white text-sm font-medium rounded-md hover:bg-red-700 dark:hover:bg-red-600 disabled:opacity-50"
                >
                  {isDeletingChats ? 'Deleting...' : 'Delete All'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Change Password Modal */}
      {showChangePasswordModal && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-50">
          <div className="relative top-20 mx-auto p-5 border w-96 shadow-lg rounded-md bg-white dark:bg-gray-800">
            <div className="mt-3">
              <div className="flex items-center justify-center h-12 w-12 rounded-full bg-blue-100 dark:bg-blue-900/20 mx-auto mb-4">
                <Lock className="h-6 w-6 text-blue-600 dark:text-blue-400" />
              </div>
              <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 text-center mb-4">Change Password</h3>
              
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Current Password</label>
                  <input
                    type="password"
                    value={passwordData.currentPassword}
                    onChange={(e) => setPasswordData({...passwordData, currentPassword: e.target.value})}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Enter current password"
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">New Password</label>
                  <input
                    type="password"
                    value={passwordData.newPassword}
                    onChange={(e) => setPasswordData({...passwordData, newPassword: e.target.value})}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Enter new password"
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Confirm New Password</label>
                  <input
                    type="password"
                    value={passwordData.confirmPassword}
                    onChange={(e) => setPasswordData({...passwordData, confirmPassword: e.target.value})}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Confirm new password"
                  />
                </div>
              </div>
              
              <div className="flex justify-center space-x-4 mt-6">
                <button
                  onClick={() => {
                    setShowChangePasswordModal(false)
                    setPasswordData({ currentPassword: '', newPassword: '', confirmPassword: '' })
                    setError('')
                  }}
                  className="px-4 py-2 bg-gray-300 dark:bg-gray-600 text-gray-800 dark:text-gray-200 text-sm font-medium rounded-md hover:bg-gray-400 dark:hover:bg-gray-500"
                >
                  Cancel
                </button>
                <button
                  onClick={handleChangePassword}
                  disabled={isChangingPassword}
                  className="px-4 py-2 bg-blue-600 dark:bg-blue-700 text-white text-sm font-medium rounded-md hover:bg-blue-700 dark:hover:bg-blue-600 disabled:opacity-50"
                >
                  {isChangingPassword ? 'Changing...' : 'Change Password'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
