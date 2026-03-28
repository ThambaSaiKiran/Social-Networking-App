import { Link } from 'react-router-dom'
import { MapPin, Briefcase, Users } from 'lucide-react'
import ConnectButton from './ConnectButton'

export default function UserCard({ user, onConnect, compact = false }) {
  return (
    <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-4 hover:shadow-md transition-shadow">
      <div className="flex items-start gap-3">
        <Link to={`/users/${user.id}`}>
          <img
            src={user.profilePictureUrl || `https://api.dicebear.com/7.x/avataaars/svg?seed=${user.username}`}
            alt={user.fullName}
            className="w-12 h-12 rounded-full object-cover border-2 border-white shadow"
          />
        </Link>
        <div className="flex-1 min-w-0">
          <Link to={`/users/${user.id}`} className="font-semibold text-gray-900 hover:text-blue-600 transition truncate block">
            {user.fullName}
          </Link>
          <p className="text-xs text-gray-500 truncate">@{user.username}</p>
          {user.location && (
            <p className="text-xs text-gray-400 flex items-center gap-1 mt-0.5">
              <MapPin className="w-3 h-3" />
              {user.location}
            </p>
          )}
        </div>
      </div>

      {!compact && user.bio && (
        <p className="mt-2 text-xs text-gray-600 line-clamp-2">{user.bio}</p>
      )}

      {!compact && user.skills?.length > 0 && (
        <div className="flex flex-wrap gap-1 mt-2">
          {user.skills.slice(0, 3).map(skill => (
            <span key={skill} className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">
              {skill}
            </span>
          ))}
          {user.skills.length > 3 && (
            <span className="text-xs text-gray-400">+{user.skills.length - 3}</span>
          )}
        </div>
      )}

      <div className="flex items-center justify-between mt-3">
        <span className="text-xs text-gray-400 flex items-center gap-1">
          <Users className="w-3 h-3" />
          {user.connectionCount ?? 0} connections
        </span>
        <ConnectButton user={user} onConnect={onConnect} />
      </div>
    </div>
  )
}
