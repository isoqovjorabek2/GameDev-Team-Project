import { io } from 'socket.io-client'
import useAuthStore from '../store/authStore'

class SocketService {
  constructor() {
    this.socket = null
    this.listeners = new Map()
  }

  connect() {
    const token = useAuthStore.getState().token
    if (!token) {
      console.error('No auth token available')
      return
    }

    this.socket = io('http://localhost:3000', {
      auth: { token },
      transports: ['websocket', 'polling']
    })

    this.socket.on('connect', () => {
      console.log('Connected to server')
    })

    this.socket.on('disconnect', () => {
      console.log('Disconnected from server')
    })

    this.socket.on('error', (error) => {
      console.error('Socket error:', error)
    })
  }

  disconnect() {
    if (this.socket) {
      this.socket.disconnect()
      this.socket = null
    }
  }

  on(event, callback) {
    if (this.socket) {
      this.socket.on(event, callback)
    }
  }

  off(event, callback) {
    if (this.socket) {
      this.socket.off(event, callback)
    }
  }

  emit(event, data) {
    if (this.socket) {
      this.socket.emit(event, data)
    }
  }

  joinRoom(roomId) {
    this.emit('join_room', { roomId })
  }

  leaveRoom(roomId) {
    this.emit('leave_room', { roomId })
  }

  createRoom(gameType, gameSettings, maxPlayers) {
    this.emit('create_room', { gameType, gameSettings, maxPlayers })
  }

  startGame(roomId) {
    this.emit('game_start', { roomId })
  }

  makeMove(roomId, move, moveData) {
    this.emit('game_move', { roomId, move, moveData })
  }

  setReady(roomId, isReady) {
    this.emit('player_ready', { roomId, isReady })
  }

  sendMessage(roomId, message) {
    this.emit('send_message', { roomId, message })
  }

  startTyping(roomId) {
    this.emit('typing_start', { roomId })
  }

  stopTyping(roomId) {
    this.emit('typing_stop', { roomId })
  }

  updateStatus(status) {
    this.emit('player_status_update', { status })
  }

  requestReconnect(roomId) {
    this.emit('request_reconnect', { roomId })
  }

  isConnected() {
    return this.socket?.connected || false
  }
}

export default new SocketService()
