import { useState, useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { LogOut, Settings, MessageSquare, Plus, Search, Sparkles } from 'lucide-react'
import { ConversationList } from '../components/ConversationList'
import { MessageList } from '../components/MessageList'
import { MessageInput } from '../components/MessageInput'
import { TemplateSelector } from '../components/TemplateSelector'
import { api } from '../api'

interface UploadedFile {
  id: string
  name: string
  size: number
  type: string
  url?: string
  content?: string
}

interface Message {
  id: string
  role: 'user' | 'assistant' | 'system'
  content: string
  timestamp: Date
  files?: UploadedFile[]
}

interface Conversation {
  id: string
  title: string
  createdAt: Date
  updatedAt: Date
  model: string
  category: string
}


export default function ChatPage() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const [conversations, setConversations] = useState<Conversation[]>([])
  const [currentConversation, setCurrentConversation] = useState<Conversation | null>(null)
  const [messages, setMessages] = useState<Message[]>([])
  // const [isLoading] = useState(false)
  const [isStreaming, setIsStreaming] = useState(false)
  const [showTemplateSelector, setShowTemplateSelector] = useState(false)
  const abortControllerRef = useRef<AbortController | null>(null)

  useEffect(() => {
    loadConversations()
  }, [])

  useEffect(() => {
    if (currentConversation) {
      loadMessages(currentConversation.id)
    }
  }, [currentConversation])


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

  const createConversation = async (title: string, model?: string, category?: string) => {
    try {
      console.log('Creating conversation with title:', title, 'model:', model, 'category:', category)
      const response = await fetch('http://localhost:5058/api/conversations', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({ 
          title, 
          model: model || '',
          category: category || 'General'
        }),
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
        return newConversation
      } else {
        console.error('Failed to create conversation, status:', response.status)
        throw new Error('Failed to create conversation')
      }
    } catch (error) {
      console.error('Failed to create conversation:', error)
      throw error
    }
  }

  const sendMessage = async (content: string, files?: UploadedFile[]) => {
    if (!content.trim() && !files?.length) return

    const userMessage: Message = {
      id: Date.now().toString(),
      role: 'user',
      content,
      timestamp: new Date(),
      files
    }

    setMessages(prev => [...prev, userMessage])
    setIsStreaming(true)

    try {
      // Prepare message content with file information
      if (files && files.length > 0) {
        const fileInfo = files.map(file => {
          if (file.type.startsWith('image/')) {
            return `[Image: ${file.name}]`
          } else if (file.content) {
            return `[File: ${file.name}]\n${file.content}`
          } else {
            return `[File: ${file.name}]`
          }
        }).join('\n\n')
        content = content ? `${content}\n\n${fileInfo}` : fileInfo
      }

      const response = await fetch('http://localhost:5058/api/chat', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({
          model: currentConversation?.model || '',
          messages: [...messages, userMessage].map(m => ({
            role: m.role,
            content: m.files ? `${m.content}\n\n${m.files.map(f => f.type.startsWith('image/') ? `[Image: ${f.name}]` : `[File: ${f.name}]`).join('\n')}` : m.content
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

  const handleConversationDelete = async (conversationId: string) => {
    try {
      await api.deleteConversation(conversationId)
      
      // Update local state
      setConversations(prev => prev.filter(conv => conv.id !== conversationId))
      
      // Clear current conversation if it's the one being deleted
      if (currentConversation?.id === conversationId) {
        setCurrentConversation(null)
        setMessages([])
      }
    } catch (error) {
      console.error('Failed to delete conversation:', error)
      throw error
    }
  }

  const handleSelectTemplate = async (template: any) => {
    try {
      // Create a new conversation with the template
      const conversation = await createConversation(template.name, '', template.category)
      
      // Add the system prompt as the first message
      const systemMessage: Message = {
        id: `system-${Date.now()}`,
        role: 'system',
        content: template.systemPrompt,
        timestamp: new Date(),
        files: []
      }
      
      setMessages([systemMessage])
      
      // If there are example messages, add them as user messages
      if (template.exampleMessages && template.exampleMessages.length > 0) {
        const exampleMessages: Message[] = template.exampleMessages.map((msg: string, index: number) => ({
          id: `example-${index}-${Date.now()}`,
          role: 'user' as const,
          content: msg,
          timestamp: new Date(),
          files: []
        }))
        
        setMessages(prev => [...prev, ...exampleMessages])
      }
      
      setCurrentConversation(conversation)
    } catch (error) {
      console.error('Failed to apply template:', error)
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
            <button
              onClick={() => setShowTemplateSelector(true)}
              className="flex items-center space-x-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100"
            >
              <Sparkles className="h-5 w-5" />
              <span>Templates</span>
            </button>
            
            <button
              onClick={() => navigate('/settings')}
              className="flex items-center space-x-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100"
            >
              <Settings className="h-5 w-5" />
              <span>Settings</span>
            </button>
            
            <button
              onClick={() => navigate('/search')}
              className="flex items-center space-x-2 text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-gray-100"
            >
              <Search className="h-5 w-5" />
              <span>Search</span>
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
            onConversationDelete={handleConversationDelete}
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
      
      {/* Template Selector Modal */}
      <TemplateSelector
        isOpen={showTemplateSelector}
        onClose={() => setShowTemplateSelector(false)}
        onSelectTemplate={handleSelectTemplate}
      />
    </div>
  )
}
