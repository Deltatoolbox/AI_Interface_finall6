import React, { useState, useRef, useEffect } from 'react'
import { Send, Square, Bold, Italic, Code, List, Link, Calculator, Upload, X, FileText, Image, File } from 'lucide-react'

interface UploadedFile {
  id: string
  name: string
  size: number
  type: string
  url?: string
  content?: string
}

interface MessageInputProps {
  onSendMessage: (message: string, files?: UploadedFile[]) => void
  disabled?: boolean
  onStop?: () => void
}

export function MessageInput({ onSendMessage, disabled = false, onStop }: MessageInputProps) {
  const [message, setMessage] = useState('')
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([])
  const [isUploading, setIsUploading] = useState(false)
  const textareaRef = useRef<HTMLTextAreaElement>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto'
      textareaRef.current.style.height = `${textareaRef.current.scrollHeight}px`
    }
  }, [message])

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if ((message.trim() || uploadedFiles.length > 0) && !disabled) {
      onSendMessage(message.trim(), uploadedFiles.length > 0 ? uploadedFiles : undefined)
      setMessage('')
      setUploadedFiles([])
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSubmit(e)
    }
    
    // Keyboard shortcuts for formatting
    if (e.ctrlKey || e.metaKey) {
      switch (e.key) {
        case 'b':
          e.preventDefault()
          formatBold()
          break
        case 'i':
          e.preventDefault()
          formatItalic()
          break
        case 'k':
          e.preventDefault()
          formatLink()
          break
        case 'm':
          e.preventDefault()
          formatInlineMath()
          break
        case 'M':
          e.preventDefault()
          formatBlockMath()
          break
      }
    }
  }

  const insertText = (before: string, after: string = '', placeholder: string = '') => {
    const textarea = textareaRef.current
    if (!textarea) return

    const start = textarea.selectionStart
    const end = textarea.selectionEnd
    const selectedText = message.substring(start, end)
    const textToInsert = selectedText || placeholder

    const newText = message.substring(0, start) + before + textToInsert + after + message.substring(end)
    setMessage(newText)

    // Set cursor position after inserted text
    setTimeout(() => {
      const newCursorPos = start + before.length + textToInsert.length
      textarea.setSelectionRange(newCursorPos, newCursorPos)
      textarea.focus()
    }, 0)
  }

  const formatBold = () => insertText('**', '**', 'bold text')
  const formatItalic = () => insertText('*', '*', 'italic text')
  const formatCode = () => insertText('`', '`', 'code')
  const formatCodeBlock = () => insertText('```\n', '\n```', 'code block')
  const formatList = () => insertText('- ', '', 'list item')
  const formatLink = () => insertText('[', '](url)', 'link text')
  const formatInlineMath = () => insertText('$', '$', 'x^2 + y^2 = z^2')
  const formatBlockMath = () => insertText('$$\n', '\n$$', '\\frac{a}{b} = \\frac{c}{d}')

  const handleFileUpload = async (files: FileList) => {
    setIsUploading(true)
    const newFiles: UploadedFile[] = []

    for (let i = 0; i < files.length; i++) {
      const file = files[i]
      
      // Validate file size (max 10MB)
      if (file.size > 10 * 1024 * 1024) {
        alert(`File ${file.name} is too large. Maximum size is 10MB.`)
        continue
      }

      // Validate file type
      const allowedTypes = [
        'image/jpeg', 'image/png', 'image/gif', 'image/webp',
        'text/plain', 'text/markdown', 'application/pdf',
        'application/json', 'text/csv'
      ]
      
      if (!allowedTypes.includes(file.type)) {
        alert(`File type ${file.type} is not supported.`)
        continue
      }

      const fileId = Math.random().toString(36).substr(2, 9)
      
      try {
        let content: string | undefined
        let url: string | undefined

        if (file.type.startsWith('image/')) {
          // For images, create object URL
          url = URL.createObjectURL(file)
        } else if (file.type.startsWith('text/') || file.type === 'application/json') {
          // For text files, read content
          content = await file.text()
        }

        newFiles.push({
          id: fileId,
          name: file.name,
          size: file.size,
          type: file.type,
          url,
          content
        })
      } catch (error) {
        console.error('Error processing file:', error)
        alert(`Error processing file ${file.name}`)
      }
    }

    setUploadedFiles(prev => [...prev, ...newFiles])
    setIsUploading(false)
  }

  const removeFile = (fileId: string) => {
    setUploadedFiles(prev => {
      const file = prev.find(f => f.id === fileId)
      if (file?.url) {
        URL.revokeObjectURL(file.url)
      }
      return prev.filter(f => f.id !== fileId)
    })
  }

  const getFileIcon = (type: string) => {
    if (type.startsWith('image/')) return <Image className="h-4 w-4" />
    if (type.startsWith('text/')) return <FileText className="h-4 w-4" />
    return <File className="h-4 w-4" />
  }

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
  }

  return (
    <div className="space-y-2">
      {/* Formatting Toolbar */}
      <div className="flex space-x-1 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
        <button
          type="button"
          onClick={formatBold}
          disabled={disabled}
          className="p-2 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          title="Bold (Ctrl+B)"
        >
          <Bold className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={formatItalic}
          disabled={disabled}
          className="p-2 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          title="Italic (Ctrl+I)"
        >
          <Italic className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={formatCode}
          disabled={disabled}
          className="p-2 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          title="Inline Code"
        >
          <Code className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={formatCodeBlock}
          disabled={disabled}
          className="p-2 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          title="Code Block"
        >
          <Code className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={formatList}
          disabled={disabled}
          className="p-2 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          title="List"
        >
          <List className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={formatLink}
          disabled={disabled}
          className="p-2 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          title="Link"
        >
          <Link className="h-4 w-4" />
        </button>
        <div className="w-px h-6 bg-gray-300 dark:bg-gray-600 mx-1"></div>
        <button
          type="button"
          onClick={formatInlineMath}
          disabled={disabled}
          className="p-2 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          title="Inline Math ($...$)"
        >
          <Calculator className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={formatBlockMath}
          disabled={disabled}
          className="p-2 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          title="Block Math ($$...$$)"
        >
          <Calculator className="h-4 w-4" />
        </button>
        <div className="w-px h-6 bg-gray-300 dark:bg-gray-600 mx-1"></div>
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          disabled={disabled || isUploading}
          className="p-2 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          title="Upload Files"
        >
          <Upload className="h-4 w-4" />
        </button>
      </div>

      {/* File Upload Area */}
      {uploadedFiles.length > 0 && (
        <div className="p-3 bg-gray-50 dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
              Attached Files ({uploadedFiles.length})
            </span>
            <button
              type="button"
              onClick={() => setUploadedFiles([])}
              className="text-xs text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200"
            >
              Clear All
            </button>
          </div>
          <div className="space-y-2">
            {uploadedFiles.map((file) => (
              <div key={file.id} className="flex items-center justify-between p-2 bg-white dark:bg-gray-700 rounded border border-gray-200 dark:border-gray-600">
                <div className="flex items-center space-x-2">
                  {getFileIcon(file.type)}
                  <div>
                    <div className="text-sm font-medium text-gray-900 dark:text-gray-100 truncate max-w-xs">
                      {file.name}
                    </div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">
                      {formatFileSize(file.size)}
                    </div>
                  </div>
                </div>
                <button
                  type="button"
                  onClick={() => removeFile(file.id)}
                  className="p-1 text-gray-400 hover:text-red-500 dark:hover:text-red-400"
                >
                  <X className="h-4 w-4" />
                </button>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Hidden File Input */}
      <input
        ref={fileInputRef}
        type="file"
        multiple
        accept="image/*,text/*,.pdf,.json,.csv,.md"
        onChange={(e) => e.target.files && handleFileUpload(e.target.files)}
        className="hidden"
      />

      <form onSubmit={handleSubmit} className="flex space-x-2">
        <div className="flex-1 relative">
          <textarea
            ref={textareaRef}
            value={message}
            onChange={(e) => setMessage(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder="Type your message... (Enter to send, Shift+Enter for new line)"
            disabled={disabled}
            className="w-full px-4 py-3 pr-12 border border-gray-300 dark:border-gray-600 rounded-lg resize-none bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed"
            rows={1}
            style={{ minHeight: '48px', maxHeight: '120px' }}
          />
        </div>
      
      <div className="flex space-x-2">
        {disabled ? (
          <button
            type="button"
            onClick={onStop}
            className="px-4 py-3 bg-red-600 dark:bg-red-700 text-white rounded-lg hover:bg-red-700 dark:hover:bg-red-600 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 transition-colors"
          >
            <Square className="h-5 w-5" />
          </button>
        ) : (
          <button
            type="submit"
            disabled={(!message.trim() && uploadedFiles.length === 0) || disabled}
            className="px-4 py-3 bg-blue-600 dark:bg-blue-700 text-white rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <Send className="h-5 w-5" />
          </button>
        )}
      </div>
      </form>
    </div>
  )
}
