import { createContext, useContext, useEffect, useMemo, useState, useCallback } from 'react'
import { authApi } from '../services/authApi'

const AuthContext = createContext(null)

function readStoredToken() {
  try { return localStorage.getItem('taskflow_token') } catch { return null }
}

function readStoredUser() {
  try {
    const raw = localStorage.getItem('taskflow_user')
    return raw ? JSON.parse(raw) : null
  } catch { return null }
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => readStoredToken())
  const [user, setUser] = useState(() => readStoredUser())

  useEffect(() => {
    authApi.setToken(token)
  }, [token])

  // Stable login function
  const login = useCallback(async (email, password) => {
    const res = await authApi.login({ email, password })
    setToken(res.token)
    const userData = { email: res.email, fullName: res.fullName, expiresAt: res.expiresAt }
    setUser(userData)
    try {
      localStorage.setItem('taskflow_token', res.token)
      localStorage.setItem('taskflow_user', JSON.stringify(userData))
    } catch {}
    return res
  }, [])

  // Stable register function
  const register = useCallback(async (fullName, email, password) => {
    const res = await authApi.register({ fullName, email, password })
    setToken(res.token)
    const userData = { email: res.email, fullName: res.fullName, expiresAt: res.expiresAt }
    setUser(userData)
    try {
      localStorage.setItem('taskflow_token', res.token)
      localStorage.setItem('taskflow_user', JSON.stringify(userData))
    } catch {}
    return res
  }, [])

  // Stable logout function
  const logout = useCallback(() => {
    setToken(null)
    setUser(null)
    try {
      localStorage.removeItem('taskflow_token')
      localStorage.removeItem('taskflow_user')
    } catch {}
  }, [])

  // Context value only changes when state data changes
  const value = useMemo(() => ({
    token,
    user,
    login,
    register,
    logout
  }), [token, user, login, register, logout])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
