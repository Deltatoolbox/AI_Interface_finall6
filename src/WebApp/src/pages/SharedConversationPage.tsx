import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { ArrowLeft, Lock, Calendar, User, MessageSquare, Copy, Check } from 'lucide-react'
import { MessageList } from '../components/MessageList'
import { api } from '../api'

interface Message {
  id: string
  role: 'user' | 'assistant' | 'system'
  content: string
  timestamp: Date
  files?: any[]
}

interface SharedConversation {
  id: string
  title: string
  createdAt: Date
  updatedAt: Date
  messages: Message[]
  sharedBy: string
  sharedAt: Date
}

export default function SharedConversationPage() {
  const { shareId } = useParams<{ shareId: string }>()
  const navigate = useNavigate()
  const [conversation, setConversation] = useState<SharedConversation | null>(null)
  const [password, setPassword] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [needsPassword, setNeedsPassword] = useState(false)
  const [copied, setCopied] = useState(false)

  useEffect(() => {
    if (shareId) {
      loadSharedConversation()
    }
  }, [shareId])

  const loadSharedConversation = async (providedPassword?: string) => {
    if (!shareId) return

    setIsLoading(true)
    setError('')

    try {
      const data = await api.getSharedConversation(shareId, providedPassword || password)
      
      // Convert timestamps to Date objects
      const messages = data.messages.map((msg: any) => ({
        ...msg,
        timestamp: new Date(msg.timestamp)
      }))

      setConversation({
        ...data,
        createdAt: new Date(data.createdAt),
        updatedAt: new Date(data.updatedAt),
        sharedAt: new Date(data.sharedAt),
        messages
      })
      setNeedsPassword(false)
    } catch (err) {
      if (err instanceof Error) {
        if (err.message.includes('password') || err.message.includes('unauthorized')) {
          setNeedsPassword(true)
        } else {
          setError(err.message)
        }
      } else {
        setError('Failed to load shared conversation')
      }
    } finally {
      setIsLoading(false)
    }
  }

  const handlePasswordSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    loadSharedConversation(password)
  }

  const copyShareLink = async () => {
    try {
      await navigator.clipboard.writeText(window.location.href)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch (err) {
      console.error('Failed to copy link:', err)
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
          <p className="text-gray-600 dark:text-gray-400">Loading shared conversation...</p>
        </div>
      </div>
    )
  }

  if (needsPassword) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="max-w-md w-full bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6">
          <div className="flex items-center justify-center mb-6">
            <Lock className="h-12 w-12 text-blue-600 dark:text-blue-400" />
          </div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100 text-center mb-6">
            Password Required
          </h1>
          <p className="text-gray-600 dark:text-gray-400 text-center mb-6">
            This shared conversation is protected with a password.
          </p>
          <form onSubmit={handlePasswordSubmit} className="space-y-4">
            <div>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Enter password"
                className="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                required
              />
            </div>
            <button
              type="submit"
              className="w-full px-4 py-3 bg-blue-600 dark:bg-blue-700 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
            >
              Access Conversation
            </button>
          </form>
          {error && (
            <p className="mt-4 text-red-600 dark:text-red-400 text-center text-sm">
              {error}
            </p>
          )}
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="max-w-md w-full bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 text-center">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-gray-100 mb-4">
            Conversation Not Found
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mb-6">
            {error}
          </p>
          <button
            onClick={() => navigate('/')}
            className="px-4 py-2 bg-blue-600 dark:bg-blue-700 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 transition-colors"
          >
            Go Home
          </button>
        </div>
      </div>
    )
  }

  if (!conversation) {
    return null
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
        <div className="max-w-4xl mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <button
                onClick={() => navigate('/')}
                className="p-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100 transition-colors"
              >
                <ArrowLeft className="h-5 w-5" />
              </button>
              <div>
                <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">
                  {conversation.title}
                </h1>
                <div className="flex items-center space-x-4 text-sm text-gray-500 dark:text-gray-400 mt-1">
                  <div className="flex items-center space-x-1">
                    <User className="h-4 w-4" />
                    <span>Shared by {conversation.sharedBy}</span>
                  </div>
                  <div className="flex items-center space-x-1">
                    <Calendar className="h-4 w-4" />
                    <span>{conversation.sharedAt.toLocaleDateString()}</span>
                  </div>
                  <div className="flex items-center space-x-1">
                    <MessageSquare className="h-4 w-4" />
                    <span>{conversation.messages.length} messages</span>
                  </div>
                </div>
              </div>
            </div>
            <button
              onClick={copyShareLink}
              className="flex items-center space-x-2 px-4 py-2 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
            >
              {copied ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
              <span>{copied ? 'Copied!' : 'Copy Link'}</span>
            </button>
          </div>
        </div>
      </div>

      {/* Messages */}
      <div className="max-w-4xl mx-auto">
        <MessageList messages={conversation.messages} />
      </div>

      {/* Footer */}
      <div className="bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 mt-8">
        <div className="max-w-4xl mx-auto px-4 py-4 text-center text-sm text-gray-500 dark:text-gray-400">
          <p>This is a shared conversation. You cannot reply or modify it.</p>
        </div>
      </div>
    </div>
  )
}
