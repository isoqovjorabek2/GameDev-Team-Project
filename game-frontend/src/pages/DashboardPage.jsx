import { useState, useEffect } from 'react'
import { Plus, Users, Clock, Play } from 'lucide-react'
import toast from 'react-hot-toast'
import gameService from '../services/gameService'
import socketService from '../services/socketService'
import { useNavigate } from 'react-router-dom'

export default function DashboardPage() {
  const [games, setGames] = useState([])
  const [sessions, setSessions] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [selectedGame, setSelectedGame] = useState(null)
  const navigate = useNavigate()

  useEffect(() => {
    loadGames()
    loadSessions()
    setupSocketListeners()

    return () => {
      socketService.off('player_joined_lobby')
      socketService.off('player_left_lobby')
    }
  }, [])

  const loadGames = async () => {
    try {
      const response = await gameService.getAllGames()
      setGames(response.games || [])
    } catch (error) {
      toast.error('Failed to load games')
    }
  }

  const loadSessions = async () => {
    try {
      const response = await gameService.getAvailableSessions()
      setSessions(response.sessions || [])
    } catch (error) {
      toast.error('Failed to load sessions')
    } finally {
      setIsLoading(false)
    }
  }

  const setupSocketListeners = () => {
    socketService.on('player_joined_lobby', (data) => {
      console.log('Player joined lobby:', data)
    })

    socketService.on('player_left_lobby', (data) => {
      console.log('Player left lobby:', data)
    })
  }

  const handleCreateRoom = (game) => {
    setSelectedGame(game)
    setShowCreateModal(true)
  }

  const handleJoinSession = async (sessionId) => {
    try {
      await gameService.joinGameSession(sessionId)
      toast.success('Joined session successfully!')
      navigate(`/room/${sessionId}`)
    } catch (error) {
      toast.error(error.response?.data?.error || 'Failed to join session')
    }
  }

  const handleCreateGameRoom = async (settings) => {
    try {
      const response = await gameService.createGameSession(
        selectedGame._id,
        settings
      )
      toast.success('Game room created!')
      setShowCreateModal(false)
      navigate(`/room/${response.session._id}`)
    } catch (error) {
      toast.error('Failed to create game room')
    }
  }

  if (isLoading) {
    return (
      <div className="loading">
        <div className="spinner"></div>
      </div>
    )
  }

  return (
    <div className="space-y-8">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-white">Game Dashboard</h1>
          <p className="text-purple-200 mt-2">Choose a game and start playing</p>
        </div>
      </div>

      {/* Available Games */}
      <div>
        <h2 className="text-2xl font-bold text-white mb-4">Available Games</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {games.map((game) => (
            <div key={game._id} className="card">
              <div className="flex justify-between items-start mb-4">
                <div>
                  <h3 className="text-xl font-bold text-gray-800">
                    {game.name}
                  </h3>
                  <p className="text-gray-600 text-sm mt-1">{game.type}</p>
                </div>
                <span className="px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm">
                  Active
                </span>
              </div>

              <p className="text-gray-600 mb-4">{game.description}</p>

              <div className="flex items-center justify-between text-sm text-gray-500 mb-4">
                <div className="flex items-center space-x-4">
                  <span className="flex items-center">
                    <Users size={16} className="mr-1" />
                    {game.minPlayers}-{game.maxPlayers} players
                  </span>
                </div>
              </div>

              <button
                onClick={() => handleCreateRoom(game)}
                className="button button-primary w-full"
              >
                <Plus size={18} className="inline mr-2" />
                Create Room
              </button>
            </div>
          ))}
        </div>
      </div>

      {/* Active Sessions */}
      <div>
        <h2 className="text-2xl font-bold text-white mb-4">
          Active Game Sessions
        </h2>
        {sessions.length === 0 ? (
          <div className="card text-center py-8">
            <p className="text-gray-500">No active sessions available</p>
            <p className="text-gray-400 text-sm mt-2">
              Create a room to start playing!
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {sessions.map((session) => (
              <div key={session._id} className="card">
                <div className="flex justify-between items-start mb-4">
                  <div>
                    <h3 className="text-lg font-bold text-gray-800">
                      {session.gameId?.name || 'Unknown Game'}
                    </h3>
                    <p className="text-gray-600 text-sm">
                      {session.players.length} / {session.gameId?.maxPlayers}{' '}
                      players
                    </p>
                  </div>
                  <span className="px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full text-sm">
                    Waiting
                  </span>
                </div>

                <div className="flex items-center space-x-2 mb-4">
                  {session.players.slice(0, 3).map((player) => (
                    <div
                      key={player.playerId}
                      className="flex items-center space-x-2"
                    >
                      <span className="online-indicator"></span>
                      <span className="text-sm text-gray-700">
                        {player.username}
                      </span>
                    </div>
                  ))}
                  {session.players.length > 3 && (
                    <span className="text-sm text-gray-500">
                      +{session.players.length - 3} more
                    </span>
                  )}
                </div>

                <button
                  onClick={() => handleJoinSession(session._id)}
                  className="button button-secondary w-full"
                >
                  <Play size={18} className="inline mr-2" />
                  Join Session
                </button>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Create Room Modal */}
      {showCreateModal && selectedGame && (
        <CreateRoomModal
          game={selectedGame}
          onClose={() => setShowCreateModal(false)}
          onCreate={handleCreateGameRoom}
        />
      )}
    </div>
  )
}

function CreateRoomModal({ game, onClose, onCreate }) {
  const [maxPlayers, setMaxPlayers] = useState(game.maxPlayers)
  const [roundTime, setRoundTime] = useState(60)

  const handleSubmit = (e) => {
    e.preventDefault()
    onCreate({
      maxPlayers,
      roundTime
    })
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="card w-full max-w-md">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold text-gray-800">
            Create {game.name} Room
          </h2>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700"
          >
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Max Players
            </label>
            <select
              value={maxPlayers}
              onChange={(e) => setMaxPlayers(parseInt(e.target.value))}
              className="input"
            >
              {Array.from(
                { length: game.maxPlayers - game.minPlayers + 1 },
                (_, i) => game.minPlayers + i
              ).map((num) => (
                <option key={num} value={num}>
                  {num} players
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Round Time (seconds)
            </label>
            <input
              type="number"
              value={roundTime}
              onChange={(e) => setRoundTime(parseInt(e.target.value))}
              className="input"
              min="30"
              max="300"
              step="30"
            />
          </div>

          <div className="flex space-x-4">
            <button
              type="button"
              onClick={onClose}
              className="button button-secondary flex-1"
            >
              Cancel
            </button>
            <button type="submit" className="button button-primary flex-1">
              Create Room
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}