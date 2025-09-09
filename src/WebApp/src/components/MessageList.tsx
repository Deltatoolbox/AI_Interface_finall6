import { useEffect, useRef } from 'react'
import ReactMarkdown from 'react-markdown'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { tomorrow } from 'react-syntax-highlighter/dist/esm/styles/prism'
import { User, Bot, Settings } from 'lucide-react'

interface Message {
  id: string
  role: 'user' | 'assistant' | 'system'
  content: string
  timestamp: Date
}

interface MessageListProps {
  messages: Message[]
}

export function MessageList({ messages }: MessageListProps) {
  const messagesEndRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const getMessageIcon = (role: string) => {
    switch (role) {
      case 'user':
        return <User className="h-5 w-5 text-blue-600" />
      case 'assistant':
        return <Bot className="h-5 w-5 text-gray-600" />
      case 'system':
        return <Settings className="h-5 w-5 text-yellow-600" />
      default:
        return null
    }
  }

  const getMessageClass = (role: string) => {
    switch (role) {
      case 'user':
        return 'message-user'
      case 'assistant':
        return 'message-assistant'
      case 'system':
        return 'message-system'
      default:
        return 'message-assistant'
    }
  }

  return (
    <div className="flex-1 overflow-y-auto p-4 space-y-4">
      {messages.length === 0 ? (
        <div className="flex items-center justify-center h-full text-gray-500">
          <div className="text-center">
            <Bot className="h-12 w-12 mx-auto mb-4 text-gray-400" />
            <p>Start a conversation by typing a message below</p>
          </div>
        </div>
      ) : (
        <>
          {messages.map((message) => (
            <div key={message.id} className={`flex space-x-3 ${getMessageClass(message.role)}`}>
              <div className="flex-shrink-0">
                {getMessageIcon(message.role)}
              </div>
              <div className="flex-1 min-w-0">
                <div className="prose prose-sm max-w-none">
                  {message.role === 'assistant' ? (
                    <ReactMarkdown
                      components={{
                        code({ node, className, children, ...props }: any) {
                          const match = /language-(\w+)/.exec(className || '')
                          return !props.inline && match ? (
                            <SyntaxHighlighter
                              style={tomorrow}
                              language={match[1]}
                              PreTag="div"
                              {...props}
                            >
                              {String(children).replace(/\n$/, '')}
                            </SyntaxHighlighter>
                          ) : (
                            <code className={className} {...props}>
                              {children}
                            </code>
                          )
                        }
                      }}
                    >
                      {message.content}
                    </ReactMarkdown>
                  ) : (
                    <p className="whitespace-pre-wrap">{message.content}</p>
                  )}
                </div>
                <div className="mt-2 text-xs text-gray-500">
                  {message.timestamp.toLocaleTimeString()}
                </div>
              </div>
            </div>
          ))}
          <div ref={messagesEndRef} />
        </>
      )}
    </div>
  )
}
