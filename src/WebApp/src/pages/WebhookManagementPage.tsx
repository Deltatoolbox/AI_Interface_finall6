import { useState, useEffect } from 'react'
import { ArrowLeft, Plus, Edit, Trash2, TestTube, Eye, Webhook, Clock, CheckCircle, XCircle, Info, AlertCircle, BookOpen } from 'lucide-react'
import { api } from '../api'

interface Webhook {
  id: string;
  name: string;
  url: string;
  events: string[];
  isActive: boolean;
  retryCount: number;
  timeoutSeconds: number;
  createdAt: string;
  description?: string;
}

interface WebhookDelivery {
  id: string;
  eventType: string;
  status: string;
  attempts: number;
  responseCode?: number;
  createdAt: string;
  deliveredAt?: string;
  errorMessage?: string;
}

// Available webhook events with descriptions
const WEBHOOK_EVENTS = [
  {
    id: 'user.created',
    name: 'User Created',
    description: 'Triggered when a new user registers',
    category: 'User Management',
    payload: {
      userId: 'string',
      username: 'string',
      email: 'string',
      createdAt: 'datetime'
    }
  },
  {
    id: 'user.login',
    name: 'User Login',
    description: 'Triggered when a user logs in',
    category: 'User Management',
    payload: {
      userId: 'string',
      username: 'string',
      loginAt: 'datetime',
      ipAddress: 'string'
    }
  },
  {
    id: 'conversation.created',
    name: 'Conversation Created',
    description: 'Triggered when a new conversation is created',
    category: 'Chat',
    payload: {
      conversationId: 'string',
      userId: 'string',
      title: 'string',
      model: 'string',
      createdAt: 'datetime'
    }
  },
  {
    id: 'conversation.updated',
    name: 'Conversation Updated',
    description: 'Triggered when a conversation is modified',
    category: 'Chat',
    payload: {
      conversationId: 'string',
      userId: 'string',
      title: 'string',
      updatedAt: 'datetime'
    }
  },
  {
    id: 'message.sent',
    name: 'Message Sent',
    description: 'Triggered when a user sends a message',
    category: 'Chat',
    payload: {
      messageId: 'string',
      conversationId: 'string',
      userId: 'string',
      content: 'string',
      role: 'string',
      createdAt: 'datetime'
    }
  },
  {
    id: 'message.received',
    name: 'Message Received',
    description: 'Triggered when AI responds to a message',
    category: 'Chat',
    payload: {
      messageId: 'string',
      conversationId: 'string',
      content: 'string',
      model: 'string',
      createdAt: 'datetime'
    }
  },
  {
    id: 'backup.created',
    name: 'Backup Created',
    description: 'Triggered when a backup is created',
    category: 'System',
    payload: {
      backupId: 'string',
      fileName: 'string',
      size: 'number',
      createdAt: 'datetime'
    }
  },
  {
    id: 'backup.restored',
    name: 'Backup Restored',
    description: 'Triggered when a backup is restored',
    category: 'System',
    payload: {
      backupId: 'string',
      fileName: 'string',
      restoredAt: 'datetime'
    }
  },
  {
    id: 'webhook.created',
    name: 'Webhook Created',
    description: 'Triggered when a new webhook is created',
    category: 'System',
    payload: {
      webhookId: 'string',
      name: 'string',
      url: 'string',
      events: 'array',
      createdAt: 'datetime'
    }
  },
  {
    id: 'webhook.failed',
    name: 'Webhook Failed',
    description: 'Triggered when a webhook delivery fails',
    category: 'System',
    payload: {
      webhookId: 'string',
      eventType: 'string',
      attempts: 'number',
      errorMessage: 'string',
      failedAt: 'datetime'
    }
  }
];

