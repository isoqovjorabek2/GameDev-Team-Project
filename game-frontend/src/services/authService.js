import api from './api'

export const authService = {
  async login(username, password) {
    const response = await api.post('/auth/login', { username, password })
    return response.data
  },

  async register(username, email, password) {
    const response = await api.post('/auth/register', {
      username,
      email,
      password
    })
    return response.data
  },

  async getCurrentPlayer() {
    const response = await api.get('/auth/me')
    return response.data
  }
}
