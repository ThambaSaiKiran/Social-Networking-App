import { useState, useEffect } from 'react'
import { getFeed, getTrending, createPost, getSuggestions } from '../services/api'
import PostCard from '../components/PostCard'
import UserCard from '../components/UserCard'
import { useAuth } from '../context/AuthContext'
import toast from 'react-hot-toast'
import { TrendingUp, Users, Send, Image } from 'lucide-react'

export default function Home() {
  const { user } = useAuth()
  const [feedPosts, setFeedPosts] = useState([])
  const [trendingPosts, setTrendingPosts] = useState([])
  const [suggestions, setSuggestions] = useState([])
  const [activeTab, setActiveTab] = useState('feed')
  const [loading, setLoading] = useState(true)
  const [postContent, setPostContent] = useState('')
  const [postTags, setPostTags] = useState('')
  const [posting, setPosting] = useState(false)

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    setLoading(true)
    try {
      const [feedRes, trendRes, suggRes] = await Promise.all([
        getFeed(),
        getTrending(10),
        getSuggestions(5)
      ])
      setFeedPosts(feedRes.data)
      setTrendingPosts(trendRes.data)
      setSuggestions(suggRes.data)
    } catch { toast.error('Failed to load feed') }
    finally { setLoading(false) }
  }

  const handlePost = async e => {
    e.preventDefault()
    if (!postContent.trim()) return
    setPosting(true)
    try {
      const tags = postTags.split(',').map(t => t.trim().replace('#', '')).filter(Boolean)
      const { data } = await createPost({ content: postContent, tags })
      setFeedPosts(prev => [data, ...prev])
      setPostContent('')
      setPostTags('')
      toast.success('Post published!')
    } catch { toast.error('Failed to create post') }
    finally { setPosting(false) }
  }

  const handleDeletePost = id => {
    setFeedPosts(prev => prev.filter(p => p.id !== id))
    setTrendingPosts(prev => prev.filter(p => p.id !== id))
  }

  const currentPosts = activeTab === 'feed' ? feedPosts : trendingPosts

  return (
    <div className="max-w-6xl mx-auto px-4 py-6 grid grid-cols-1 lg:grid-cols-3 gap-6">

      {/* Main Feed */}
      <div className="lg:col-span-2 space-y-4">
        {/* Create Post */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
          <div className="flex gap-3">
            <img
              src={user?.profilePictureUrl || `https://api.dicebear.com/7.x/avataaars/svg?seed=${user?.username}`}
              className="w-10 h-10 rounded-full border border-gray-200"
              alt=""
            />
            <form onSubmit={handlePost} className="flex-1 space-y-3">
              <textarea
                value={postContent}
                onChange={e => setPostContent(e.target.value)}
                placeholder="What's on your mind? Share knowledge, ideas, or updates..."
                rows={3}
                className="w-full text-sm px-3 py-2.5 bg-gray-50 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-400 focus:bg-white resize-none transition"
              />
              <div className="flex items-center gap-3">
                <input
                  value={postTags}
                  onChange={e => setPostTags(e.target.value)}
                  placeholder="#tags, #csharp, #react"
                  className="flex-1 text-xs px-3 py-2 bg-gray-50 rounded-full border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-400"
                />
                <button type="submit" disabled={posting || !postContent.trim()}
                  className="flex items-center gap-1.5 bg-blue-600 text-white text-sm px-4 py-2 rounded-full hover:bg-blue-700 transition disabled:opacity-60 font-medium">
                  {posting ? <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" /> : <Send className="w-3.5 h-3.5" />}
                  Post
                </button>
              </div>
            </form>
          </div>
        </div>

        {/* Tab Switcher */}
        <div className="flex rounded-xl bg-white border border-gray-100 shadow-sm p-1">
          <TabBtn active={activeTab === 'feed'} onClick={() => setActiveTab('feed')} icon={<Users className="w-4 h-4" />} label="My Feed" />
          <TabBtn active={activeTab === 'trending'} onClick={() => setActiveTab('trending')} icon={<TrendingUp className="w-4 h-4" />} label="Trending" />
        </div>

        {/* Posts */}
        {loading ? (
          <div className="space-y-4">
            {[1,2,3].map(i => <div key={i} className="h-40 bg-white rounded-xl border border-gray-100 animate-pulse" />)}
          </div>
        ) : currentPosts.length === 0 ? (
          <div className="bg-white rounded-xl border border-gray-100 p-12 text-center">
            <p className="text-gray-400 text-sm">No posts yet. Connect with people to see their posts!</p>
          </div>
        ) : (
          <div className="space-y-4">
            {currentPosts.map(post => (
              <PostCard key={post.id} post={post} onDelete={handleDeletePost} />
            ))}
          </div>
        )}
      </div>

      {/* Right Sidebar: Suggestions */}
      <div className="space-y-4">
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
          <h3 className="font-semibold text-gray-800 mb-4 flex items-center gap-2">
            <Users className="w-4 h-4 text-blue-600" />
            People You May Know
          </h3>
          {suggestions.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-4">No suggestions right now</p>
          ) : (
            <div className="space-y-4">
              {suggestions.map(user => (
                <div key={user.id}>
                  <UserCard user={user} compact />
                  {user.suggestionReasons?.length > 0 && (
                    <p className="text-xs text-gray-400 mt-1 ml-15 pl-14">
                      {user.suggestionReasons.join(' · ')}
                    </p>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

function TabBtn({ active, onClick, icon, label }) {
  return (
    <button
      onClick={onClick}
      className={`flex-1 flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-medium transition ${
        active ? 'bg-blue-600 text-white shadow-sm' : 'text-gray-500 hover:text-blue-600'
      }`}
    >
      {icon} {label}
    </button>
  )
}
