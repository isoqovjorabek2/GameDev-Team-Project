import { Outlet, Link, useNavigate } from 'react-router-dom'
import { LogOut, User, Trophy, Home, Gamepad2 } from 'lucide-react'
import useAuthStore from '../store/authStore'
import socketService from '../services/socketService'

export default function Layout() {
  const { user, logout } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = () => {
    socketService.disconnect()
    logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen">
      <nav className="bg-white shadow-lg">
        <div className="container mx-auto px-4">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center space-x-8">
              <Link to="/" className="flex items-center space-x-2 text-purple-600 font-bold text-xl">
                <Gamepad2 size={24} />
                <span>GameHub</span>
              </Link>
              <div className="flex space-x-4">
                <Link
                  to="/"
                  className="flex items-center space-x-1 text-gray-700 hover:text-purple-600 transition"
                >
                  <Home size={18} />
                  <span>Dashboard</span>
                </Link>
                <Link
                  to="/leaderboard"
                  className="flex items-center space-x-1 text-gray-700 hover:text-purple-600 transition"
                >
                  <Trophy size={18} />
                  <span>Leaderboard</span>
                </Link>
              </div>
            </div>

            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2">
                <span className="online-indicator"></span>
                <span className="text-gray-700">{user?.username}</span>
                <span className="text-sm text-gray-500">Level {user?.profile?.level || 1}</span>
              </div>
              <Link
                to="/profile"
                className="p-2 rounded-full hover:bg-gray-100 transition"
              >
                <User size={20} className="text-gray-700" />
              </Link>
              <button
                onClick={handleLogout}
                className="flex items-center space-x-1 px-3 py-2 rounded-lg bg-red-500 text-white hover:bg-red-600 transition"
              >
                <LogOut size={18} />
                <span>Logout</span>
              </button>
            </div>
          </div>
        </div>
      </nav>

      <main className="container mx-auto px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}
