import { useEffect, useMemo, useRef } from 'react'
import { MathJax, MathJaxContext } from 'better-react-mathjax'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import remarkMath from 'remark-math'
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter'
import { tomorrow, vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism'
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

  const mathJaxConfig = useMemo(
    () => ({
      tex: {
        inlineMath: [
          ['$', '$'],
          ['\\(', '\\)']
        ],
        displayMath: [
          ['$$', '$$'],
          ['\\[', '\\]']
        ],
        processEscapes: true
      },
      options: {
        skipHtmlTags: ['script', 'noscript', 'style', 'textarea', 'pre', 'code'],
        ignoreHtmlClass: 'no-mathjax',
        processHtmlClass: 'mathjax-process'
      }
    }),
    []
  )

  // Clean up GPT-style LaTeX artifacts and normalize math so KaTeX can render it
  const normalizeMathText = (text: string) => {
    if (!text || typeof text !== 'string') return text

    // Heuristics: detect common LaTeX commands
    const latexCommandPattern = /\\(frac|tfrac|dfrac|sqrt|sum|prod|int|lim|infty|alpha|beta|gamma|pi|cdot|times|leq|geq|neq|approx|theta|lambda|mu|nu|rho|sigma|tau|phi|psi|omega|sin|cos|tan|log|ln|text|Omega|Delta|pm)/
    const latexInlinePattern = /\\[A-Za-z]+|[_^][{]|\\pm|\\cdot|\\times|\\frac|\\sqrt/

    const stripIndentedMathLines = (input: string) => {
      return input
        .split('\n')
        .map((line) => {
          if (!/^\s{4,}/.test(line)) return line
          const trimmed = line.trim()
          if (trimmed.startsWith('$$') || latexCommandPattern.test(trimmed) || latexInlinePattern.test(trimmed)) {
            return trimmed
          }
          return line
        })
        .join('\n')
    }

    const wrapParentheticalMath = (input: string) => {
      return input
        .split('\n')
        .map((line) => {
          if (line.includes('$')) return line
          return line.replace(/\(([^()\n]{2,})\)/g, (match, inner) => {
            const trimmedInner = inner.trim()
            if (!latexCommandPattern.test(trimmedInner) && !latexInlinePattern.test(trimmedInner)) {
              return match
            }
            const cleanedInner = trimmedInner.replace(/[;,]/g, ' ')
            return `$${cleanedInner}$`
          })
        })
        .join('\n')
    }

    // First, sanitize LaTeX segments that are already in $...$ or $$...$$
    const sanitizeDelimitedMath = (input: string) => {
      if (!input.includes('$')) return input
      return input.replace(/(\$\$[\s\S]*?\$\$|\$[^$]*\$)/g, (match) => {
        const isBlock = match.startsWith('$$')
        const inner = match.slice(isBlock ? 2 : 1, isBlock ? -2 : -1)
        // Replace stray commas/semicolons (often used as visual separators) with spaces
        const cleanedInner = inner.replace(/[;,]/g, ' ')
        if (isBlock) {
          // Ensure block math is surrounded by blank lines so Markdown renders it as block
          return `\n\n$$${cleanedInner.trim()}$$\n\n`
        }
        return `$${cleanedInner.trim()}$`
      })
    }

    let normalized = stripIndentedMathLines(text)
    normalized = wrapParentheticalMath(normalized)
    normalized = sanitizeDelimitedMath(normalized)

    // If we already have math delimiters after sanitizing, just return
    if (normalized.includes('$')) {
      return normalized
    }

    // If there are no obvious LaTeX commands, don't touch the text
    if (!latexCommandPattern.test(normalized)) {
      return normalized
    }

    // Convert bracketed expressions [ ... ] into block math $$ ... $$
    normalized = normalized.replace(/\[(.+?)\]/gs, (match, inner) => {
      if (latexCommandPattern.test(inner)) {
        const cleanedInner = inner.replace(/[;,]/g, ' ')
        return `$$${cleanedInner.trim()}$$`
      }
      return match
    })

    // If we produced $$...$$ blocks, return them
    if (normalized.includes('$$')) {
      return normalized
    }

    // Otherwise wrap the whole expression in inline math
    const cleaned = normalized.replace(/[;,]/g, ' ')
    return `$${cleaned.trim()}$`
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

  const markdownComponents = {
    math({ value }: any) {
      if (typeof value !== 'string') return null
      return (
        <MathJax dynamic hideUntilTypeset="first">
          {`\\[${value}\\]`}
        </MathJax>
      )
    },
    inlineMath({ value }: any) {
      if (typeof value !== 'string') return null
      return (
        <MathJax inline dynamic hideUntilTypeset="first">
          {`\\(${value}\\)`}
        </MathJax>
      )
    },
    code({ node, className, children, ...props }: any) {
      const match = /language-(\w+)/.exec(className || '')
      return !props.inline && match ? (
        <SyntaxHighlighter style={codeTheme} language={match[1]} PreTag="div" {...props}>
          {String(children).replace(/\n$/, '')}
        </SyntaxHighlighter>
      ) : (
        <code className={`${className} bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200 px-1 py-0.5 rounded text-sm`} {...props}>
          {children}
        </code>
      )
    },
    table({ children, ...props }: any) {
      return (
        <div className="overflow-x-auto my-4 rounded-lg border border-gray-300 dark:border-gray-600">
          <table className="min-w-full border-collapse" {...props}>
            {children}
          </table>
        </div>
      )
    },
    thead({ children, ...props }: any) {
      return (
        <thead className="bg-gray-50 dark:bg-gray-700" {...props}>
          {children}
        </thead>
      )
    },
    tbody({ children, ...props }: any) {
      return <tbody {...props}>{children}</tbody>
    },
    tr({ children, ...props }: any) {
      return (
        <tr className="border-b border-gray-200 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-800" {...props}>
          {children}
        </tr>
      )
    },
    th({ children, ...props }: any) {
      return (
        <th className="border-r border-gray-200 dark:border-gray-600 bg-gray-50 dark:bg-gray-700 px-4 py-3 text-left font-semibold text-gray-900 dark:text-gray-100" {...props}>
          {children}
        </th>
      )
    },
    td({ children, ...props }: any) {
      return (
        <td className="border-r border-gray-200 dark:border-gray-600 px-4 py-3 text-gray-800 dark:text-gray-200" {...props}>
          {children}
        </td>
      )
    }
  }

  return (
    <MathJaxContext version={3} config={mathJaxConfig}>
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
                  <ReactMarkdown remarkPlugins={[remarkMath, remarkGfm]} components={markdownComponents}>
                    {normalizeMathText(message.content)}
                  </ReactMarkdown>
                  
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
    </MathJaxContext>
  )
}
