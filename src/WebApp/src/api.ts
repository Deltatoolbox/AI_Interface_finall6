export const API_BASE_URL = 'http://localhost:5058'

export const api = {
  async getModels() {
    const response = await fetch(`${API_BASE_URL}/api/models`, {
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Failed to fetch models')
    return response.json()
  },

  async login(username: string, password: string) {
    console.log('API: Attempting login with:', username)
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ username, password }),
    })
    
    console.log('API: Login response status:', response.status)
    
    if (!response.ok) {
      const errorText = await response.text()
      console.error('API: Login failed with status:', response.status, 'Error:', errorText)
      throw new Error(`Login failed: ${response.status}`)
    }
    
    const data = await response.json()
    console.log('API: Login response data:', data)
    return data
  },

  async register(username: string, password: string, email: string) {
    const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ username, password, email }),
    })
    if (!response.ok) throw new Error('Registration failed')
    return response.json()
  },

  async logout() {
    const response = await fetch(`${API_BASE_URL}/api/auth/logout`, {
      method: 'POST',
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Logout failed')
  },

  // Admin User Management APIs
  async getUsers() {
    const response = await fetch(`${API_BASE_URL}/api/admin/users`, {
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to fetch users')
    return response.json()
  },

  async createUser(username: string, password: string, email: string, role: string) {
    const response = await fetch(`${API_BASE_URL}/api/admin/users`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ username, password, email, role }),
    })
    if (!response.ok) throw new Error('Failed to create user')
    return response.json()
  },

  async updateUser(userId: string, username?: string, email?: string, role?: string) {
    const response = await fetch(`${API_BASE_URL}/api/admin/users/${userId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ username, email, role }),
    })
    if (!response.ok) throw new Error('Failed to update user')
    return response.json()
  },

  async deleteUser(userId: string) {
    const response = await fetch(`${API_BASE_URL}/api/admin/users/${userId}`, {
      method: 'DELETE',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to delete user')
  },

  async resetPassword(username: string, newPassword: string) {
    const response = await fetch(`${API_BASE_URL}/api/admin/reset-password`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ username, newPassword }),
    })
    if (!response.ok) throw new Error('Failed to reset password')
    return response.json()
  },

  async getCsrfToken() {
    const response = await fetch(`${API_BASE_URL}/api/auth/csrf`, {
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Failed to get CSRF token')
    return response.json()
  },

  async sendChat(model: string, messages: any[], conversationId?: string) {
    const response = await fetch(`${API_BASE_URL}/api/chat`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ model, messages, conversationId }),
    })
    if (!response.ok) throw new Error('Chat request failed')
    return response.json()
  },

  async updateConversationTitle(conversationId: string, newTitle: string) {
    const response = await fetch(`${API_BASE_URL}/api/conversations/${conversationId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ title: newTitle }),
    })
    if (!response.ok) throw new Error('Failed to update conversation title')
    return response.json()
  },

  // Export/Import APIs
  async exportConversations() {
    const response = await fetch(`${API_BASE_URL}/api/conversations/export`, {
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to export conversations')
    return response.json()
  },

  async importConversations(conversations: any[]) {
    const response = await fetch(`${API_BASE_URL}/api/conversations/import`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ conversations }),
    })
    if (!response.ok) throw new Error('Failed to import conversations')
    return response.json()
  },

  // Search API
  async searchMessages(query: string, limit?: number, offset?: number) {
    const response = await fetch(`${API_BASE_URL}/api/search`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ query, limit, offset }),
    })
    if (!response.ok) throw new Error('Failed to search messages')
    return response.json()
  },

  // Chat Sharing API
  async createShare(conversationId: string, password?: string, expiresAt?: Date) {
    const response = await fetch(`${API_BASE_URL}/api/shares`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ conversationId, password, expiresAt }),
    })
    if (!response.ok) throw new Error('Failed to create share')
    return response.json()
  },

  async getSharedConversation(shareId: string, password?: string) {
    const url = new URL(`${API_BASE_URL}/api/shares/${shareId}`)
    if (password) url.searchParams.set('password', password)
    
    const response = await fetch(url.toString(), {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get shared conversation')
    return response.json()
  },

  async revokeShare(shareId: string) {
    const response = await fetch(`${API_BASE_URL}/api/shares/${shareId}`, {
      method: 'DELETE',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to revoke share')
    return response.json()
  },

  async getUserShares() {
    const response = await fetch(`${API_BASE_URL}/api/shares`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get user shares')
    return response.json()
  },

  // Chat Templates API
  async getTemplates() {
    const response = await fetch(`${API_BASE_URL}/api/templates`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get templates')
    return response.json()
  },

  async getTemplateCategories() {
    const response = await fetch(`${API_BASE_URL}/api/templates/categories`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get template categories')
    return response.json()
  },

  async getTemplateById(templateId: string) {
    const response = await fetch(`${API_BASE_URL}/api/templates/${templateId}`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get template')
    return response.json()
  },

  async getTemplatesByCategory(category: string) {
    const response = await fetch(`${API_BASE_URL}/api/templates/category/${category}`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get templates by category')
    return response.json()
  },

  async createTemplate(template: { name: string; description: string; category: string; systemPrompt: string; exampleMessages: string[] }) {
    const response = await fetch(`${API_BASE_URL}/api/templates`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(template),
    })
    if (!response.ok) throw new Error('Failed to create template')
    return response.json()
  },

  async updateTemplate(templateId: string, template: { name: string; description: string; category: string; systemPrompt: string; exampleMessages: string[] }) {
    const response = await fetch(`${API_BASE_URL}/api/templates/${templateId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(template),
    })
    if (!response.ok) throw new Error('Failed to update template')
    return response.json()
  },

  async deleteTemplate(templateId: string) {
    const response = await fetch(`${API_BASE_URL}/api/templates/${templateId}`, {
      method: 'DELETE',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to delete template')
    return response.json()
  },

  async seedBuiltInTemplates() {
    const response = await fetch(`${API_BASE_URL}/api/templates/seed`, {
      method: 'POST',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to seed built-in templates')
    return response.json()
  }
}
