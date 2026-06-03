import { Link, useNavigate } from 'react-router-dom'
import { useCallback } from 'react'
import { useAuth } from '../auth/AuthContext' 

export default function Navbar() {
  const { token, user, logout } = useAuth()
  const navigate = useNavigate()

  // Memoize handler to prevent re-renders
  const handleLogout = useCallback(() => {
    logout()
    navigate('/login')
  }, [logout, navigate])

  return (
    <header className="sticky top-0 z-10 border-b border-slate-200 bg-white/80 backdrop-blur">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
        
        {/* Brand Logo Section */}
        <Link to="/" className="flex items-center gap-3 transition-opacity hover:opacity-90">
          <div className="h-9 w-9 rounded-lg bg-brand-600 text-white flex items-center justify-center font-bold shadow-soft">
            TF
          </div>
          <div className="leading-tight">
            <div className="text-sm font-semibold text-slate-900">TaskFlow</div>
            <div className="text-xs text-slate-500">Plan. Do. Done.</div>
          </div>
        </Link>

        {/* Navigation Action Links */}
        <nav className="flex items-center gap-4 text-sm">
          {token ? (
            <>
              {/* Optional user badge moved inside conditional block for layout consistency */}
              {user?.fullName && (
                <div className="hidden border-r border-slate-200 pr-4 sm:block">
                  <span className="text-xs text-slate-400">Signed in as </span>
                  <span className="font-medium text-slate-700">{user.fullName}</span>
                </div>
              )}
              <Link className="text-slate-700 hover:text-slate-900 transition-colors" to="/tasks">
                Tasks
              </Link>
              <button
                className="rounded-md bg-slate-100 px-3 py-2 text-slate-800 hover:bg-slate-200 transition-colors"
                onClick={handleLogout}
              >
                Logout
              </button>
            </>
          ) : (
            <>
              <Link className="text-slate-700 hover:text-slate-900 transition-colors" to="/login">
                Login
              </Link>
              <Link className="rounded-md bg-brand-600 px-3 py-2 text-white hover:bg-brand-700 transition-colors" to="/register">
                Register
              </Link>
            </>
          )}
        </nav>

      </div>
    </header>
  )
}