export default function WebhookManagementPage() {
  const [webhooks, setWebhooks] = useState<Webhook[]>([]);
  const [deliveries, setDeliveries] = useState<WebhookDelivery[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showTestModal, setShowTestModal] = useState(false);
  const [showDeliveriesModal, setShowDeliveriesModal] = useState(false);
  const [showEventsModal, setShowEventsModal] = useState(false);
  const [selectedWebhook, setSelectedWebhook] = useState<Webhook | null>(null);
  const [testResult, setTestResult] = useState<any>(null);
  const [isProcessing, setIsProcessing] = useState(false);

  const [formData, setFormData] = useState({
    name: '',
    url: '',
    secret: '',
    events: '',
    retryCount: 3,
    timeoutSeconds: 30,
    description: ''
  });

  useEffect(() => {
    loadWebhooks();
  }, []);

  const loadWebhooks = async () => {
    setIsLoading(true);
    setError('');
    try {
      const response = await api.getWebhooks();
      setWebhooks(response);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load webhooks');
    } finally {
      setIsLoading(false);
    }
  };

  const loadDeliveries = async (webhookId: string) => {
    try {
      const response = await api.getWebhookDeliveries(webhookId);
      setDeliveries(response);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load deliveries');
    }
  };

  const handleCreateWebhook = async () => {
    setIsProcessing(true);
    try {
      const events = formData.events.split(',').map(e => e.trim()).filter(e => e);
      await api.createWebhook({
        ...formData,
        events: events
      });
      setSuccess('Webhook created successfully');
      setShowCreateModal(false);
      resetForm();
      loadWebhooks();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create webhook');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleUpdateWebhook = async () => {
    if (!selectedWebhook) return;
    
    setIsProcessing(true);
    try {
      const events = formData.events.split(',').map(e => e.trim()).filter(e => e);
      await api.updateWebhook(selectedWebhook.id, {
        ...formData,
        events: events
      });
      setSuccess('Webhook updated successfully');
      setShowEditModal(false);
      resetForm();
      loadWebhooks();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update webhook');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleDeleteWebhook = async (id: string) => {
    if (!confirm('Are you sure you want to delete this webhook?')) return;
    
    try {
      await api.deleteWebhook(id);
      setSuccess('Webhook deleted successfully');
      loadWebhooks();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete webhook');
    }
  };

  const handleTestWebhook = async () => {
    if (!selectedWebhook) return;
    
    setIsProcessing(true);
    try {
      const result = await api.testWebhook({
        url: selectedWebhook.url,
        secret: formData.secret,
        eventType: 'webhook.test',
        payload: {
          test: true,
          timestamp: new Date().toISOString(),
          message: 'This is a test webhook from LM Gateway'
        }
      });
      setTestResult(result);
      setSuccess('Webhook test completed');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to test webhook');
    } finally {
      setIsProcessing(false);
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      url: '',
      secret: '',
      events: '',
      retryCount: 3,
      timeoutSeconds: 30,
      description: ''
    });
    setSelectedWebhook(null);
    setTestResult(null);
  };

  const openEditModal = (webhook: Webhook) => {
    setSelectedWebhook(webhook);
    setFormData({
      name: webhook.name,
      url: webhook.url,
      secret: '', // Don't show existing secret
      events: webhook.events.join(', '),
      retryCount: webhook.retryCount,
      timeoutSeconds: webhook.timeoutSeconds,
      description: webhook.description || ''
    });
    setShowEditModal(true);
  };

  const openTestModal = (webhook: Webhook) => {
    setSelectedWebhook(webhook);
    setFormData({
      name: webhook.name,
      url: webhook.url,
      secret: '',
      events: webhook.events.join(', '),
      retryCount: webhook.retryCount,
      timeoutSeconds: webhook.timeoutSeconds,
      description: webhook.description || ''
    });
    setShowTestModal(true);
  };

  const openDeliveriesModal = async (webhook: Webhook) => {
    setSelectedWebhook(webhook);
    await loadDeliveries(webhook.id);
    setShowDeliveriesModal(true);
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'delivered':
        return <CheckCircle className="h-4 w-4 text-green-600" />;
      case 'pending':
        return <Clock className="h-4 w-4 text-yellow-600" />;
      case 'failed':
        return <XCircle className="h-4 w-4 text-red-600" />;
      default:
        return <Clock className="h-4 w-4 text-gray-600" />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'delivered':
        return 'bg-green-100 text-green-800';
      case 'pending':
        return 'bg-yellow-100 text-yellow-800';
      case 'failed':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading webhooks...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <header className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700 px-4 py-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <button
              onClick={() => window.history.back()}
              className="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 text-sm font-medium rounded-md text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back
            </button>
            <Webhook className="h-6 w-6 text-blue-600" />
            <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">Webhook Management</h1>
          </div>
          
          <div className="flex items-center space-x-4">
            <button
              onClick={() => setShowEventsModal(true)}
              className="flex items-center space-x-2 px-3 py-2 bg-gray-600 dark:bg-gray-700 text-white rounded-lg hover:bg-gray-700 dark:hover:bg-gray-600 transition-colors"
            >
              <BookOpen className="h-4 w-4" />
              <span>Event Reference</span>
            </button>
            <button
              onClick={() => setShowCreateModal(true)}
              className="flex items-center space-x-2 px-3 py-2 bg-blue-600 dark:bg-blue-700 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 transition-colors"
            >
              <Plus className="h-4 w-4" />
              <span>Create Webhook</span>
            </button>
          </div>
        </div>
      </header>

      <div className="max-w-7xl mx-auto px-4 py-8">
        {/* Alerts */}
        {error && (
          <div className="mb-6 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
            <div className="flex">
              <AlertCircle className="h-5 w-5 text-red-600 dark:text-red-400" />
              <div className="ml-3">
                <p className="text-sm text-red-800 dark:text-red-200">{error}</p>
              </div>
            </div>
          </div>
        )}

        {success && (
          <div className="mb-6 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg p-4">
            <div className="flex">
              <CheckCircle className="h-5 w-5 text-green-600 dark:text-green-400" />
              <div className="ml-3">
                <p className="text-sm text-green-800 dark:text-green-200">{success}</p>
              </div>
            </div>
          </div>
        )}

        {/* Webhooks List */}
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
          <div className="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
            <h2 className="text-lg font-medium text-gray-900 dark:text-gray-100">Webhooks ({webhooks.length})</h2>
          </div>
          
          {webhooks.length === 0 ? (
            <div className="text-center py-12">
              <Webhook className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-2">No webhooks configured</h3>
              <p className="text-gray-500 dark:text-gray-400 mb-4">Create your first webhook to start receiving events</p>
              <button
                onClick={() => setShowCreateModal(true)}
                className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                <Plus className="h-4 w-4 mr-2" />
                Create Webhook
              </button>
            </div>
          ) : (
            <div className="divide-y divide-gray-200 dark:divide-gray-700">
              {webhooks.map((webhook) => (
                <div key={webhook.id} className="px-6 py-4">
                  <div className="flex items-center justify-between">
                    <div className="flex-1">
                      <div className="flex items-center space-x-3">
                        <h3 className="text-sm font-medium text-gray-900 dark:text-gray-100">{webhook.name}</h3>
                        <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                          webhook.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                        }`}>
                          {webhook.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </div>
                      <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{webhook.url}</p>
                      <div className="flex items-center space-x-4 mt-2">
                        <span className="text-xs text-gray-500 dark:text-gray-400">
                          Events: {webhook.events.length}
                        </span>
                        <span className="text-xs text-gray-500 dark:text-gray-400">
                          Retries: {webhook.retryCount}
                        </span>
                        <span className="text-xs text-gray-500 dark:text-gray-400">
                          Timeout: {webhook.timeoutSeconds}s
                        </span>
                        <span className="text-xs text-gray-500 dark:text-gray-400">
                          Created: {new Date(webhook.createdAt).toLocaleDateString()}
                        </span>
                      </div>
                    </div>
                    
                    <div className="flex items-center space-x-2">
                      <button
                        onClick={() => openDeliveriesModal(webhook)}
                        className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                        title="View Deliveries"
                      >
                        <Eye className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => openTestModal(webhook)}
                        className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                        title="Test Webhook"
                      >
                        <TestTube className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => openEditModal(webhook)}
                        className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                        title="Edit Webhook"
                      >
                        <Edit className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => handleDeleteWebhook(webhook.id)}
                        className="p-2 text-gray-400 hover:text-red-600 dark:hover:text-red-400"
                        title="Delete Webhook"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Create Webhook Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">Create Webhook</h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Name *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  placeholder="My Webhook"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  URL *
                </label>
                <input
                  type="url"
                  value={formData.url}
                  onChange={(e) => setFormData({...formData, url: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  placeholder="https://example.com/webhook"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Secret *
                </label>
                <input
                  type="password"
                  value={formData.secret}
                  onChange={(e) => setFormData({...formData, secret: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  placeholder="Your webhook secret"
                />
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Used to verify webhook authenticity
                </p>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Events *
                </label>
                <input
                  type="text"
                  value={formData.events}
                  onChange={(e) => setFormData({...formData, events: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  placeholder="user.created, conversation.created"
                />
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  Comma-separated list of events. Click "Event Reference" for available events.
                </p>
              </div>
              
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Retry Count
                  </label>
                  <input
                    type="number"
                    min="0"
                    max="10"
                    value={formData.retryCount}
                    onChange={(e) => setFormData({...formData, retryCount: parseInt(e.target.value)})}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Timeout (seconds)
                  </label>
                  <input
                    type="number"
                    min="5"
                    max="300"
                    value={formData.timeoutSeconds}
                    onChange={(e) => setFormData({...formData, timeoutSeconds: parseInt(e.target.value)})}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  />
                </div>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Description
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({...formData, description: e.target.value})}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  placeholder="Optional description"
                />
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setShowCreateModal(false)}
                className="px-4 py-2 text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700"
              >
                Cancel
              </button>
              <button
                onClick={handleCreateWebhook}
                disabled={isProcessing || !formData.name || !formData.url || !formData.secret || !formData.events}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isProcessing ? 'Creating...' : 'Create Webhook'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Event Reference Modal */}
      {showEventsModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-4xl max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100">Webhook Event Reference</h3>
              <button
                onClick={() => setShowEventsModal(false)}
                className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <XCircle className="h-6 w-6" />
              </button>
            </div>
            
            <div className="space-y-6">
              {Object.entries(WEBHOOK_EVENTS.reduce((acc, event) => {
                if (!acc[event.category]) acc[event.category] = [];
                acc[event.category].push(event);
                return acc;
              }, {} as Record<string, typeof WEBHOOK_EVENTS>)).map(([category, events]) => (
                <div key={category}>
                  <h4 className="text-md font-medium text-gray-900 dark:text-gray-100 mb-3 border-b border-gray-200 dark:border-gray-700 pb-2">
                    {category}
                  </h4>
                  <div className="space-y-3">
                    {events.map((event) => (
                      <div key={event.id} className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="flex items-center space-x-2 mb-2">
                              <code className="text-sm font-mono bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200 px-2 py-1 rounded">
                                {event.id}
                              </code>
                              <span className="text-sm font-medium text-gray-900 dark:text-gray-100">
                                {event.name}
                              </span>
                            </div>
                            <p className="text-sm text-gray-600 dark:text-gray-400 mb-3">
                              {event.description}
                            </p>
                            <div>
                              <p className="text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">Payload:</p>
                              <pre className="text-xs bg-gray-100 dark:bg-gray-800 p-2 rounded overflow-x-auto">
{JSON.stringify(event.payload, null, 2)}
                              </pre>
                            </div>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
            
            <div className="mt-6 p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
              <div className="flex items-start space-x-2">
                <Info className="h-5 w-5 text-blue-600 dark:text-blue-400 mt-0.5" />
                <div className="text-sm text-blue-800 dark:text-blue-200">
                  <p className="font-medium mb-1">Webhook Headers</p>
                  <p>All webhooks include these headers:</p>
                  <ul className="list-disc list-inside mt-1 space-y-1">
                    <li><code>X-Webhook-Signature</code> - HMAC signature for verification</li>
                    <li><code>X-Webhook-Event</code> - The event type that triggered the webhook</li>
                    <li><code>X-Webhook-Timestamp</code> - Unix timestamp of when the webhook was sent</li>
                    <li><code>Content-Type: application/json</code> - The payload format</li>
                  </ul>
                </div>
              </div>
            </div>
            
            <div className="flex justify-end mt-6">
              <button
                onClick={() => setShowEventsModal(false)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Edit Webhook Modal */}
      {showEditModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">Edit Webhook</h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Name *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  URL *
                </label>
                <input
                  type="url"
                  value={formData.url}
                  onChange={(e) => setFormData({...formData, url: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Secret *
                </label>
                <input
                  type="password"
                  value={formData.secret}
                  onChange={(e) => setFormData({...formData, secret: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  placeholder="Enter new secret or leave blank to keep current"
                />
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Events *
                </label>
                <input
                  type="text"
                  value={formData.events}
                  onChange={(e) => setFormData({...formData, events: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                />
              </div>
              
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Retry Count
                  </label>
                  <input
                    type="number"
                    min="0"
                    max="10"
                    value={formData.retryCount}
                    onChange={(e) => setFormData({...formData, retryCount: parseInt(e.target.value)})}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                    Timeout (seconds)
                  </label>
                  <input
                    type="number"
                    min="5"
                    max="300"
                    value={formData.timeoutSeconds}
                    onChange={(e) => setFormData({...formData, timeoutSeconds: parseInt(e.target.value)})}
                    className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  />
                </div>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Description
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({...formData, description: e.target.value})}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                />
              </div>
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setShowEditModal(false)}
                className="px-4 py-2 text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700"
              >
                Cancel
              </button>
              <button
                onClick={handleUpdateWebhook}
                disabled={isProcessing || !formData.name || !formData.url || !formData.events}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isProcessing ? 'Updating...' : 'Update Webhook'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Test Webhook Modal */}
      {showTestModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-2xl">
            <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100 mb-4">Test Webhook</h3>
            
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Secret (for testing)
                </label>
                <input
                  type="password"
                  value={formData.secret}
                  onChange={(e) => setFormData({...formData, secret: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-gray-100"
                  placeholder="Enter webhook secret"
                />
              </div>
              
              {testResult && (
                <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
                  <h4 className="text-sm font-medium text-gray-900 dark:text-gray-100 mb-2">Test Result</h4>
                  <pre className="text-xs text-gray-600 dark:text-gray-400 overflow-x-auto">
{JSON.stringify(testResult, null, 2)}
                  </pre>
                </div>
              )}
            </div>
            
            <div className="flex justify-end space-x-3 mt-6">
              <button
                onClick={() => setShowTestModal(false)}
                className="px-4 py-2 text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700"
              >
                Cancel
              </button>
              <button
                onClick={handleTestWebhook}
                disabled={isProcessing || !formData.secret}
                className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isProcessing ? 'Testing...' : 'Test Webhook'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Deliveries Modal */}
      {showDeliveriesModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-4xl max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-medium text-gray-900 dark:text-gray-100">
                Deliveries for {selectedWebhook?.name}
              </h3>
              <button
                onClick={() => setShowDeliveriesModal(false)}
                className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <XCircle className="h-6 w-6" />
              </button>
            </div>
            
            {deliveries.length === 0 ? (
              <div className="text-center py-8">
                <Clock className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                <p className="text-gray-500 dark:text-gray-400">No deliveries yet</p>
              </div>
            ) : (
              <div className="space-y-3">
                {deliveries.map((delivery) => (
                  <div key={delivery.id} className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
                    <div className="flex items-center justify-between mb-2">
                      <div className="flex items-center space-x-2">
                        {getStatusIcon(delivery.status)}
                        <span className="text-sm font-medium text-gray-900 dark:text-gray-100">
                          {delivery.eventType}
                        </span>
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(delivery.status)}`}>
                          {delivery.status}
                        </span>
                      </div>
                      <span className="text-xs text-gray-500 dark:text-gray-400">
                        {new Date(delivery.createdAt).toLocaleString()}
                      </span>
                    </div>
                    
                    <div className="text-xs text-gray-600 dark:text-gray-400">
                      <p>Attempts: {delivery.attempts}</p>
                      {delivery.responseCode && <p>Response Code: {delivery.responseCode}</p>}
                      {delivery.errorMessage && <p className="text-red-600 dark:text-red-400">Error: {delivery.errorMessage}</p>}
                    </div>
                  </div>
                ))}
              </div>
            )}
            
            <div className="flex justify-end mt-6">
              <button
                onClick={() => setShowDeliveriesModal(false)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}