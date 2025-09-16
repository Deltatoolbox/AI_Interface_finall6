import { useState, useEffect } from 'react'
import { ArrowLeft, Settings, CheckCircle, XCircle, MessageSquare, Hash, Bot } from 'lucide-react'
import { api } from '../api'

interface SlackChannels {
  channels: string[];
  configured: boolean;
}

interface DiscordChannels {
  channels: string[];
  configured: boolean;
}

export default function IntegrationManagementPage() {
  const [slackChannels, setSlackChannels] = useState<SlackChannels>({ channels: [], configured: false });
  const [discordChannels, setDiscordChannels] = useState<DiscordChannels>({ channels: [], configured: false });
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [showSlackModal, setShowSlackModal] = useState(false);
  const [showDiscordModal, setShowDiscordModal] = useState(false);
  const [activeTab, setActiveTab] = useState('overview');
  const [isProcessing, setIsProcessing] = useState(false);

  const [slackForm, setSlackForm] = useState({
    channel: '',
    message: ''
  });

  const [discordForm, setDiscordForm] = useState({
    channelId: '',
    message: ''
  });

  useEffect(() => {
    loadIntegrationStatus();
  }, []);

  const loadIntegrationStatus = async () => {
    setIsLoading(true);
    setError('');
    try {
      const [slackResponse, discordResponse] = await Promise.all([
        api.getSlackChannels(),
        api.getDiscordChannels()
      ]);
      
      setSlackChannels(slackResponse);
      setDiscordChannels(discordResponse);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load integration status');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSlackSend = async () => {
    setIsProcessing(true);
    try {
      const response = await api.sendSlackMessage({
        channel: slackForm.channel,
        message: slackForm.message
      });
      
      if (response.success) {
        setSuccess('Message sent to Slack successfully!');
        setSlackForm({ channel: '', message: '' });
        setShowSlackModal(false);
      } else {
        setError('Failed to send message to Slack');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send Slack message');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleDiscordSend = async () => {
    setIsProcessing(true);
    try {
      const response = await api.sendDiscordMessage({
        channelId: discordForm.channelId,
        message: discordForm.message
      });
      
      if (response.success) {
        setSuccess('Message sent to Discord successfully!');
        setDiscordForm({ channelId: '', message: '' });
        setShowDiscordModal(false);
      } else {
        setError('Failed to send message to Discord');
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to send Discord message');
    } finally {
      setIsProcessing(false);
    }
  };

  const getStatusBadge = (configured: boolean) => {
    return (
      <span className={`px-2 py-1 rounded-full text-xs font-medium ${configured ? 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-200' : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-200'}`}>
        {configured ? 'Configured' : 'Not Configured'}
      </span>
    );
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <div className="max-w-6xl mx-auto px-4 py-4">
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
                  <Bot className="h-5 w-5 mr-2 text-blue-600" />
                  Integration Management
                </h1>
                <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  Manage Slack and Discord integrations
                </p>
              </div>
            </div>
            <div className="flex items-center space-x-2">
              <button
                onClick={() => setShowSlackModal(true)}
                className="flex items-center space-x-2 px-4 py-2 bg-blue-600 dark:bg-blue-700 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 transition-colors"
              >
                <MessageSquare className="h-4 w-4" />
                <span>Send to Slack</span>
              </button>
              <button
                onClick={() => setShowDiscordModal(true)}
                className="flex items-center space-x-2 px-4 py-2 bg-purple-600 dark:bg-purple-700 text-white rounded-lg hover:bg-purple-700 dark:hover:bg-purple-600 transition-colors"
              >
                <Bot className="h-4 w-4" />
                <span>Send to Discord</span>
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="max-w-6xl mx-auto px-4 py-6">
        {error && (
          <div className="mb-6 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
            <p className="text-red-800 dark:text-red-200">{error}</p>
          </div>
        )}

        {success && (
          <div className="mb-6 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg p-4">
            <p className="text-green-800 dark:text-green-200">{success}</p>
          </div>
        )}

        {/* Tab Navigation */}
        <div className="border-b border-gray-200 dark:border-gray-700 mb-6">
          <nav className="-mb-px flex space-x-8">
            <button
              onClick={() => setActiveTab('overview')}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'overview'
                  ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                  : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
              }`}
            >
              Overview
            </button>
            <button
              onClick={() => setActiveTab('slack')}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'slack'
                  ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                  : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
              }`}
            >
              Slack
            </button>
            <button
              onClick={() => setActiveTab('discord')}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'discord'
                  ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                  : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
              }`}
            >
              Discord
            </button>
          </nav>
        </div>

        {isLoading ? (
          <div className="flex items-center justify-center h-32">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          </div>
        ) : (
          <>
            {/* Overview Tab */}
            {activeTab === 'overview' && (
              <div className="space-y-6">
                <div className="grid md:grid-cols-2 gap-6">
                  <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="text-lg font-semibold flex items-center gap-2 text-gray-900 dark:text-gray-100">
                        <MessageSquare className="h-5 w-5 text-blue-600" />
                        Slack Integration
                        {getStatusBadge(slackChannels.configured)}
                      </h3>
                    </div>
                    <div className="space-y-4">
                      <div>
                        <h4 className="font-medium mb-2 text-gray-700 dark:text-gray-300">Available Channels:</h4>
                        <div className="flex flex-wrap gap-2">
                          {slackChannels.channels.map((channel) => (
                            <span key={channel} className="px-2 py-1 bg-gray-100 dark:bg-gray-700 rounded text-sm text-gray-700 dark:text-gray-300">
                              #{channel}
                            </span>
                          ))}
                        </div>
                      </div>
                      <div className="text-sm text-gray-600 dark:text-gray-400">
                        {slackChannels.configured ? (
                          <p>✅ Slack integration is properly configured and ready to use.</p>
                        ) : (
                          <p>⚠️ Slack integration needs to be configured with proper credentials.</p>
                        )}
                      </div>
                      <button 
                        onClick={() => setShowSlackModal(true)}
                        disabled={!slackChannels.configured}
                        className={`w-full px-4 py-2 rounded transition-colors ${slackChannels.configured ? 'bg-blue-600 text-white hover:bg-blue-700' : 'bg-gray-300 text-gray-500 cursor-not-allowed'}`}
                      >
                        Send Test Message
                      </button>
                    </div>
                  </div>

                  <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="text-lg font-semibold flex items-center gap-2 text-gray-900 dark:text-gray-100">
                        <Bot className="h-5 w-5 text-purple-600" />
                        Discord Integration
                        {getStatusBadge(discordChannels.configured)}
                      </h3>
                    </div>
                    <div className="space-y-4">
                      <div>
                        <h4 className="font-medium mb-2 text-gray-700 dark:text-gray-300">Available Channels:</h4>
                        <div className="flex flex-wrap gap-2">
                          {discordChannels.channels.map((channel) => (
                            <span key={channel} className="px-2 py-1 bg-gray-100 dark:bg-gray-700 rounded text-sm text-gray-700 dark:text-gray-300">
                              #{channel}
                            </span>
                          ))}
                        </div>
                      </div>
                      <div className="text-sm text-gray-600 dark:text-gray-400">
                        {discordChannels.configured ? (
                          <p>✅ Discord integration is properly configured and ready to use.</p>
                        ) : (
                          <p>⚠️ Discord integration needs to be configured with proper credentials.</p>
                        )}
                      </div>
                      <button 
                        onClick={() => setShowDiscordModal(true)}
                        disabled={!discordChannels.configured}
                        className={`w-full px-4 py-2 rounded transition-colors ${discordChannels.configured ? 'bg-purple-600 text-white hover:bg-purple-700' : 'bg-gray-300 text-gray-500 cursor-not-allowed'}`}
                      >
                        Send Test Message
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Slack Tab */}
            {activeTab === 'slack' && (
              <div className="space-y-6">
                <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold flex items-center gap-2 text-gray-900 dark:text-gray-100">
                      <MessageSquare className="h-5 w-5 text-blue-600" />
                      Slack Configuration
                      {getStatusBadge(slackChannels.configured)}
                    </h3>
                  </div>
                  <div className="space-y-6">
                    <div className="p-4 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg">
                      <h4 className="font-medium text-blue-900 dark:text-blue-200 mb-2">Configuration Required</h4>
                      <p className="text-sm text-blue-800 dark:text-blue-300 mb-3">
                        To use Slack integration, you need to configure the following:
                      </p>
                      <ul className="text-sm text-blue-800 dark:text-blue-300 space-y-1 list-disc list-inside">
                        <li>Slack Bot Token or Webhook URL</li>
                        <li>Channel permissions for the bot</li>
                        <li>Event subscriptions (if using Bot API)</li>
                      </ul>
                    </div>

                    <div>
                      <h4 className="font-medium mb-2 text-gray-700 dark:text-gray-300">Available Channels:</h4>
                      <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
                        {slackChannels.channels.map((channel) => (
                          <div key={channel} className="p-3 border border-gray-200 dark:border-gray-600 rounded-lg text-center">
                            <Hash className="h-4 w-4 mx-auto mb-1 text-gray-500 dark:text-gray-400" />
                            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{channel}</span>
                          </div>
                        ))}
                      </div>
                    </div>

                    <div className="flex space-x-2">
                      <button 
                        onClick={() => setShowSlackModal(true)}
                        disabled={!slackChannels.configured}
                        className={`px-4 py-2 rounded transition-colors ${slackChannels.configured ? 'bg-blue-600 text-white hover:bg-blue-700' : 'bg-gray-300 text-gray-500 cursor-not-allowed'}`}
                      >
                        Send Message
                      </button>
                      <button className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300">
                        <Settings className="h-4 w-4 mr-2" />
                        Configure
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Discord Tab */}
            {activeTab === 'discord' && (
              <div className="space-y-6">
                <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="text-lg font-semibold flex items-center gap-2 text-gray-900 dark:text-gray-100">
                      <Bot className="h-5 w-5 text-purple-600" />
                      Discord Configuration
                      {getStatusBadge(discordChannels.configured)}
                    </h3>
                  </div>
                  <div className="space-y-6">
                    <div className="p-4 bg-purple-50 dark:bg-purple-900/20 border border-purple-200 dark:border-purple-800 rounded-lg">
                      <h4 className="font-medium text-purple-900 dark:text-purple-200 mb-2">Configuration Required</h4>
                      <p className="text-sm text-purple-800 dark:text-purple-300 mb-3">
                        To use Discord integration, you need to configure the following:
                      </p>
                      <ul className="text-sm text-purple-800 dark:text-purple-300 space-y-1 list-disc list-inside">
                        <li>Discord Bot Token</li>
                        <li>Channel permissions for the bot</li>
                        <li>Server invite permissions</li>
                      </ul>
                    </div>

                    <div>
                      <h4 className="font-medium mb-2 text-gray-700 dark:text-gray-300">Available Channels:</h4>
                      <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
                        {discordChannels.channels.map((channel) => (
                          <div key={channel} className="p-3 border border-gray-200 dark:border-gray-600 rounded-lg text-center">
                            <Hash className="h-4 w-4 mx-auto mb-1 text-gray-500 dark:text-gray-400" />
                            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{channel}</span>
                          </div>
                        ))}
                      </div>
                    </div>

                    <div className="flex space-x-2">
                      <button 
                        onClick={() => setShowDiscordModal(true)}
                        disabled={!discordChannels.configured}
                        className={`px-4 py-2 rounded transition-colors ${discordChannels.configured ? 'bg-purple-600 text-white hover:bg-purple-700' : 'bg-gray-300 text-gray-500 cursor-not-allowed'}`}
                      >
                        Send Message
                      </button>
                      <button className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300">
                        <Settings className="h-4 w-4 mr-2" />
                        Configure
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Integration Status Summary */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h3 className="text-lg font-semibold mb-4 text-gray-900 dark:text-gray-100">Integration Status Summary</h3>
              <div className="grid md:grid-cols-2 gap-4">
                <div className="flex items-center justify-between p-4 border border-gray-200 dark:border-gray-600 rounded-lg">
                  <div className="flex items-center gap-3">
                    <MessageSquare className="h-5 w-5 text-blue-500" />
                    <div>
                      <h4 className="font-medium text-gray-900 dark:text-gray-100">Slack</h4>
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        {slackChannels.channels.length} channels available
                      </p>
                    </div>
                  </div>
                  {slackChannels.configured ? (
                    <CheckCircle className="h-5 w-5 text-green-500" />
                  ) : (
                    <XCircle className="h-5 w-5 text-red-500" />
                  )}
                </div>

                <div className="flex items-center justify-between p-4 border border-gray-200 dark:border-gray-600 rounded-lg">
                  <div className="flex items-center gap-3">
                    <Bot className="h-5 w-5 text-purple-500" />
                    <div>
                      <h4 className="font-medium text-gray-900 dark:text-gray-100">Discord</h4>
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        {discordChannels.channels.length} channels available
                      </p>
                    </div>
                  </div>
                  {discordChannels.configured ? (
                    <CheckCircle className="h-5 w-5 text-green-500" />
                  ) : (
                    <XCircle className="h-5 w-5 text-red-500" />
                  )}
                </div>
              </div>
            </div>
          </>
        )}
      </div>

      {/* Slack Send Modal */}
      {showSlackModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-lg">
            <h2 className="text-xl font-bold mb-4 text-gray-900 dark:text-gray-100">Send Message to Slack</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">Channel</label>
                <select
                  value={slackForm.channel}
                  onChange={(e) => setSlackForm({ ...slackForm, channel: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 rounded px-3 py-2 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                >
                  <option value="">Select a channel</option>
                  {slackChannels.channels.map((channel) => (
                    <option key={channel} value={channel}>
                      #{channel}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">Message</label>
                <textarea
                  value={slackForm.message}
                  onChange={(e) => setSlackForm({ ...slackForm, message: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 rounded px-3 py-2 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                  placeholder="Enter your message..."
                  rows={4}
                />
              </div>
              <div className="flex justify-end space-x-2">
                <button
                  onClick={() => setShowSlackModal(false)}
                  className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300"
                >
                  Cancel
                </button>
                <button
                  onClick={handleSlackSend}
                  disabled={!slackForm.channel || !slackForm.message || isProcessing}
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isProcessing ? 'Sending...' : 'Send Message'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Discord Send Modal */}
      {showDiscordModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-lg">
            <h2 className="text-xl font-bold mb-4 text-gray-900 dark:text-gray-100">Send Message to Discord</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">Channel</label>
                <select
                  value={discordForm.channelId}
                  onChange={(e) => setDiscordForm({ ...discordForm, channelId: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 rounded px-3 py-2 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                >
                  <option value="">Select a channel</option>
                  {discordChannels.channels.map((channel) => (
                    <option key={channel} value={channel}>
                      #{channel}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">Message</label>
                <textarea
                  value={discordForm.message}
                  onChange={(e) => setDiscordForm({ ...discordForm, message: e.target.value })}
                  className="w-full border border-gray-300 dark:border-gray-600 rounded px-3 py-2 bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100"
                  placeholder="Enter your message..."
                  rows={4}
                />
              </div>
              <div className="flex justify-end space-x-2">
                <button
                  onClick={() => setShowDiscordModal(false)}
                  className="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-50 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300"
                >
                  Cancel
                </button>
                <button
                  onClick={handleDiscordSend}
                  disabled={!discordForm.channelId || !discordForm.message || isProcessing}
                  className="px-4 py-2 bg-purple-600 text-white rounded hover:bg-purple-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isProcessing ? 'Sending...' : 'Send Message'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}