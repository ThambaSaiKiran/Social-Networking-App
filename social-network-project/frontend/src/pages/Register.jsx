import { useState, useEffect } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { register as registerApi, getInterests, getSkills } from '../services/api'
import toast from 'react-hot-toast'
import { UserPlus, Eye, EyeOff } from 'lucide-react'

export default function Register() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({
    username: '', email: '', password: '', fullName: '', bio: '', location: ''
  })
  const [showPwd, setShowPwd] = useState(false)
  const [loading, setLoading] = useState(false)
  const [allInterests, setAllInterests] = useState([])
  const [allSkills, setAllSkills] = useState([])
  const [selectedInterests, setSelectedInterests] = useState([])
  const [selectedSkills, setSelectedSkills] = useState([])
  const [step, setStep] = useState(1)

  useEffect(() => {
    getInterests().then(r => setAllInterests(r.data)).catch(() => {})
    getSkills().then(r => setAllSkills(r.data)).catch(() => {})
  }, [])

  const handleChange = e => setForm(prev => ({ ...prev, [e.target.name]: e.target.value }))
  const toggleInterest = name => setSelectedInterests(prev =>
    prev.includes(name) ? prev.filter(i => i !== name) : [...prev, name])
  const toggleSkill = name => setSelectedSkills(prev =>
    prev.includes(name) ? prev.filter(s => s !== name) : [...prev, name])

  const handleSubmit = async e => {
    e.preventDefault()
    setLoading(true)
    try {
      const { data } = await registerApi({
        ...form,
        interests: selectedInterests,
        skills: selectedSkills
      })
      login({
        userId: data.userId,
        username: data.username,
        email: data.email,
        fullName: data.fullName,
        profilePictureUrl: data.profilePictureUrl
      }, data.token)
      toast.success('Account created! Welcome to SocialNet!')
      navigate('/home')
    } catch (err) {
      toast.error(err.response?.data?.message || 'Registration failed.')
    } finally {
      setLoading(false)
    }
  }

  const groupedInterests = allInterests.reduce((acc, i) => {
    if (!acc[i.category]) acc[i.category] = []
    acc[i.category].push(i)
    return acc
  }, {})

  const groupedSkills = allSkills.reduce((acc, s) => {
    if (!acc[s.category]) acc[s.category] = []
    acc[s.category].push(s)
    return acc
  }, {})

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-indigo-50 flex items-center justify-center p-4">
      <div className="w-full max-w-lg">
        <div className="text-center mb-6">
          <div className="inline-flex items-center justify-center w-14 h-14 bg-blue-600 rounded-2xl mb-3 shadow-lg">
            <span className="text-white text-xl font-bold">SN</span>
          </div>
          <h1 className="text-2xl font-bold text-gray-900">Join SocialNet</h1>
          <p className="text-gray-500 text-sm mt-1">Connect with developers and professionals</p>
        </div>

        {/* Step Indicator */}
        <div className="flex justify-center gap-2 mb-6">
          {[1, 2, 3].map(s => (
            <div key={s} className={`w-8 h-1.5 rounded-full transition ${step >= s ? 'bg-blue-600' : 'bg-gray-200'}`} />
          ))}
        </div>

        <div className="bg-white rounded-2xl shadow-xl border border-gray-100 p-7">
          <form onSubmit={handleSubmit}>
            {/* Step 1: Basic Info */}
            {step === 1 && (
              <div className="space-y-4">
                <h2 className="text-lg font-semibold text-gray-800 mb-4">Basic Information</h2>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="block text-xs font-medium text-gray-700 mb-1">Username *</label>
                    <input name="username" value={form.username} onChange={handleChange} required
                      placeholder="alex_dev" className="w-full px-3 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm" />
                  </div>
                  <div>
                    <label className="block text-xs font-medium text-gray-700 mb-1">Full Name *</label>
                    <input name="fullName" value={form.fullName} onChange={handleChange} required
                      placeholder="Alex Johnson" className="w-full px-3 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm" />
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Email *</label>
                  <input name="email" type="email" value={form.email} onChange={handleChange} required
                    placeholder="alex@example.com" className="w-full px-3 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Password *</label>
                  <div className="relative">
                    <input name="password" type={showPwd ? 'text' : 'password'} value={form.password} onChange={handleChange} required minLength={6}
                      placeholder="Min 6 characters" className="w-full px-3 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm" />
                    <button type="button" onClick={() => setShowPwd(!showPwd)} className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400">
                      {showPwd ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                    </button>
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Location</label>
                  <input name="location" value={form.location} onChange={handleChange}
                    placeholder="San Francisco, CA" className="w-full px-3 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1">Bio</label>
                  <textarea name="bio" value={form.bio} onChange={handleChange} rows={3}
                    placeholder="Tell us about yourself..." className="w-full px-3 py-2.5 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm resize-none" />
                </div>
                <button type="button" onClick={() => setStep(2)}
                  disabled={!form.username || !form.email || !form.password || !form.fullName}
                  className="w-full py-3 bg-blue-600 text-white rounded-xl font-semibold text-sm hover:bg-blue-700 transition disabled:opacity-50">
                  Next: Choose Interests →
                </button>
              </div>
            )}

            {/* Step 2: Interests */}
            {step === 2 && (
              <div>
                <h2 className="text-lg font-semibold text-gray-800 mb-1">Your Interests</h2>
                <p className="text-sm text-gray-500 mb-4">Select topics you care about (used for better suggestions)</p>
                <div className="space-y-3 max-h-64 overflow-y-auto pr-1">
                  {Object.entries(groupedInterests).map(([cat, items]) => (
                    <div key={cat}>
                      <p className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1.5">{cat}</p>
                      <div className="flex flex-wrap gap-2">
                        {items.map(i => (
                          <button key={i.id} type="button" onClick={() => toggleInterest(i.name)}
                            className={`text-xs px-3 py-1.5 rounded-full border transition ${
                              selectedInterests.includes(i.name)
                                ? 'bg-blue-600 text-white border-blue-600'
                                : 'bg-white text-gray-600 border-gray-200 hover:border-blue-400'
                            }`}>
                            {i.name}
                          </button>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
                <div className="flex gap-3 mt-5">
                  <button type="button" onClick={() => setStep(1)} className="flex-1 py-3 border border-gray-200 text-gray-600 rounded-xl font-medium text-sm hover:bg-gray-50 transition">
                    ← Back
                  </button>
                  <button type="button" onClick={() => setStep(3)} className="flex-1 py-3 bg-blue-600 text-white rounded-xl font-semibold text-sm hover:bg-blue-700 transition">
                    Next: Choose Skills →
                  </button>
                </div>
              </div>
            )}

            {/* Step 3: Skills + Submit */}
            {step === 3 && (
              <div>
                <h2 className="text-lg font-semibold text-gray-800 mb-1">Your Skills</h2>
                <p className="text-sm text-gray-500 mb-4">Select skills to help us match you with the right people</p>
                <div className="space-y-3 max-h-64 overflow-y-auto pr-1">
                  {Object.entries(groupedSkills).map(([cat, items]) => (
                    <div key={cat}>
                      <p className="text-xs font-medium text-gray-500 uppercase tracking-wider mb-1.5">{cat}</p>
                      <div className="flex flex-wrap gap-2">
                        {items.map(s => (
                          <button key={s.id} type="button" onClick={() => toggleSkill(s.name)}
                            className={`text-xs px-3 py-1.5 rounded-full border transition ${
                              selectedSkills.includes(s.name)
                                ? 'bg-green-600 text-white border-green-600'
                                : 'bg-white text-gray-600 border-gray-200 hover:border-green-400'
                            }`}>
                            {s.name}
                          </button>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
                <div className="flex gap-3 mt-5">
                  <button type="button" onClick={() => setStep(2)} className="flex-1 py-3 border border-gray-200 text-gray-600 rounded-xl font-medium text-sm hover:bg-gray-50 transition">
                    ← Back
                  </button>
                  <button type="submit" disabled={loading}
                    className="flex-1 flex items-center justify-center gap-2 py-3 bg-blue-600 text-white rounded-xl font-semibold text-sm hover:bg-blue-700 transition disabled:opacity-60">
                    {loading ? <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" /> : <><UserPlus className="w-4 h-4" />Create Account</>}
                  </button>
                </div>
              </div>
            )}
          </form>

          <p className="mt-5 text-center text-sm text-gray-500">
            Already have an account?{' '}
            <Link to="/login" className="text-blue-600 hover:text-blue-700 font-medium">Sign in</Link>
          </p>
        </div>
      </div>
    </div>
  )
}
