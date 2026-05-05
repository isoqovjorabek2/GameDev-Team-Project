import { useState, useEffect } from 'react'
import { Trophy, Medal, Award, Crown } from 'lucide-react'
import toast from 'react-hot-toast'
import playerService from '../services/playerService'

export default function LeaderboardPage() {
  const [leaderboard, setLeaderboard] = useState([])
  const [isLoading, setIsLoading] = useState(true)
  const [timeFilter, setTimeFilter] = useState('all')

  useEffect(() => {
    loadLeaderboard()
  }, [timeFilter])

  const loadLeaderboard = async () => {
    try {
      const response = await playerService.getLeaderboard()
      setLeaderboard(response.players || [])
    } catch (error) {
      toast.error('Failed to load leaderboard')
    } finally {
      setIsLoading(false)
    }
  }

  const getRankIcon = (rank) => {
    switch (rank) {
      case 1:
        return <Crown size={24} className="text-yellow-500" />
      case 2:
        return <Medal size={24} className="text-gray-400" />
      case 3:
        return <Award size={24} className="text-amber-600" />
      default:
        return <span className="text-lg font-bold text-gray-600">#{rank}</span>
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
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-white">Leaderboard</h1>
          <p className="text-purple-200 mt-2">Top players across all games</p>
        </div>
        <div className="flex space-x-2">
          {['all', 'weekly', 'monthly'].map((filter) => (
            <button
              key={filter}
              onClick={() => setTimeFilter(filter)}
              className={`px-4 py-2 rounded-lg capitalize ${
                timeFilter === filter
                  ? 'bg-purple-600 text-white'
                  : 'bg-white text-gray-700'
              }`}
            >
              {filter}
            </button>
          ))}
        </div>
      </div>

      <div className="card">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b">
                <th className="text-left py-3 px-4 text-gray-600 font-semibold">
                  Rank
                </th>
                <th className="text-left py-3 px-4 text-gray-600 font-semibold">
                  Player
                </th>
                <th className="text-left py-3 px-4 text-gray-600 font-semibold">
                  Level
                </th>
                <th className="text-left py-3 px-4 text-gray-600 font-semibold">
                  Games Played
                </th>
                <th className="text-left py-3 px-4 text-gray-600 font-semibold">
                  Win Rate
                </th>
                <th className="text-left py-3 px-4 text-gray-600 font-semibold">
                  High Score
                </th>
                <th className="text-left py-3 px-4 text-gray-600 font-semibold">
                  Total Score
                </th>
              </tr>
            </thead>
            <tbody>
              {leaderboard.length === 0 ? (
                <tr>
                  <td colSpan="7" className="text-center py-8 text-gray-500">
                    No players on the leaderboard yet
                  </td>
                </tr>
              ) : (
                leaderboard.map((player, index) => (
                  <tr
                    key={player._id}
                    className="border-b hover:bg-gray-50 transition"
                  >
                    <td className="py-3 px-4">
                      {getRankIcon(index + 1)}
                    </td>
                    <td className="py-3 px-4">
                      <div className="flex items-center space-x-3">
                        <div className="w-10 h-10 bg-purple-100 rounded-full flex items-center justify-center">
                          <span className="text-purple-600 font-bold">
                            {player.username?.[0]?.toUpperCase() || '?'}
                          </span>
                        </div>
                        <div>
                          <p className="font-medium text-gray-800">
                            {player.username}
                          </p>
                          <p className="text-sm text-gray-500">
                            {player.profile?.title || 'Player'}
                          </p>
                        </div>
                      </div>
                    </td>
                    <td className="py-3 px-4">
                      <span className="px-2 py-1 bg-purple-100 text-purple-800 rounded-full text-sm">
                        {player.profile?.level || 1}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-gray-700">
                      {player.stats?.gamesPlayed || 0}
                    </td>
                    <td className="py-3 px-4">
                      <span
                        className={`font-medium ${
                          (player.stats?.gamesWon || 0) /
                            (player.stats?.gamesPlayed || 1) >
                          0.5
                            ? 'text-green-600'
                            : 'text-gray-700'
                        }`}
                      >
                        {player.stats?.gamesPlayed
                          ? `${Math.round(
                              (player.stats.gamesWon / player.stats.gamesPlayed) *
                                100
                            )}%`
                          : '0%'}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-gray-700">
                      {player.stats?.highScore || 0}
                    </td>
                    <td className="py-3 px-4 font-medium text-gray-800">
                      {player.stats?.totalScore || 0}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Stats Summary */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="card">
          <div className="flex items-center space-x-3">
            <Trophy size={32} className="text-yellow-500" />
            <div>
              <p className="text-sm text-gray-600">Total Players</p>
              <p className="text-2xl font-bold text-gray-800">
                {leaderboard.length}
              </p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <Award size={32} className="text-purple-500" />
            <div>
              <p className="text-sm text-gray-600">Total Games Played</p>
              <p className="text-2xl font-bold text-gray-800">
                {leaderboard.reduce(
                  (sum, p) => sum + (p.stats?.gamesPlayed || 0),
                  0
                )}
              </p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <Medal size={32} className="text-green-500" />
            <div>
              <p className="text-sm text-gray-600">Total Score</p>
              <p className="text-2xl font-bold text-gray-800">
                {leaderboard.reduce(
                  (sum, p) => sum + (p.stats?.totalScore || 0),
                  0
                )}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}