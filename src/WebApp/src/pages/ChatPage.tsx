import { useState, useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { LogOut, Settings, MessageSquare, Plus } from 'lucide-react'
import { ConversationList } from '../components/ConversationList'
import { MessageList } from '../components/MessageList'
import { MessageInput } from '../components/MessageInput'
import { ModelSelector } from '../components/ModelSelector'
import { api } from '../api'

interface Message {
  id: string
  role: 'user' | 'assistant' | 'system'
  content: string
  timestamp: Date
}

interface Conversation {
  id: string
  title: string
  createdAt: Date
  updatedAt: Date
}

interface Model {
  id: string
  object: string
  created: number
  ownedBy: string
}

export default function ChatPage() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const [conversations, setConversations] = useState<Conversation[]>([])
  const [currentConversation, setCurrentConversation] = useState<Conversation | null>(null)
  const [messages, setMessages] = useState<Message[]>([])
  const [models, setModels] = useState<Model[]>([])
  const [selectedModel, setSelectedModel] = useState<string>('')
  // const [isLoading] = useState(false)
  const [isStreaming, setIsStreaming] = useState(false)
  const abortControllerRef = useRef<AbortController | null>(null)

  useEffect(() => {
    loadModels()
    loadConversations()
  }, [])

  useEffect(() => {
    if (currentConversation) {
      loadMessages(currentConversation.id)
    }
  }, [currentConversation])

  const loadModels = async () => {
    try {
      const response = await fetch('http://localhost:5058/api/models', {
        credentials: 'include'
      })
      if (response.ok) {
        const data = await response.json()
        setModels((data.data || []).map((model: any) => ({
          id: model.id,
          object: model.object,
          created: model.created || 0,
          ownedBy: model.owned_by
        })).filter((model: any) => !model.id.includes('embedding'))) // Filtere Embedding-Modelle heraus
        
        const chatModels = (data.data || []).filter((model: any) => !model.id.includes('embedding'))
        if (chatModels.length > 0) {
          setSelectedModel(chatModels[0].id)
        }
      }
    } catch (error) {
      console.error('Failed to load models:', error)
    }
  }

  const loadConversations = async () => {
    try {
      const response = await fetch('http://localhost:5058/api/conversations', {
        credentials: 'include'
      })
      if (response.ok) {
        const data = await response.json()
        setConversations((data.data || []).map((c: any) => ({
          ...c,
          createdAt: new Date(c.createdAt),
          updatedAt: new Date(c.updatedAt)
        })))
      }
    } catch (error) {
      console.error('Failed to load conversations:', error)
    }
  }

  const loadMessages = async (conversationId: string) => {
    try {
      const response = await fetch(`http://localhost:5058/api/conversations/${conversationId}`, {
        credentials: 'include'
      })
      if (response.ok) {
        const data = await response.json()
        setMessages((data.messages || []).map((m: any) => ({
          ...m,
          timestamp: new Date(m.createdAt)
        })))
      }
    } catch (error) {
      console.error('Failed to load messages:', error)
    }
  }

  const createConversation = async (title: string) => {
    try {
      console.log('Creating conversation with title:', title)
      const response = await fetch('http://localhost:5058/api/conversations', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({ title }),
      })

      console.log('Response status:', response.status)
      
      if (response.ok) {
        const conversation = await response.json()
        console.log('Created conversation:', conversation)
        const newConversation = {
          ...conversation,
          createdAt: new Date(conversation.createdAt),
          updatedAt: new Date(conversation.updatedAt)
        }
        setConversations(prev => [newConversation, ...prev])
        setCurrentConversation(newConversation)
        setMessages([])
        console.log('Conversation set as current:', newConversation)
      } else {
        console.error('Failed to create conversation, status:', response.status)
      }
    } catch (error) {
      console.error('Failed to create conversation:', error)
    }
  }

  const sendMessage = async (content: string) => {
    if (!selectedModel || !content.trim()) return

    const userMessage: Message = {
      id: Date.now().toString(),
      role: 'user',
      content,
      timestamp: new Date()
    }

    setMessages(prev => [...prev, userMessage])
    setIsStreaming(true)

    try {
      const response = await fetch('http://localhost:5058/api/chat', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({
          model: selectedModel,
          messages: [...messages, userMessage].map(m => ({
            role: m.role,
            content: m.content
          })),
          conversationId: currentConversation?.id
        })
      })

      if (!response.ok) {
        throw new Error('Failed to send message')
      }

      const data = await response.json()
      console.log('Chat response:', data)

      const assistantMessage: Message = {
        id: (Date.now() + 1).toString(),
        role: 'assistant',
        content: data.choices?.[0]?.message?.content || 'Keine Antwort erhalten',
        timestamp: new Date()
      }

      setMessages(prev => [...prev, assistantMessage])
    } catch (error) {
      console.error('Failed to send message:', error)
      
      const errorMessage: Message = {
        id: (Date.now() + 1).toString(),
        role: 'assistant',
        content: 'Fehler beim Senden der Nachricht. Bitte versuche es erneut.',
        timestamp: new Date()
      }
      
      setMessages(prev => [...prev, errorMessage])
    } finally {
      setIsStreaming(false)
    }
  }

  const stopStreaming = () => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort()
      setIsStreaming(false)
    }
  }

  const handleLogout = async () => {
    await logout()
  }

  const handleConversationRename = async (conversationId: string, newTitle: string) => {
    try {
      await api.updateConversationTitle(conversationId, newTitle)
      
      // Update local state
      setConversations(prev => 
        prev.map(conv => 
          conv.id === conversationId 
            ? { ...conv, title: newTitle, updatedAt: new Date() }
            : conv
        )
      )
      
      // Update current conversation if it's the one being renamed
      if (currentConversation?.id === conversationId) {
        setCurrentConversation(prev => 
          prev ? { ...prev, title: newTitle, updatedAt: new Date() } : null
        )
      }
    } catch (error) {
      console.error('Failed to rename conversation:', error)
      throw error
    }
  }

  return (
    <div className="h-screen flex flex-col bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <header className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700 px-4 py-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <MessageSquare className="h-6 w-6 text-blue-600" />
            <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">LM Gateway</h1>
          </div>
          
          <div className="flex items-center space-x-4">
            <ModelSelector 
              models={models}
              selectedModel={selectedModel}
              onModelChange={setSelectedModel}
            />
            
            <button
              onClick={() => navigate('/settings')}
              className="flex items-center space-x-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100"
            >
              <Settings className="h-5 w-5" />
              <span>Settings</span>
            </button>
            
            <button
              onClick={handleLogout}
              className="flex items-center space-x-2 text-gray-600 hover:text-gray-900"
            >
              <LogOut className="h-5 w-5" />
              <span>Logout</span>
            </button>
          </div>
        </div>
      </header>

      <div className="flex-1 flex overflow-hidden">
      {/* Sidebar */}
      <div className="w-80 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 flex flex-col">
          <div className="p-4 border-b border-gray-200 dark:border-gray-700">
            <button
              onClick={() => createConversation('New Conversation')}
              className="w-full flex items-center justify-center space-x-2 py-2 px-4 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              <Plus className="h-4 w-4" />
              <span>New Conversation</span>
            </button>
          </div>
          
          <ConversationList
            conversations={conversations}
            currentConversation={currentConversation}
            onConversationSelect={setCurrentConversation}
            onConversationRename={handleConversationRename}
          />
        </div>

        {/* Main Chat Area */}
        <div className="flex-1 flex flex-col">
          {currentConversation ? (
            <>
              <MessageList messages={messages} />
              <div className="border-t border-gray-200 dark:border-gray-700 p-4">
                <MessageInput
                  onSendMessage={sendMessage}
                  disabled={isStreaming}
                  onStop={stopStreaming}
                />
              </div>
            </>
          ) : (
            <div className="flex-1 flex items-center justify-center text-gray-500 dark:text-gray-400">
              <div className="text-center">
                <MessageSquare className="h-12 w-12 mx-auto mb-4 text-gray-400 dark:text-gray-500" />
                <p>Select a conversation or create a new one to start chatting</p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
