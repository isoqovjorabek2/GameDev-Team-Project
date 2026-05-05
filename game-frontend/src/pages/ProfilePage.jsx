import { useState, useEffect } from 'react'
import { User, Mail, Trophy, Target, Edit2, Save } from 'lucide-react'
import toast from 'react-hot-toast'
import useAuthStore from '../store/authStore'
import playerService from '../services/playerService'

export default function ProfilePage() {
  const { user, updateUser } = useAuthStore()
  const [isEditing, setIsEditing] = useState(false)
  const [formData, setFormData] = useState({
    username: user?.username || '',
    email: user?.email || '',
    bio: user?.profile?.bio || '',
    title: user?.profile?.title || 'Player'
  })
  const [isLoading, setIsLoading] = useState(false)

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    })
  }

  const handleSave = async () => {
    setIsLoading(true)
    try {
      await playerService.updateProfile({
        username: formData.username,
        email: formData.email,
        bio: formData.bio,
        title: formData.title
      })
      updateUser({
        username: formData.username,
        email: formData.email,
        profile: {
          ...user.profile,
          bio: formData.bio,
          title: formData.title
        }
      })
      toast.success('Profile updated successfully!')
      setIsEditing(false)
    } catch (error) {
      toast.error('Failed to update profile')
    } finally {
      setIsLoading(false)
    }
  }

  const handleCancel = () => {
    setFormData({
      username: user?.username || '',
      email: user?.email || '',
      bio: user?.profile?.bio || '',
      title: user?.profile?.title || 'Player'
    })
    setIsEditing(false)
  }

  const getLevelProgress = () => {
    const currentLevel = user?.profile?.level || 1
    const currentXP = user?.profile?.xp || 0
    const xpForNextLevel = currentLevel * 1000
    return (currentXP / xpForNextLevel) * 100
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-white">My Profile</h1>
        <p className="text-purple-200 mt-2">Manage your account and view stats</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Profile Card */}
        <div className="lg:col-span-1">
          <div className="card">
            <div className="text-center mb-6">
              <div className="w-24 h-24 bg-purple-100 rounded-full mx-auto mb-4 flex items-center justify-center">
                <span className="text-4xl font-bold text-purple-600">
                  {user?.username?.[0]?.toUpperCase() || '?'}
                </span>
              </div>
              <h2 className="text-2xl font-bold text-gray-800">
                {user?.username}
              </h2>
              <p className="text-gray-600">{user?.profile?.title || 'Player'}</p>
            </div>

            <div className="space-y-4">
              <div className="flex items-center space-x-3 text-gray-700">
                <User size={20} className="text-gray-500" />
                <span>{user?.username}</span>
              </div>
              <div className="flex items-center space-x-3 text-gray-700">
                <Mail size={20} className="text-gray-500" />
                <span>{user?.email}</span>
              </div>
            </div>

            <div className="mt-6 pt-6 border-t">
              <div className="flex justify-between items-center mb-2">
                <span className="text-sm text-gray-600">Level {user?.profile?.level || 1}</span>
                <span className="text-sm text-gray-600">
                  {user?.profile?.xp || 0} / {(user?.profile?.level || 1) * 1000} XP
                </span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className="bg-purple-600 h-2 rounded-full transition-all"
                  style={{ width: `${getLevelProgress()}%` }}
                ></div>
              </div>
            </div>
          </div>
        </div>

        {/* Stats and Edit Form */}
        <div className="lg:col-span-2 space-y-6">
          {/* Stats */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="card">
              <div className="flex items-center space-x-2 mb-2">
                <Trophy size={20} className="text-yellow-500" />
                <span className="text-sm text-gray-600">Games Won</span>
              </div>
              <p className="text-2xl font-bold text-gray-800">
                {user?.stats?.gamesWon || 0}
              </p>
            </div>
            <div className="card">
              <div className="flex items-center space-x-2 mb-2">
                <Target size={20} className="text-purple-500" />
                <span className="text-sm text-gray-600">Games Played</span>
              </div>
              <p className="text-2xl font-bold text-gray-800">
                {user?.stats?.gamesPlayed || 0}
              </p>
            </div>
            <div className="card">
              <div className="flex items-center space-x-2 mb-2">
                <Award size={20} className="text-green-500" />
                <span className="text-sm text-gray-600">High Score</span>
              </div>
              <p className="text-2xl font-bold text-gray-800">
                {user?.stats?.highScore || 0}
              </p>
            </div>
            <div className="card">
              <div className="flex items-center space-x-2 mb-2">
                <Trophy size={20} className="text-blue-500" />
                <span className="text-sm text-gray-600">Total Score</span>
              </div>
              <p className="text-2xl font-bold text-gray-800">
                {user?.stats?.totalScore || 0}
              </p>
            </div>
          </div>

          {/* Edit Profile */}
          <div className="card">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-xl font-bold text-gray-800">
                Edit Profile
              </h2>
              {!isEditing && (
                <button
                  onClick={() => setIsEditing(true)}
                  className="button button-secondary flex items-center"
                >
                  <Edit2 size={18} className="mr-2" />
                  Edit
                </button>
              )}
            </div>

            {isEditing ? (
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Username
                  </label>
                  <input
                    type="text"
                    name="username"
                    value={formData.username}
                    onChange={handleChange}
                    className="input"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Email
                  </label>
                  <input
                    type="email"
                    name="email"
                    value={formData.email}
                    onChange={handleChange}
                    className="input"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Title
                  </label>
                  <input
                    type="text"
                    name="title"
                    value={formData.title}
                    onChange={handleChange}
                    className="input"
                    placeholder="e.g., Pro Gamer, Champion"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Bio
                  </label>
                  <textarea
                    name="bio"
                    value={formData.bio}
                    onChange={handleChange}
                    className="input"
                    rows={4}
                    placeholder="Tell us about yourself..."
                  />
                </div>

                <div className="flex space-x-4">
                  <button
                    onClick={handleCancel}
                    disabled={isLoading}
                    className="button button-secondary flex-1"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleSave}
                    disabled={isLoading}
                    className="button button-primary flex-1"
                  >
                    {isLoading ? (
                      'Saving...'
                    ) : (
                      <>
                        <Save size={18} className="inline mr-2" />
                        Save Changes
                      </>
                    )}
                  </button>
                </div>
              </div>
            ) : (
              <div className="space-y-4">
                <div>
                  <p className="text-sm text-gray-600 mb-1">Username</p>
                  <p className="text-gray-800">{user?.username}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 mb-1">Email</p>
                  <p className="text-gray-800">{user?.email}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 mb-1">Title</p>
                  <p className="text-gray-800">{user?.profile?.title || 'Player'}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 mb-1">Bio</p>
                  <p className="text-gray-800">
                    {user?.profile?.bio || 'No bio yet'}
                  </p>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}