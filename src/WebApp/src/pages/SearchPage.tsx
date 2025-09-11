import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Search, ArrowLeft, MessageSquare, Calendar } from 'lucide-react'
import { api } from '../api'

interface SearchResult {
  conversationId: string
  conversationTitle: string
  messageId: string
  role: string
  content: string
  createdAt: string
  highlightedContent: string[]
}

interface SearchResponse {
  results: SearchResult[]
  totalCount: number
}

export default function SearchPage() {
  const navigate = useNavigate()
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<SearchResult[]>([])
  const [isSearching, setIsSearching] = useState(false)
  const [error, setError] = useState('')
  const [hasSearched, setHasSearched] = useState(false)

  const handleSearch = async (searchQuery: string) => {
    if (!searchQuery.trim()) {
      setResults([])
      setHasSearched(false)
      return
    }

    setIsSearching(true)
    setError('')
    setHasSearched(true)

    try {
      const response: SearchResponse = await api.searchMessages(searchQuery, 50)
      setResults(response.results)
    } catch (err) {
      setError('Failed to search messages')
      console.error('Search error:', err)
    } finally {
      setIsSearching(false)
    }
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    handleSearch(query)
  }

  const handleResultClick = (conversationId: string) => {
    navigate(`/chat/${conversationId}`)
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('de-DE', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  const renderHighlightedContent = (highlightedContent: string[]) => {
    return highlightedContent.map((part, index) => {
      if (part.startsWith('<mark>') && part.endsWith('</mark>')) {
        return (
          <mark key={index} className="bg-yellow-200 dark:bg-yellow-800 px-1 rounded">
            {part.slice(6, -7)}
          </mark>
        )
      }
      return <span key={index}>{part}</span>
    })
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* Header */}
      <div className="bg-white dark:bg-gray-800 shadow-sm border-b border-gray-200 dark:border-gray-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center">
              <button
                onClick={() => navigate('/chat')}
                className="mr-4 p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <ArrowLeft className="h-5 w-5" />
              </button>
              <h1 className="text-xl font-semibold text-gray-900 dark:text-gray-100">
                Search Messages
              </h1>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Search Form */}
        <div className="mb-8">
          <form onSubmit={handleSubmit} className="relative">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
              <input
                type="text"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                placeholder="Search through all your messages..."
                className="w-full pl-10 pr-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 placeholder-gray-500 dark:placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
            <button
              type="submit"
              disabled={isSearching || !query.trim()}
              className="mt-4 w-full bg-blue-600 dark:bg-blue-700 text-white py-2 px-4 rounded-lg hover:bg-blue-700 dark:hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isSearching ? 'Searching...' : 'Search'}
            </button>
          </form>
        </div>

        {/* Error Message */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
            <p className="text-red-800 dark:text-red-200">{error}</p>
          </div>
        )}

        {/* Search Results */}
        {hasSearched && (
          <div className="space-y-6">
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-medium text-gray-900 dark:text-gray-100">
                Search Results
              </h2>
              <span className="text-sm text-gray-500 dark:text-gray-400">
                {results.length} result{results.length !== 1 ? 's' : ''} found
              </span>
            </div>

            {results.length === 0 ? (
              <div className="text-center py-12">
                <MessageSquare className="mx-auto h-12 w-12 text-gray-400" />
                <h3 className="mt-2 text-sm font-medium text-gray-900 dark:text-gray-100">
                  No messages found
                </h3>
                <p className="mt-1 text-sm text-gray-500 dark:text-gray-400">
                  Try searching with different keywords.
                </p>
              </div>
            ) : (
              <div className="space-y-4">
                {results.map((result) => (
                  <div
                    key={result.messageId}
                    onClick={() => handleResultClick(result.conversationId)}
                    className="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-4 hover:shadow-md hover:border-blue-300 dark:hover:border-blue-600 cursor-pointer transition-all"
                  >
                    <div className="flex items-start justify-between mb-2">
                      <div className="flex items-center space-x-2">
                        <div className={`px-2 py-1 rounded text-xs font-medium ${
                          result.role === 'user' 
                            ? 'bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200' 
                            : 'bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200'
                        }`}>
                          {result.role === 'user' ? 'You' : 'AI'}
                        </div>
                        <span className="text-sm font-medium text-gray-900 dark:text-gray-100">
                          {result.conversationTitle}
                        </span>
                      </div>
                      <div className="flex items-center text-xs text-gray-500 dark:text-gray-400">
                        <Calendar className="h-3 w-3 mr-1" />
                        {formatDate(result.createdAt)}
                      </div>
                    </div>
                    
                    <div className="text-sm text-gray-700 dark:text-gray-300">
                      {renderHighlightedContent(result.highlightedContent)}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
