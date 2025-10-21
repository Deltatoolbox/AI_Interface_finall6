export const API_BASE_URL = ''

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
    const response = await fetch(`${API_BASE_URL}/api/users/with-roles`, {
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

  // Role Management APIs
  async getRoles() {
    const response = await fetch(`${API_BASE_URL}/api/roles`, {
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to fetch roles')
    return response.json()
  },

  async getRolePermissions() {
    const response = await fetch(`${API_BASE_URL}/api/roles/permissions`, {
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to fetch permissions')
    return response.json()
  },

  async createRole(name: string, description: string, permissions: string[]) {
    const response = await fetch(`${API_BASE_URL}/api/roles`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ name, description, permissions }),
    })
    if (!response.ok) throw new Error('Failed to create role')
    return response.json()
  },

  async updateRole(roleId: string, name: string, description: string, permissions: string[]) {
    const response = await fetch(`${API_BASE_URL}/api/roles/${roleId}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ name, description, permissions }),
    })
    if (!response.ok) throw new Error('Failed to update role')
    return response.json()
  },

  async deleteRole(roleId: string) {
    const response = await fetch(`${API_BASE_URL}/api/roles/${roleId}`, {
      method: 'DELETE',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to delete role')
    return response.json()
  },

  async assignRoleToUser(userId: string, roleId: string) {
    const response = await fetch(`${API_BASE_URL}/api/roles/assign`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ userId, roleId }),
    })
    if (!response.ok) throw new Error('Failed to assign role')
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

  async deleteConversation(conversationId: string) {
    const response = await fetch(`${API_BASE_URL}/api/conversations/${conversationId}`, {
      method: 'DELETE',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to delete conversation')
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
    let url: string
    if (password) {
      const urlObj = new URL(`${API_BASE_URL}/api/shares/${shareId}`, window.location.origin)
      urlObj.searchParams.set('password', password)
      url = urlObj.toString()
    } else {
      url = `${API_BASE_URL}/api/shares/${shareId}`
    }
    
    const response = await fetch(url, {
      method: 'GET',
      credentials: 'include',
    })
    
    if (!response.ok) {
      if (response.status === 401) {
        const errorData = await response.json().catch(() => ({ error: 'Unauthorized' }))
        throw new Error(errorData.error || 'Password required')
      }
      if (response.status === 404) {
        throw new Error('Share not found or expired')
      }
      throw new Error('Failed to get shared conversation')
    }
    
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
  },

  // Backup/Restore API
  async getBackups() {
    const response = await fetch(`${API_BASE_URL}/api/admin/backups`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get backups')
    return response.json()
  },

  async createBackup(name: string, description?: string) {
    const response = await fetch(`${API_BASE_URL}/api/admin/backups`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ name, description }),
    })
    if (!response.ok) throw new Error('Failed to create backup')
    return response.json()
  },

  async restoreBackup(backupId: string) {
    const response = await fetch(`${API_BASE_URL}/api/admin/backups/${backupId}/restore`, {
      method: 'POST',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to restore backup')
    return response.json()
  },

  async deleteBackup(backupId: string) {
    const response = await fetch(`${API_BASE_URL}/api/admin/backups/${backupId}`, {
      method: 'DELETE',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to delete backup')
    return response.json()
  },

  // Webhook Management API
  async getWebhooks() {
    const response = await fetch(`${API_BASE_URL}/api/webhooks`, {
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Failed to fetch webhooks')
    return response.json()
  },

  async createWebhook(webhookData: any) {
    const response = await fetch(`${API_BASE_URL}/api/webhooks`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(webhookData)
    })
    if (!response.ok) throw new Error('Failed to create webhook')
    return response.json()
  },

  async updateWebhook(id: string, webhookData: any) {
    const response = await fetch(`${API_BASE_URL}/api/webhooks/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(webhookData)
    })
    if (!response.ok) throw new Error('Failed to update webhook')
    return response.json()
  },

  async deleteWebhook(id: string) {
    const response = await fetch(`${API_BASE_URL}/api/webhooks/${id}`, {
      method: 'DELETE',
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Failed to delete webhook')
    return response.json()
  },

  async testWebhook(testData: any) {
    const response = await fetch(`${API_BASE_URL}/api/webhooks/test`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(testData)
    })
    if (!response.ok) throw new Error('Failed to test webhook')
    return response.json()
  },

  async getWebhookDeliveries(id: string) {
    const response = await fetch(`${API_BASE_URL}/api/webhooks/${id}/deliveries`, {
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Failed to fetch webhook deliveries')
    return response.json()
  },

  // Integration Management API
  async getSlackChannels() {
    const response = await fetch(`${API_BASE_URL}/api/integrations/slack/channels`, {
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Failed to fetch Slack channels')
    return response.json()
  },

  async sendSlackMessage(messageData: any) {
    const response = await fetch(`${API_BASE_URL}/api/integrations/slack/send`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(messageData)
    })
    if (!response.ok) throw new Error('Failed to send Slack message')
    return response.json()
  },

  async getDiscordChannels() {
    const response = await fetch(`${API_BASE_URL}/api/integrations/discord/channels`, {
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Failed to fetch Discord channels')
    return response.json()
  },

  async sendDiscordMessage(messageData: any) {
    const response = await fetch(`${API_BASE_URL}/api/integrations/discord/send`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify(messageData)
    })
    if (!response.ok) throw new Error('Failed to send Discord message')
    return response.json()
  },

  // Integration Configuration APIs
  async configureSlack(botToken: string, webhookUrl?: string, channels?: string[]) {
    const response = await fetch(`${API_BASE_URL}/api/integrations/slack/configure`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ botToken, webhookUrl, channels })
    })
    if (!response.ok) throw new Error('Failed to configure Slack')
    return response.json()
  },

  async configureDiscord(botToken: string, webhookUrl?: string, channelIds?: string[]) {
    const response = await fetch(`${API_BASE_URL}/api/integrations/discord/configure`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      credentials: 'include',
      body: JSON.stringify({ botToken, webhookUrl, channelIds })
    })
    if (!response.ok) throw new Error('Failed to configure Discord')
    return response.json()
  },

  async getSlackStatus() {
    const response = await fetch(`${API_BASE_URL}/api/integrations/slack/status`, {
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Failed to get Slack status')
    return response.json()
  },

  async getDiscordStatus() {
    const response = await fetch(`${API_BASE_URL}/api/integrations/discord/status`, {
      credentials: 'include'
    })
    if (!response.ok) throw new Error('Failed to get Discord status')
    return response.json()
  },

  async downloadBackup(backupId: string) {
    const response = await fetch(`${API_BASE_URL}/api/admin/backups/${backupId}/download`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to download backup')
    return response.blob()
  },

  async uploadBackup(file: File, name: string, description?: string) {
    const formData = new FormData()
    formData.append('backup', file)
    formData.append('name', name)
    if (description) formData.append('description', description)

    const response = await fetch(`${API_BASE_URL}/api/admin/backups/upload`, {
      method: 'POST',
      credentials: 'include',
      body: formData,
    })
    if (!response.ok) throw new Error('Failed to upload backup')
    return response.json()
  },

  // Health Monitoring API
  async getSystemHealth() {
    const response = await fetch(`${API_BASE_URL}/api/health`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get system health')
    return response.json()
  },

  async getSystemMetrics() {
    const response = await fetch(`${API_BASE_URL}/api/health/metrics`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get system metrics')
    return response.json()
  },

  async getServiceStatuses() {
    const response = await fetch(`${API_BASE_URL}/api/health/services`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get service statuses')
    return response.json()
  },

  async checkServiceHealth(serviceName: string) {
    const response = await fetch(`${API_BASE_URL}/api/health/check/${serviceName}`, {
      method: 'POST',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to check service health')
    return response.json()
  },

  // Audit Trail API
  async getAuditLogs(filter: AuditLogFilter) {
    const params = new URLSearchParams()
    if (filter.userId) params.append('UserId', filter.userId)
    if (filter.action) params.append('Action', filter.action)
    if (filter.resource) params.append('Resource', filter.resource)
    if (filter.startDate) params.append('StartDate', filter.startDate.toISOString())
    if (filter.endDate) params.append('EndDate', filter.endDate.toISOString())
    params.append('Page', (filter.page || 1).toString())
    params.append('PageSize', (filter.pageSize || 50).toString())

    const response = await fetch(`${API_BASE_URL}/api/audit/logs?${params}`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get audit logs')
    return response.json()
  },

  async getAvailableActions() {
    const response = await fetch(`${API_BASE_URL}/api/audit/actions`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get available actions')
    return response.json()
  },

  async getAvailableResources() {
    const response = await fetch(`${API_BASE_URL}/api/audit/resources`, {
      method: 'GET',
      credentials: 'include',
    })
    if (!response.ok) throw new Error('Failed to get available resources')
    return response.json()
  }
}

export interface AuditLogDto {
  id: string
  userId: string
  userName: string
  action: string
  resource: string
  details: string
  timestamp: string
  ipAddress: string
  userAgent: string
}

export interface AuditLogFilter {
  userId?: string
  action?: string
  resource?: string
  startDate?: Date
  endDate?: Date
  page?: number
  pageSize?: number
}

export interface AuditLogResponse {
  logs: AuditLogDto[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}
