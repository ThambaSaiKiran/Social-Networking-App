import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { getUserById } from '../services/api'
import { useAuth } from '../context/AuthContext'
import PostCard from '../components/PostCard'
import ConnectButton from '../components/ConnectButton'
import { MapPin, Calendar, Briefcase, Users, Star } from 'lucide-react'
import { formatDistanceToNow } from 'date-fns'
import toast from 'react-hot-toast'

export default function UserProfile() {
  const { id } = useParams()
  const { user: currentUser } = useAuth()
  const [profile, setProfile] = useState(null)
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState('posts')

  useEffect(() => {
    loadProfile()
  }, [id])

  const loadProfile = async () => {
    setLoading(true)
    try {
      const { data } = await getUserById(id)
      setProfile(data)
    } catch { toast.error('User not found') }
    finally { setLoading(false) }
  }

  if (loading) return (
    <div className="max-w-4xl mx-auto px-4 py-6 animate-pulse space-y-4">
      <div className="h-40 bg-white rounded-2xl border" />
      <div className="h-64 bg-white rounded-2xl border" />
    </div>
  )

  if (!profile) return (
    <div className="max-w-4xl mx-auto px-4 py-16 text-center">
      <p className="text-gray-500">User not found.</p>
    </div>
  )

  const isOwnProfile = currentUser?.userId === profile.id

  return (
    <div className="max-w-4xl mx-auto px-4 py-6 space-y-5">
      {/* Profile Header */}
      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
        {/* Cover */}
        <div className="h-28 bg-gradient-to-r from-blue-500 via-blue-600 to-indigo-600" />

        <div className="px-6 pb-5">
          <div className="flex items-end justify-between -mt-12 mb-4">
            <img
              src={profile.profilePictureUrl || `https://api.dicebear.com/7.x/avataaars/svg?seed=${profile.username}`}
              alt={profile.fullName}
              className="w-24 h-24 rounded-2xl border-4 border-white shadow-md object-cover"
            />
            <div className="flex gap-2 mb-1">
              {!isOwnProfile && (
                <ConnectButton user={profile} onConnect={loadProfile} />
              )}
              {isOwnProfile && (
                <span className="text-xs bg-blue-50 text-blue-600 px-3 py-1.5 rounded-full font-medium">Your Profile</span>
              )}
            </div>
          </div>

          <h1 className="text-2xl font-bold text-gray-900">{profile.fullName}</h1>
          <p className="text-gray-500 text-sm">@{profile.username}</p>

          {profile.bio && <p className="text-gray-700 text-sm mt-2 leading-relaxed">{profile.bio}</p>}

          <div className="flex flex-wrap gap-4 mt-3 text-sm text-gray-500">
            {profile.location && (
              <span className="flex items-center gap-1.5"><MapPin className="w-4 h-4" />{profile.location}</span>
            )}
            <span className="flex items-center gap-1.5">
              <Calendar className="w-4 h-4" />
              Joined {formatDistanceToNow(new Date(profile.createdAt), { addSuffix: true })}
            </span>
            <span className="flex items-center gap-1.5">
              <Users className="w-4 h-4" />
              {profile.connectionCount} connections
            </span>
            <span className="flex items-center gap-1.5">
              <Briefcase className="w-4 h-4" />
              {profile.postCount} posts
            </span>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        {/* Sidebar */}
        <div className="space-y-4">
          {/* Skills */}
          {profile.skills?.length > 0 && (
            <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-4">
              <h3 className="font-semibold text-gray-800 mb-3 flex items-center gap-2 text-sm">
                <Star className="w-4 h-4 text-blue-500" /> Skills
              </h3>
              <div className="flex flex-wrap gap-1.5">
                {profile.skills.map(skill => (
                  <span key={skill} className="text-xs bg-blue-50 text-blue-700 px-2.5 py-1 rounded-full font-medium">
                    {skill}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Interests */}
          {profile.interests?.length > 0 && (
            <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-4">
              <h3 className="font-semibold text-gray-800 mb-3 text-sm">Interests</h3>
              <div className="flex flex-wrap gap-1.5">
                {profile.interests.map(interest => (
                  <span key={interest} className="text-xs bg-gray-100 text-gray-600 px-2.5 py-1 rounded-full">
                    {interest}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Mutual Friends */}
          {profile.mutualFriends?.length > 0 && (
            <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-4">
              <h3 className="font-semibold text-gray-800 mb-3 text-sm">
                {profile.mutualFriends.length} Mutual Connections
              </h3>
              <div className="space-y-2">
                {profile.mutualFriends.map(friend => (
                  <Link key={friend.id} to={`/users/${friend.id}`} className="flex items-center gap-2 hover:text-blue-600 transition">
                    <img
                      src={friend.profilePictureUrl || `https://api.dicebear.com/7.x/avataaars/svg?seed=${friend.username}`}
                      className="w-7 h-7 rounded-full border border-gray-200"
                      alt=""
                    />
                    <span className="text-xs text-gray-700">{friend.fullName}</span>
                  </Link>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Posts */}
        <div className="lg:col-span-2 space-y-4">
          <div className="flex rounded-xl bg-white border border-gray-100 shadow-sm p-1">
            <TabBtn active={activeTab === 'posts'} onClick={() => setActiveTab('posts')} label={`Posts (${profile.recentPosts?.length ?? 0})`} />
          </div>

          {profile.recentPosts?.length === 0 ? (
            <div className="bg-white rounded-xl border border-gray-100 p-10 text-center">
              <p className="text-gray-400 text-sm">No posts yet.</p>
            </div>
          ) : (
            profile.recentPosts?.map(post => (
              <PostCard key={post.id} post={post} />
            ))
          )}
        </div>
      </div>
    </div>
  )
}

function TabBtn({ active, onClick, label }) {
  return (
    <button
      onClick={onClick}
      className={`flex-1 py-2 rounded-lg text-sm font-medium transition ${
        active ? 'bg-blue-600 text-white' : 'text-gray-500 hover:text-blue-600'
      }`}
    >
      {label}
    </button>
  )
}
