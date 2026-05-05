import api from './api'

export const gameService = {
  async getAllGames() {
    const response = await api.get('/games')
    return response.data
  },

  async getGameById(gameId) {
    const response = await api.get(`/games/${gameId}`)
    return response.data
  },

  async createGame(gameData) {
    const response = await api.post('/games', gameData)
    return response.data
  },

  async createGameSession(gameId, settings) {
    const response = await api.post('/games/sessions', { gameId, settings })
    return response.data
  },

  async joinGameSession(sessionId) {
    const response = await api.post('/games/sessions/join', { sessionId })
    return response.data
  },

  async getAvailableSessions(gameId) {
    const params = gameId ? { gameId } : {}
    const response = await api.get('/games/sessions/available', { params })
    return response.data
  },

  async updateSessionState(sessionId, gameState, status, currentTurn) {
    const response = await api.put('/games/sessions/state', {
      sessionId,
      gameState,
      status,
      currentTurn
    })
    return response.data
  }
}
