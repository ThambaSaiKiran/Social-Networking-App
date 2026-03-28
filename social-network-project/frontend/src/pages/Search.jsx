import { useState, useEffect } from 'react'
import { useSearchParams } from 'react-router-dom'
import { searchUsers } from '../services/api'
import UserCard from '../components/UserCard'
import { Search as SearchIcon } from 'lucide-react'

export default function Search() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [query, setQuery] = useState(searchParams.get('q') || '')
  const [results, setResults] = useState([])
  const [loading, setLoading] = useState(false)
  const [searched, setSearched] = useState(false)

  useEffect(() => {
    const q = searchParams.get('q')
    if (q) { setQuery(q); doSearch(q) }
  }, [searchParams])

  const doSearch = async (q) => {
    if (!q?.trim()) return
    setLoading(true)
    setSearched(true)
    try {
      const { data } = await searchUsers(q.trim())
      setResults(data.results)
    } catch { setResults([]) }
    finally { setLoading(false) }
  }

  const handleSubmit = e => {
    e.preventDefault()
    if (query.trim()) {
      setSearchParams({ q: query.trim() })
    }
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-6">
      <h1 className="text-xl font-bold text-gray-900 mb-5">Search People</h1>

      <form onSubmit={handleSubmit} className="relative mb-8">
        <SearchIcon className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 w-5 h-5" />
        <input
          value={query}
          onChange={e => setQuery(e.target.value)}
          placeholder="Search by name, username, skills, or interests..."
          className="w-full pl-11 pr-24 py-3.5 rounded-2xl border border-gray-200 shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm bg-white"
          autoFocus
        />
        <button type="submit"
          className="absolute right-2 top-1/2 -translate-y-1/2 bg-blue-600 text-white px-4 py-2 rounded-xl text-sm font-medium hover:bg-blue-700 transition">
          Search
        </button>
      </form>

      {loading && (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {[1,2,3,4].map(i => <div key={i} className="h-40 bg-white rounded-xl border animate-pulse" />)}
        </div>
      )}

      {!loading && searched && results.length === 0 && (
        <div className="text-center py-16">
          <SearchIcon className="w-12 h-12 text-gray-200 mx-auto mb-3" />
          <p className="text-gray-500 font-medium">No results found for "{searchParams.get('q')}"</p>
          <p className="text-sm text-gray-400 mt-1">Try a different name, skill, or username</p>
        </div>
      )}

      {!loading && results.length > 0 && (
        <div>
          <p className="text-sm text-gray-500 mb-4">
            Found <strong className="text-gray-800">{results.length}</strong> results for "{searchParams.get('q')}"
          </p>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {results.map(user => (
              <UserCard key={user.id} user={user} />
            ))}
          </div>
        </div>
      )}

      {!searched && (
        <div className="text-center py-16">
          <div className="w-20 h-20 bg-blue-50 rounded-full flex items-center justify-center mx-auto mb-4">
            <SearchIcon className="w-8 h-8 text-blue-400" />
          </div>
          <p className="text-gray-500">Search for people by their name, username, skills, or interests</p>
          <p className="text-sm text-gray-400 mt-1">Powered by Trie-based prefix matching for instant results</p>
        </div>
      )}
    </div>
  )
}
