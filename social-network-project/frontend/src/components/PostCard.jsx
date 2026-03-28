import { useState } from 'react'
import { Link } from 'react-router-dom'
import { formatDistanceToNow } from 'date-fns'
import { Heart, MessageCircle, Eye, Trash2, Send } from 'lucide-react'
import { toggleLike, addComment, getComments, deletePost } from '../services/api'
import { useAuth } from '../context/AuthContext'
import toast from 'react-hot-toast'

export default function PostCard({ post, onDelete }) {
  const { user } = useAuth()
  const [liked, setLiked] = useState(post.isLikedByCurrentUser)
  const [likeCount, setLikeCount] = useState(post.likeCount)
  const [showComments, setShowComments] = useState(false)
  const [comments, setComments] = useState([])
  const [commentText, setCommentText] = useState('')
  const [loadingComments, setLoadingComments] = useState(false)

  const handleLike = async () => {
    try {
      const { data } = await toggleLike(post.id)
      setLiked(data.liked)
      setLikeCount(data.likeCount + (data.liked ? 1 : 0))
    } catch { toast.error('Failed to like post') }
  }

  const handleToggleComments = async () => {
    if (!showComments && comments.length === 0) {
      setLoadingComments(true)
      try {
        const { data } = await getComments(post.id)
        setComments(data)
      } catch { toast.error('Failed to load comments') }
      finally { setLoadingComments(false) }
    }
    setShowComments(!showComments)
  }

  const handleComment = async (e) => {
    e.preventDefault()
    if (!commentText.trim()) return
    try {
      const { data } = await addComment(post.id, commentText)
      setComments(prev => [...prev, data])
      setCommentText('')
    } catch { toast.error('Failed to post comment') }
  }

  const handleDelete = async () => {
    if (!window.confirm('Delete this post?')) return
    try {
      await deletePost(post.id)
      onDelete?.(post.id)
      toast.success('Post deleted')
    } catch { toast.error('Failed to delete post') }
  }

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
      {/* Author */}
      <div className="flex items-start justify-between mb-3">
        <Link to={`/users/${post.userId}`} className="flex items-center gap-3 group">
          <img
            src={post.authorProfilePic || `https://api.dicebear.com/7.x/avataaars/svg?seed=${post.authorUsername}`}
            alt={post.authorFullName}
            className="w-10 h-10 rounded-full object-cover border border-gray-200"
          />
          <div>
            <p className="font-semibold text-gray-900 group-hover:text-blue-600 transition text-sm">
              {post.authorFullName}
            </p>
            <p className="text-xs text-gray-500">
              @{post.authorUsername} · {formatDistanceToNow(new Date(post.createdAt), { addSuffix: true })}
            </p>
          </div>
        </Link>
        {user?.userId === post.userId && (
          <button onClick={handleDelete} className="p-1.5 text-gray-400 hover:text-red-500 transition">
            <Trash2 className="w-4 h-4" />
          </button>
        )}
      </div>

      {/* Content */}
      <p className="text-gray-800 text-sm leading-relaxed mb-3 whitespace-pre-wrap">{post.content}</p>

      {/* Tags */}
      {post.tags?.length > 0 && (
        <div className="flex flex-wrap gap-1.5 mb-3">
          {post.tags.map(tag => (
            <span key={tag} className="text-xs text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full">
              #{tag}
            </span>
          ))}
        </div>
      )}

      {/* Actions */}
      <div className="flex items-center gap-5 pt-2 border-t border-gray-50">
        <button
          onClick={handleLike}
          className={`flex items-center gap-1.5 text-sm transition ${
            liked ? 'text-red-500' : 'text-gray-500 hover:text-red-500'
          }`}
        >
          <Heart className={`w-4 h-4 ${liked ? 'fill-red-500' : ''}`} />
          {likeCount}
        </button>
        <button
          onClick={handleToggleComments}
          className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-blue-600 transition"
        >
          <MessageCircle className="w-4 h-4" />
          {post.commentCount}
        </button>
        <span className="flex items-center gap-1.5 text-sm text-gray-400">
          <Eye className="w-4 h-4" />
          {post.viewCount}
        </span>
      </div>

      {/* Comments */}
      {showComments && (
        <div className="mt-4 space-y-3">
          {loadingComments ? (
            <p className="text-sm text-gray-400 text-center py-2">Loading...</p>
          ) : (
            comments.map(c => (
              <div key={c.id} className="flex items-start gap-2">
                <img
                  src={c.authorProfilePic || `https://api.dicebear.com/7.x/avataaars/svg?seed=${c.authorUsername}`}
                  className="w-7 h-7 rounded-full border border-gray-200"
                  alt=""
                />
                <div className="bg-gray-50 rounded-xl px-3 py-2 flex-1">
                  <p className="text-xs font-semibold text-gray-700">{c.authorFullName}</p>
                  <p className="text-sm text-gray-800">{c.content}</p>
                </div>
              </div>
            ))
          )}
          <form onSubmit={handleComment} className="flex gap-2 pt-2">
            <input
              value={commentText}
              onChange={e => setCommentText(e.target.value)}
              placeholder="Write a comment..."
              className="flex-1 text-sm px-3 py-2 rounded-full border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
            <button type="submit" className="p-2 rounded-full bg-blue-600 text-white hover:bg-blue-700 transition">
              <Send className="w-4 h-4" />
            </button>
          </form>
        </div>
      )}
    </div>
  )
}
