import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import { AppearanceProvider } from './contexts/AppearanceContext'
import { ProtectedRoute } from './components/ProtectedRoute'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import ChatPage from './pages/ChatPage'
import SettingsPage from './pages/SettingsPage'
import AdminPage from './pages/AdminPage'
import UserManagementPage from './pages/UserManagementPage'
import DebugPage from './pages/DebugPage'
import SearchPage from './pages/SearchPage'
import SharedConversationPage from './pages/SharedConversationPage'
import BackupManagementPage from './pages/BackupManagementPage'
import AuditLogPage from './pages/AuditLogPage'
import WebhookManagementPage from './pages/WebhookManagementPage'
import IntegrationManagementPage from './pages/IntegrationManagementPage'
import ModelManagementPage from './pages/ModelManagementPage'

function App() {
  return (
    <AppearanceProvider>
      <AuthProvider>
        <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/debug" element={<DebugPage />} />
          <Route 
            path="/chat" 
            element={
              <ProtectedRoute>
                <ChatPage />
              </ProtectedRoute>
            } 
          />
          <Route 
            path="/settings" 
            element={
              <ProtectedRoute>
                <SettingsPage />
              </ProtectedRoute>
            } 
          />
        <Route 
          path="/search" 
          element={
            <ProtectedRoute>
              <SearchPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/shared/:shareId" 
          element={<SharedConversationPage />} 
        />
        <Route 
          path="/backups" 
          element={
            <ProtectedRoute>
              <BackupManagementPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/admin/webhooks" 
          element={
            <ProtectedRoute requireAdmin>
              <WebhookManagementPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/admin/integrations" 
          element={
            <ProtectedRoute requireAdmin>
              <IntegrationManagementPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/admin/models" 
          element={
            <ProtectedRoute requireAdmin>
              <ModelManagementPage />
            </ProtectedRoute>
          } 
        />
        <Route 
          path="/audit" 
          element={
            <ProtectedRoute requireAdmin>
              <AuditLogPage />
            </ProtectedRoute>
          } 
        />
          <Route 
            path="/admin" 
            element={
              <ProtectedRoute requireAdmin>
                <AdminPage />
              </ProtectedRoute>
            } 
          />
          <Route 
            path="/admin/users" 
            element={
              <ProtectedRoute requireAdmin>
                <UserManagementPage />
              </ProtectedRoute>
            } 
          />
          <Route path="/" element={<Navigate to="/chat" replace />} />
        </Routes>
        </div>
      </AuthProvider>
    </AppearanceProvider>
  )
}

export default App
