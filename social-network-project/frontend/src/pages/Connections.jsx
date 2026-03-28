import { useState, useEffect } from 'react'
import { getMyConnections, getReceivedReqs, acceptRequest, rejectRequest } from '../services/api'
import { Link } from 'react-router-dom'
import toast from 'react-hot-toast'
import { Users, UserCheck, UserX, Clock } from 'lucide-react'

export default function Connections() {
  const [myConnections, setMyConnections] = useState([])
  const [pendingReqs, setPendingReqs] = useState([])
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState('connections')

  useEffect(() => {
    loadData()
  }, [])

  const loadData = async () => {
    setLoading(true)
    try {
      const [connsRes, reqsRes] = await Promise.all([
        getMyConnections(),
        getReceivedReqs()
      ])
      setMyConnections(connsRes.data)
      setPendingReqs(reqsRes.data)
    } catch { toast.error('Failed to load connections') }
    finally { setLoading(false) }
  }

  const handleAccept = async (connId) => {
    try {
      await acceptRequest(connId)
      setPendingReqs(prev => prev.filter(r => r.id !== connId))
      toast.success('Connection accepted!')
      loadData()
    } catch { toast.error('Failed to accept') }
  }

  const handleReject = async (connId) => {
    try {
      await rejectRequest(connId)
      setPendingReqs(prev => prev.filter(r => r.id !== connId))
      toast.success('Request declined.')
    } catch { toast.error('Failed to reject') }
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-6">
      <div className="flex items-center gap-3 mb-6">
        <div className="w-10 h-10 bg-blue-100 rounded-xl flex items-center justify-center">
          <Users className="w-5 h-5 text-blue-600" />
        </div>
        <h1 className="text-xl font-bold text-gray-900">My Network</h1>
      </div>

      {/* Tab Switcher */}
      <div className="flex rounded-xl bg-white border border-gray-100 shadow-sm p-1 mb-6">
        <TabBtn active={activeTab === 'connections'} onClick={() => setActiveTab('connections')}
          label={`Connections (${myConnections.length})`} icon={<UserCheck className="w-4 h-4" />} />
        <TabBtn active={activeTab === 'pending'} onClick={() => setActiveTab('pending')}
          label={`Pending (${pendingReqs.length})`} icon={<Clock className="w-4 h-4" />}
          badge={pendingReqs.length} />
      </div>

      {loading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {[1,2,3,4].map(i => <div key={i} className="h-24 bg-white rounded-xl border animate-pulse" />)}
        </div>
      ) : activeTab === 'connections' ? (
        myConnections.length === 0 ? (
          <EmptyState icon={<Users className="w-10 h-10 text-gray-300" />} text="No connections yet. Browse people and connect!" />
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {myConnections.map(conn => (
              <Link key={conn.userId} to={`/users/${conn.userId}`}
                className="bg-white rounded-xl border border-gray-100 shadow-sm p-4 flex items-center gap-3 hover:shadow-md transition">
                <img
                  src={conn.profilePic || `https://api.dicebear.com/7.x/avataaars/svg?seed=${conn.username}`}
                  className="w-12 h-12 rounded-full border border-gray-200"
                  alt=""
                />
                <div>
                  <p className="font-semibold text-gray-900 text-sm">{conn.fullName}</p>
                  <p className="text-xs text-gray-500">@{conn.username}</p>
                </div>
              </Link>
            ))}
          </div>
        )
      ) : (
        pendingReqs.length === 0 ? (
          <EmptyState icon={<Clock className="w-10 h-10 text-gray-300" />} text="No pending requests." />
        ) : (
          <div className="space-y-3">
            {pendingReqs.map(req => (
              <div key={req.id} className="bg-white rounded-xl border border-gray-100 shadow-sm p-4 flex items-center justify-between">
                <Link to={`/users/${req.senderId}`} className="flex items-center gap-3 hover:text-blue-600 transition">
                  <img
                    src={req.senderProfilePic || `https://api.dicebear.com/7.x/avataaars/svg?seed=${req.senderUsername}`}
                    className="w-12 h-12 rounded-full border border-gray-200"
                    alt=""
                  />
                  <div>
                    <p className="font-semibold text-sm">{req.senderFullName}</p>
                    <p className="text-xs text-gray-500">@{req.senderUsername} wants to connect</p>
                  </div>
                </Link>
                <div className="flex gap-2">
                  <button onClick={() => handleAccept(req.id)}
                    className="flex items-center gap-1.5 bg-blue-600 text-white text-xs px-3 py-1.5 rounded-full hover:bg-blue-700 transition">
                    <UserCheck className="w-3.5 h-3.5" /> Accept
                  </button>
                  <button onClick={() => handleReject(req.id)}
                    className="flex items-center gap-1.5 bg-gray-100 text-gray-600 text-xs px-3 py-1.5 rounded-full hover:bg-red-50 hover:text-red-500 transition">
                    <UserX className="w-3.5 h-3.5" /> Decline
                  </button>
                </div>
              </div>
            ))}
          </div>
        )
      )}
    </div>
  )
}

function TabBtn({ active, onClick, label, icon, badge }) {
  return (
    <button
      onClick={onClick}
      className={`flex-1 flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-medium transition relative ${
        active ? 'bg-blue-600 text-white' : 'text-gray-500 hover:text-blue-600'
      }`}
    >
      {icon} {label}
      {badge > 0 && (
        <span className="absolute -top-1 -right-1 w-5 h-5 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">
          {badge}
        </span>
      )}
    </button>
  )
}

function EmptyState({ icon, text }) {
  return (
    <div className="text-center py-16">
      <div className="flex justify-center mb-4">{icon}</div>
      <p className="text-gray-500 text-sm">{text}</p>
    </div>
  )
}
