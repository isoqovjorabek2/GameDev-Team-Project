import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { Send, Users, Clock, Play, LogOut, MessageSquare } from 'lucide-react'
import toast from 'react-hot-toast'
import socketService from '../services/socketService'
import useAuthStore from '../store/authStore'

export default function GameRoomPage() {
  const { roomId } = useParams()
  const navigate = useNavigate()
  const { user } = useAuthStore()

  const [roomInfo, setRoomInfo] = useState(null)
  const [gameState, setGameState] = useState(null)
  const [players, setPlayers] = useState([])
  const [messages, setMessages] = useState([])
  const [newMessage, setNewMessage] = useState('')
  const [isReady, setIsReady] = useState(false)
  const [isGameStarted, setIsGameStarted] = useState(false)
  const [timeLeft, setTimeLeft] = useState(0)

  useEffect(() => {
    if (!socketService.isConnected()) {
      socketService.connect()
    }

    setupSocketListeners()
    socketService.joinRoom(roomId)

    return () => {
      cleanup()
    }
  }, [roomId])

  const setupSocketListeners = () => {
    socketService.on('room_state', (data) => {
      setRoomInfo(data)
      setPlayers(data.players || [])
      setGameState(data.gameState)
    })

    socketService.on('player_joined_room', (data) => {
      setPlayers((prev) => [...prev, data])
      toast.success(`${data.username} joined the room`)
    })

    socketService.on('player_left_room', (data) => {
      setPlayers((prev) => prev.filter((p) => p.playerId !== data.playerId))
      toast.info(`${data.username} left the room`)
    })

    socketService.on('player_ready_changed', (data) => {
      setPlayers((prev) =>
        prev.map((p) =>
          p.playerId === data.playerId ? { ...p, isReady: data.isReady } : p
        )
      )
    })

    socketService.on('all_players_ready', (data) => {
      if (data.canStart) {
        toast.success('All players ready! Game can start.')
      }
    })

    socketService.on('game_started', (data) => {
      setIsGameStarted(true)
      setGameState(data.gameState)
      setTimeLeft(data.gameState?.timeLeft || 0)
      toast.success('Game started!')
    })

    socketService.on('game_state_updated', (data) => {
      setGameState(data.gameState)
    })

    socketService.on('move_broadcast', (data) => {
      console.log('Move broadcast:', data)
    })

    socketService.on('game_ended', (data) => {
      setIsGameStarted(false)
      toast.success(`Game ended! Winner: ${data.winner}`)
    })

    socketService.on('timer_update', (data) => {
      setTimeLeft(data.timeLeft)
    })

    socketService.on('chat_message', (data) => {
      setMessages((prev) => [...prev, data])
    })

    socketService.on('typing_start', (data) => {
      console.log('User typing:', data)
    })

    socketService.on('typing_stop', (data) => {
      console.log('User stopped typing:', data)
    })

    socketService.on('error', (data) => {
      toast.error(data.message)
    })
  }

  const cleanup = () => {
    socketService.off('room_state')
    socketService.off('player_joined_room')
    socketService.off('player_left_room')
    socketService.off('player_ready_changed')
    socketService.off('all_players_ready')
    socketService.off('game_started')
    socketService.off('game_state_updated')
    socketService.off('move_broadcast')
    socketService.off('game_ended')
    socketService.off('timer_update')
    socketService.off('chat_message')
    socketService.off('typing_start')
    socketService.off('typing_stop')
    socketService.off('error')
  }

  const handleReady = () => {
    const newReadyState = !isReady
    setIsReady(newReadyState)
    socketService.setReady(roomId, newReadyState)
  }

  const handleStartGame = () => {
    socketService.startGame(roomId)
  }

  const handleLeaveRoom = () => {
    socketService.leaveRoom(roomId)
    navigate('/')
  }

  const handleSendMessage = (e) => {
    e.preventDefault()
    if (newMessage.trim()) {
      socketService.sendMessage(roomId, newMessage)
      setNewMessage('')
    }
  }

  const canStartGame =
    players.length >= (roomInfo?.minPlayers || 2) &&
    players.every((p) => p.isReady)

  const currentPlayer = players.find((p) => p.playerId === user?._id)

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="card">
        <div className="flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-gray-800">
              Game Room: {roomId?.slice(0, 8)}...
            </h1>
            <div className="flex items-center space-x-4 mt-2 text-sm text-gray-600">
              <span className="flex items-center">
                <Users size={16} className="mr-1" />
                {players.length} players
              </span>
              {isGameStarted && (
                <span className="flex items-center">
                  <Clock size={16} className="mr-1" />
                  {Math.floor(timeLeft / 60)}:{(timeLeft % 60)
                    .toString()
                    .padStart(2, '0')}
                </span>
              )}
            </div>
          </div>
          <button
            onClick={handleLeaveRoom}
            className="button button-secondary flex items-center"
          >
            <LogOut size={18} className="mr-2" />
            Leave
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Game Area */}
        <div className="lg:col-span-2 space-y-6">
          {/* Players */}
          <div className="card">
            <h2 className="text-xl font-bold text-gray-800 mb-4">Players</h2>
            <div className="space-y-3">
              {players.map((player) => (
                <div
                  key={player.playerId}
                  className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                >
                  <div className="flex items-center space-x-3">
                    <span
                      className={
                        player.isReady ? 'online-indicator' : 'offline-indicator'
                      }
                    ></span>
                    <div>
                      <p className="font-medium text-gray-800">
                        {player.username}
                        {player.playerId === user?._id && (
                          <span className="ml-2 text-sm text-purple-600">
                            (You)
                          </span>
                        )}
                      </p>
                      <p className="text-sm text-gray-500">
                        {player.isReady ? 'Ready' : 'Not Ready'}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-gray-600">
                      Score: {player.score || 0}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Game Board */}
          <div className="card">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-bold text-gray-800">Game Board</h2>
              {isGameStarted && (
                <div className="text-2xl font-bold text-purple-600">
                  {Math.floor(timeLeft / 60)}:{(timeLeft % 60)
                    .toString()
                    .padStart(2, '0')}
                </div>
              )}
            </div>

            {!isGameStarted ? (
              <div className="text-center py-12">
                <Play size={48} className="mx-auto text-gray-400 mb-4" />
                <p className="text-gray-600 mb-4">
                  Waiting for game to start...
                </p>
                {canStartGame && (
                  <button
                    onClick={handleStartGame}
                    className="button button-primary"
                  >
                    Start Game
                  </button>
                )}
              </div>
            ) : (
              <div className="bg-gray-100 rounded-lg p-8 min-h-[300px]">
                <p className="text-center text-gray-600">
                  Game in progress... (Game state visualization would go here)
                </p>
                {gameState && (
                  <pre className="mt-4 text-xs bg-white p-4 rounded overflow-auto">
                    {JSON.stringify(gameState, null, 2)}
                  </pre>
                )}
              </div>
            )}
          </div>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Ready Button */}
          <div className="card">
            <button
              onClick={handleReady}
              disabled={isGameStarted}
              className={`button w-full ${
                isReady ? 'button-secondary' : 'button-primary'
              }`}
            >
              {isReady ? 'Not Ready' : 'Ready'}
            </button>
          </div>

          {/* Chat */}
          <div className="card">
            <div className="flex items-center space-x-2 mb-4">
              <MessageSquare size={20} className="text-gray-600" />
              <h2 className="text-xl font-bold text-gray-800">Chat</h2>
            </div>

            <div className="h-64 overflow-y-auto mb-4 space-y-2">
              {messages.length === 0 ? (
                <p className="text-center text-gray-500 text-sm">
                  No messages yet
                </p>
              ) : (
                messages.map((msg, index) => (
                  <div
                    key={index}
                    className={`p-2 rounded-lg ${
                      msg.playerId === user?._id
                        ? 'bg-purple-100 ml-8'
                        : 'bg-gray-100 mr-8'
                    }`}
                  >
                    <p className="text-xs font-medium text-gray-700 mb-1">
                      {msg.username}
                    </p>
                    <p className="text-sm text-gray-800">{msg.message}</p>
                  </div>
                ))
              )}
            </div>

            <form onSubmit={handleSendMessage} className="flex space-x-2">
              <input
                type="text"
                value={newMessage}
                onChange={(e) => setNewMessage(e.target.value)}
                placeholder="Type a message..."
                className="input flex-1"
                disabled={isGameStarted}
              />
              <button
                type="submit"
                disabled={!newMessage.trim() || isGameStarted}
                className="button button-primary px-4"
              >
                <Send size={18} />
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  )
}