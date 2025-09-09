import { useAuth } from '../contexts/AuthContext'

export default function DebugPage() {
  const { user, isLoading } = useAuth()

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-4xl mx-auto">
        <h1 className="text-3xl font-bold mb-8">Debug Auth Status</h1>
        
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h2 className="text-xl font-semibold mb-4">Auth State</h2>
          <div className="space-y-2">
            <p><strong>isLoading:</strong> {isLoading ? 'true' : 'false'}</p>
            <p><strong>user:</strong> {user ? JSON.stringify(user, null, 2) : 'null'}</p>
          </div>
        </div>

        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <h2 className="text-xl font-semibold mb-4">Cookies</h2>
          <p><strong>All cookies:</strong> {document.cookie}</p>
        </div>

        <div className="bg-white rounded-lg shadow p-6">
          <h2 className="text-xl font-semibold mb-4">JWT Token Debug</h2>
          <div id="jwt-debug">
            <p>Loading...</p>
          </div>
        </div>
      </div>
    </div>
  )
}
