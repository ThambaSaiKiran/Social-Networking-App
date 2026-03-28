import { useState } from 'react'
import { Link, useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { autoComplete } from '../services/api'
import { Users, Home, Search, UserPlus, LogOut, Bell, Menu, X } from 'lucide-react'

export default function Navbar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [query, setQuery] = useState('')
  const [suggestions, setSuggestions] = useState([])
  const [menuOpen, setMenuOpen] = useState(false)

  const handleSearchInput = async (e) => {
    const val = e.target.value
    setQuery(val)
    if (val.length >= 1) {
      try {
        const { data } = await autoComplete(val)
        setSuggestions(data.suggestions || [])
      } catch { setSuggestions([]) }
    } else {
      setSuggestions([])
    }
  }

  const handleSearch = (e) => {
    e.preventDefault()
    if (query.trim()) {
      setSuggestions([])
      navigate(`/search?q=${encodeURIComponent(query.trim())}`)
    }
  }

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  const isActive = (path) => location.pathname === path

  return (
    <nav className="fixed top-0 left-0 right-0 z-50 bg-white border-b border-gray-200 shadow-sm">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">

          {/* Logo */}
          <Link to="/home" className="flex items-center gap-2 font-bold text-xl text-blue-600">
            <div className="w-8 h-8 bg-blue-600 rounded-lg flex items-center justify-center">
              <span className="text-white text-sm font-bold">SN</span>
            </div>
            <span className="hidden sm:block">SocialNet</span>
          </Link>

          {/* Search Bar */}
          <form onSubmit={handleSearch} className="relative flex-1 max-w-md mx-4">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 w-4 h-4" />
              <input
                value={query}
                onChange={handleSearchInput}
                placeholder="Search people, skills..."
                className="w-full pl-9 pr-4 py-2 bg-gray-100 rounded-full text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:bg-white transition"
              />
            </div>
            {suggestions.length > 0 && (
              <div className="absolute top-full mt-1 left-0 right-0 bg-white rounded-xl shadow-lg border border-gray-200 z-50 overflow-hidden">
                {suggestions.map((s, i) => (
                  <button
                    key={i}
                    type="button"
                    onClick={() => { setQuery(s); setSuggestions([]); navigate(`/search?q=${s}`) }}
                    className="w-full text-left px-4 py-2 text-sm hover:bg-blue-50 flex items-center gap-2"
                  >
                    <Search className="w-3 h-3 text-gray-400" />
                    {s}
                  </button>
                ))}
              </div>
            )}
          </form>

          {/* Desktop Nav */}
          <div className="hidden sm:flex items-center gap-1">
            <NavLink to="/home" active={isActive('/home')} icon={<Home className="w-5 h-5" />} label="Home" />
            <NavLink to="/users" active={isActive('/users')} icon={<Users className="w-5 h-5" />} label="People" />
            <NavLink to="/connections" active={isActive('/connections')} icon={<UserPlus className="w-5 h-5" />} label="Network" />

            {/* Avatar */}
            <Link to={`/users/${user?.userId}`} className="ml-2 flex items-center gap-2 px-3 py-1.5 rounded-full hover:bg-gray-100 transition">
              <img
                src={user?.profilePictureUrl || `https://api.dicebear.com/7.x/avataaars/svg?seed=${user?.username}`}
                alt="avatar"
                className="w-7 h-7 rounded-full object-cover border border-gray-200"
              />
              <span className="text-sm font-medium text-gray-700 hidden md:block">{user?.username}</span>
            </Link>

            <button
              onClick={handleLogout}
              className="ml-1 p-2 rounded-full hover:bg-red-50 text-gray-500 hover:text-red-500 transition"
              title="Logout"
            >
              <LogOut className="w-5 h-5" />
            </button>
          </div>

          {/* Mobile menu button */}
          <button className="sm:hidden p-2" onClick={() => setMenuOpen(!menuOpen)}>
            {menuOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
          </button>
        </div>
      </div>

      {/* Mobile Menu */}
      {menuOpen && (
        <div className="sm:hidden bg-white border-t border-gray-200 px-4 py-3 space-y-2">
          <MobileNavLink to="/home" label="Home" onClick={() => setMenuOpen(false)} />
          <MobileNavLink to="/users" label="People" onClick={() => setMenuOpen(false)} />
          <MobileNavLink to="/connections" label="My Network" onClick={() => setMenuOpen(false)} />
          <MobileNavLink to={`/users/${user?.userId}`} label="My Profile" onClick={() => setMenuOpen(false)} />
          <button onClick={handleLogout} className="w-full text-left py-2 text-red-500 font-medium">Logout</button>
        </div>
      )}
    </nav>
  )
}

function NavLink({ to, active, icon, label }) {
  return (
    <Link
      to={to}
      className={`flex flex-col items-center gap-0.5 px-3 py-1.5 rounded-lg transition text-xs font-medium ${
        active ? 'text-blue-600' : 'text-gray-500 hover:text-blue-600 hover:bg-blue-50'
      }`}
    >
      {icon}
      {label}
    </Link>
  )
}

function MobileNavLink({ to, label, onClick }) {
  return (
    <Link to={to} onClick={onClick} className="block py-2 text-gray-700 font-medium">
      {label}
    </Link>
  )
}
