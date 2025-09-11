import { Clock, Edit2, Check, X } from 'lucide-react'
import { useState } from 'react'

interface Conversation {
  id: string
  title: string
  createdAt: Date
  updatedAt: Date
  model: string
  category: string
}

interface ConversationListProps {
  conversations: Conversation[]
  currentConversation: Conversation | null
  onConversationSelect: (conversation: Conversation) => void
  onConversationRename: (conversationId: string, newTitle: string) => Promise<void>
}

export function ConversationList({ 
  conversations, 
  currentConversation, 
  onConversationSelect,
  onConversationRename
}: ConversationListProps) {
  const [editingId, setEditingId] = useState<string | null>(null)
  const [editTitle, setEditTitle] = useState('')

  const formatDate = (date: Date) => {
    const now = new Date()
    const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60)
    
    if (diffInHours < 24) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    } else {
      return date.toLocaleDateString()
    }
  }

  const handleEditStart = (conversation: Conversation) => {
    setEditingId(conversation.id)
    setEditTitle(conversation.title)
  }

  const handleEditSave = async () => {
    if (editingId && editTitle.trim()) {
      try {
        await onConversationRename(editingId, editTitle.trim())
        setEditingId(null)
        setEditTitle('')
      } catch (error) {
        console.error('Failed to rename conversation:', error)
      }
    }
  }

  const handleEditCancel = () => {
    setEditingId(null)
    setEditTitle('')
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleEditSave()
    } else if (e.key === 'Escape') {
      handleEditCancel()
    }
  }

  return (
    <div className="flex-1 overflow-y-auto">
      {conversations.length === 0 ? (
        <div className="p-4 text-center text-gray-500 dark:text-gray-400">
          <p>No conversations yet</p>
          <p className="text-sm">Create your first conversation to get started</p>
        </div>
      ) : (
        <div className="p-2 space-y-1">
          {conversations.map((conversation) => (
            <div
              key={conversation.id}
              className={`w-full text-left p-3 rounded-lg transition-colors ${
                currentConversation?.id === conversation.id
                  ? 'bg-blue-100 dark:bg-blue-900/30 text-blue-900 dark:text-blue-100'
                  : 'hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300'
              }`}
            >
              <div className="flex items-start justify-between">
                <div className="flex-1 min-w-0" onClick={() => onConversationSelect(conversation)}>
                  {editingId === conversation.id ? (
                    <input
                      type="text"
                      value={editTitle}
                      onChange={(e) => setEditTitle(e.target.value)}
                      onKeyDown={handleKeyDown}
                      onBlur={handleEditSave}
                      className="w-full text-sm font-medium bg-transparent border-none outline-none text-gray-900 dark:text-gray-100"
                      autoFocus
                    />
                  ) : (
                    <p className="text-sm font-medium truncate">
                      {conversation.title}
                    </p>
                  )}
                  {conversation.model && (
                    <div className="text-xs text-blue-600 dark:text-blue-400 mt-1">
                      {conversation.model.split('/').pop()}
                    </div>
                  )}
                  {conversation.category && conversation.category !== 'General' && (
                    <div className="text-xs text-green-600 dark:text-green-400 mt-1">
                      #{conversation.category}
                    </div>
                  )}
                  <div className="flex items-center mt-1 text-xs text-gray-500 dark:text-gray-400">
                    <Clock className="h-3 w-3 mr-1" />
                    <span>{formatDate(conversation.updatedAt)}</span>
                  </div>
                </div>
                <div className="flex items-center space-x-1 ml-2">
                  {editingId === conversation.id ? (
                    <>
                      <button
                        onClick={handleEditSave}
                        className="p-1 hover:bg-gray-200 dark:hover:bg-gray-600 rounded"
                        title="Save"
                      >
                        <Check className="h-3 w-3" />
                      </button>
                      <button
                        onClick={handleEditCancel}
                        className="p-1 hover:bg-gray-200 dark:hover:bg-gray-600 rounded"
                        title="Cancel"
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </>
                  ) : (
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleEditStart(conversation)
                      }}
                      className="p-1 hover:bg-gray-200 rounded transition-colors"
                      title="Rename"
                    >
                      <Edit2 className="h-3 w-3" />
                    </button>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
