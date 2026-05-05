import api from './api'

export const playerService = {
  async getPlayerById(playerId) {
    const response = await api.get(`/players/${playerId}`)
    return response.data
  },

  async updateProfile(profileData) {
    const response = await api.put('/players/profile', profileData)
    return response.data
  },

  async updateStats(statsData) {
    const response = await api.put('/players/stats', statsData)
    return response.data
  },

  async addToInventory(item) {
    const response = await api.post('/players/inventory', item)
    return response.data
  },

  async getLeaderboard() {
    const response = await api.get('/players/leaderboard/all')
    return response.data
  }
}
