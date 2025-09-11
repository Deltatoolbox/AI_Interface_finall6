import { useEffect, useRef } from 'react'
import ReactMarkdown from 'react-markdown'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { tomorrow, vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism'
import { InlineMath, BlockMath } from 'react-katex'
import 'katex/dist/katex.min.css'
import { User, Bot, Settings, Image, FileText, File } from 'lucide-react'

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

interface MessageListProps {
  messages: Message[]
}

export function MessageList({ messages }: MessageListProps) {
  const messagesEndRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  // Check if dark mode is active
  const isDarkMode = document.documentElement.classList.contains('dark')
  const codeTheme = isDarkMode ? vscDarkPlus : tomorrow

  // Function to render LaTeX math expressions
  const renderMathContent = (content: string) => {
    // Split content by LaTeX delimiters
    const parts = content.split(/(\$\$[\s\S]*?\$\$|\$[^$]*?\$)/g)
    
    return parts.map((part, index) => {
      if (part.startsWith('$$') && part.endsWith('$$')) {
        // Block math (display mode)
        const mathContent = part.slice(2, -2).trim()
        return (
          <div key={index} className="my-4">
            <BlockMath math={mathContent} />
          </div>
        )
      } else if (part.startsWith('$') && part.endsWith('$') && part.length > 2) {
        // Inline math
        const mathContent = part.slice(1, -1).trim()
        return <InlineMath key={index} math={mathContent} />
      } else {
        // Regular text
        return part
      }
    })
  }

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  const getFileIcon = (type: string) => {
    if (type.startsWith('image/')) return <Image className="h-4 w-4" />
    if (type.startsWith('text/')) return <FileText className="h-4 w-4" />
    return <File className="h-4 w-4" />
  }

  const renderFileContent = (file: UploadedFile) => {
    if (file.type.startsWith('image/') && file.url) {
      return (
        <div className="mt-2">
          <img 
            src={file.url} 
            alt={file.name}
            className="max-w-full h-auto rounded-lg border border-gray-200 dark:border-gray-600"
            style={{ maxHeight: '300px' }}
          />
        </div>
      )
    } else if (file.content && file.type.startsWith('text/')) {
      return (
        <div className="mt-2 p-3 bg-gray-100 dark:bg-gray-700 rounded-lg border border-gray-200 dark:border-gray-600">
          <div className="text-xs text-gray-500 dark:text-gray-400 mb-2 font-mono">
            {file.name} ({formatFileSize(file.size)})
          </div>
          <pre className="text-sm text-gray-800 dark:text-gray-200 whitespace-pre-wrap overflow-x-auto">
            {file.content}
          </pre>
        </div>
      )
    }
    return null
  }

  const getMessageIcon = (role: string) => {
    switch (role) {
      case 'user':
        return <User className="h-5 w-5 text-blue-600 dark:text-blue-400" />
      case 'assistant':
        return <Bot className="h-5 w-5 text-gray-600 dark:text-gray-400" />
      case 'system':
        return <Settings className="h-5 w-5 text-yellow-600 dark:text-yellow-400" />
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
        <div className="flex items-center justify-center h-full text-gray-500 dark:text-gray-400">
          <div className="text-center">
            <Bot className="h-12 w-12 mx-auto mb-4 text-gray-400 dark:text-gray-500" />
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
                <div className="text-gray-800 dark:text-gray-200 [&_h1]:text-gray-900 dark:[&_h1]:text-gray-100 [&_h2]:text-gray-900 dark:[&_h2]:text-gray-100 [&_h3]:text-gray-900 dark:[&_h3]:text-gray-100 [&_h4]:text-gray-900 dark:[&_h4]:text-gray-100 [&_h5]:text-gray-900 dark:[&_h5]:text-gray-100 [&_h6]:text-gray-900 dark:[&_h6]:text-gray-100 [&_p]:text-gray-800 dark:[&_p]:text-gray-200 [&_strong]:text-gray-900 dark:[&_strong]:text-gray-100 [&_em]:text-gray-800 dark:[&_em]:text-gray-200 [&_a]:text-blue-600 dark:[&_a]:text-blue-400 [&_blockquote]:text-gray-700 dark:[&_blockquote]:text-gray-300 [&_blockquote]:border-gray-300 dark:[&_blockquote]:border-gray-600 [&_ul]:text-gray-800 dark:[&_ul]:text-gray-200 [&_ol]:text-gray-800 dark:[&_ol]:text-gray-200 [&_li]:text-gray-800 dark:[&_li]:text-gray-200">
                  {message.role === 'assistant' ? (
                    <ReactMarkdown
                      components={{
                        code({ node, className, children, ...props }: any) {
                          const match = /language-(\w+)/.exec(className || '')
                          return !props.inline && match ? (
                            <SyntaxHighlighter
                              style={codeTheme}
                              language={match[1]}
                              PreTag="div"
                              {...props}
                            >
                              {String(children).replace(/\n$/, '')}
                            </SyntaxHighlighter>
                          ) : (
                            <code className={`${className} bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200 px-1 py-0.5 rounded text-sm`} {...props}>
                              {children}
                            </code>
                          )
                        },
                        p({ children, ...props }: any) {
                          return <p {...props}>{renderMathContent(String(children))}</p>
                        },
                        span({ children, ...props }: any) {
                          return <span {...props}>{renderMathContent(String(children))}</span>
                        }
                      }}
                    >
                      {message.content}
                    </ReactMarkdown>
                  ) : (
                    <ReactMarkdown
                      components={{
                        code({ node, className, children, ...props }: any) {
                          const match = /language-(\w+)/.exec(className || '')
                          return !props.inline && match ? (
                            <SyntaxHighlighter
                              style={codeTheme}
                              language={match[1]}
                              PreTag="div"
                              {...props}
                            >
                              {String(children).replace(/\n$/, '')}
                            </SyntaxHighlighter>
                          ) : (
                            <code className={`${className} bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200 px-1 py-0.5 rounded text-sm`} {...props}>
                              {children}
                            </code>
                          )
                        },
                        p({ children, ...props }: any) {
                          return <p {...props}>{renderMathContent(String(children))}</p>
                        },
                        span({ children, ...props }: any) {
                          return <span {...props}>{renderMathContent(String(children))}</span>
                        }
                      }}
                    >
                      {message.content}
                    </ReactMarkdown>
                  )}
                  
                  {/* File Attachments */}
                  {message.files && message.files.length > 0 && (
                    <div className="mt-3 space-y-2">
                      {message.files.map((file) => (
                        <div key={file.id} className="flex items-start space-x-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-600">
                          <div className="flex-shrink-0 mt-1">
                            {getFileIcon(file.type)}
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center justify-between">
                              <div className="text-sm font-medium text-gray-900 dark:text-gray-100 truncate">
                                {file.name}
                              </div>
                              <div className="text-xs text-gray-500 dark:text-gray-400 ml-2">
                                {formatFileSize(file.size)}
                              </div>
                            </div>
                            {renderFileContent(file)}
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
                <div className="mt-2 text-xs text-gray-500 dark:text-gray-400">
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
