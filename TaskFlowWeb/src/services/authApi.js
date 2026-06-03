import { apiClient } from './apiClient'

export const authApi = {
  setToken(token) {
    apiClient.defaults.headers.common['Authorization'] = token ? `Bearer ${token}` : ''
  },

  async login({ email, password }) {
    const res = await apiClient.post('/api/auth/login', { email, password })
    return res.data
  },

  async register({ fullName, email, password }) {
    const res = await apiClient.post('/api/auth/register', { fullName, email, password })
    return res.data
  },
}

