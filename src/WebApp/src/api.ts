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
  }
}
