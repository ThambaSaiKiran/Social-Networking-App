import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
})

// Attach JWT token to every request
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Handle 401 globally
api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      window.location.href = '/login'
    }
    return Promise.reject(err)
  }
)

// Auth
export const register = data => api.post('/auth/register', data)
export const login    = data => api.post('/auth/login', data)

// Users
export const getUsers       = (page = 1, pageSize = 20) => api.get(`/users?page=${page}&pageSize=${pageSize}`)
export const getUserById    = id  => api.get(`/users/${id}`)
export const getMe          = ()  => api.get('/users/me')
export const getSuggestions = (count = 10) => api.get(`/users/suggestions?count=${count}`)
export const getInterests   = ()  => api.get('/users/interests')
export const getSkills      = ()  => api.get('/users/skills')

// Posts
export const getFeed          = (page = 1) => api.get(`/posts/feed?page=${page}`)
export const getTrending      = (count = 20) => api.get(`/posts/trending?count=${count}`)
export const createPost       = data => api.post('/posts', data)
export const deletePost       = id   => api.delete(`/posts/${id}`)
export const toggleLike       = id   => api.post(`/posts/${id}/like`)
export const getComments      = id   => api.get(`/posts/${id}/comments`)
export const addComment       = (id, content) => api.post(`/posts/${id}/comments`, { content })

// Connections
export const sendRequest      = targetId    => api.post(`/connections/request/${targetId}`)
export const acceptRequest    = connId      => api.put(`/connections/accept/${connId}`)
export const rejectRequest    = connId      => api.put(`/connections/reject/${connId}`)
export const removeConnection = targetId    => api.delete(`/connections/${targetId}`)
export const getMyConnections = ()           => api.get('/connections/my')
export const getReceivedReqs  = ()           => api.get('/connections/requests/received')

// Search
export const searchUsers   = (q, page = 1) => api.get(`/search/users?q=${encodeURIComponent(q)}&page=${page}`)
export const autoComplete  = prefix        => api.get(`/search/autocomplete?prefix=${encodeURIComponent(prefix)}`)

export default api
