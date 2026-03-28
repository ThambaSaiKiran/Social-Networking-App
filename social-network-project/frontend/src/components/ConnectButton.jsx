import { useState } from 'react'
import { sendRequest, removeConnection } from '../services/api'
import toast from 'react-hot-toast'
import { UserPlus, UserCheck, Clock } from 'lucide-react'

export default function ConnectButton({ user, onConnect }) {
  const [status, setStatus] = useState(user.connectionStatus || 'none')
  const [loading, setLoading] = useState(false)

  if (status === 'self') return null

  const handleConnect = async () => {
    setLoading(true)
    try {
      if (status === 'none') {
        await sendRequest(user.id)
        setStatus('pending_sent')
        toast.success(`Connection request sent to ${user.fullName}!`)
        onConnect?.()
      } else if (status === 'connected') {
        await removeConnection(user.id)
        setStatus('none')
        toast.success('Disconnected successfully.')
        onConnect?.()
      }
    } catch (err) {
      toast.error(err.response?.data?.message || 'Action failed')
    } finally {
      setLoading(false)
    }
  }

  const configs = {
    none: {
      label: 'Connect',
      icon: <UserPlus className="w-3.5 h-3.5" />,
      className: 'bg-blue-600 text-white hover:bg-blue-700'
    },
    pending_sent: {
      label: 'Pending',
      icon: <Clock className="w-3.5 h-3.5" />,
      className: 'bg-gray-100 text-gray-500 cursor-default'
    },
    pending_received: {
      label: 'Respond',
      icon: <UserCheck className="w-3.5 h-3.5" />,
      className: 'bg-green-600 text-white hover:bg-green-700'
    },
    connected: {
      label: 'Connected',
      icon: <UserCheck className="w-3.5 h-3.5" />,
      className: 'bg-gray-100 text-gray-600 hover:bg-red-50 hover:text-red-500'
    }
  }

  const cfg = configs[status] || configs.none

  return (
    <button
      onClick={handleConnect}
      disabled={loading || status === 'pending_sent'}
      className={`flex items-center gap-1.5 text-xs font-medium px-3 py-1.5 rounded-full transition ${cfg.className} disabled:opacity-60`}
    >
      {cfg.icon}
      {cfg.label}
    </button>
  )
}
