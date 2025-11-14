import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import api from '../services/api'
import { UserInfo } from '../types'

interface AuthContextType {
  user: UserInfo | null
  token: string | null
  login: (username: string, password: string, useLocalAuth: boolean) => Promise<void>
  logout: () => void
  loading: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}

interface AuthProviderProps {
  children: ReactNode
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<UserInfo | null>(null)
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'))
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const initAuth = async () => {
      const storedToken = localStorage.getItem('token')
      if (storedToken) {
        try {
          const response = await api.get('/auth/user-info')
          setUser(response.data)
          setToken(storedToken)
        } catch (error) {
          console.error('Failed to validate token:', error)
          localStorage.removeItem('token')
          setToken(null)
        }
      }
      setLoading(false)
    }

    initAuth()
  }, [])

  const login = async (username: string, password: string, useLocalAuth: boolean) => {
    try {
      const response = await api.post('/auth/login', {
        username,
        password,
        useLocalAuth
      })
      
      const { token: newToken, userInfo } = response.data
      localStorage.setItem('token', newToken)
      setToken(newToken)
      setUser(userInfo)
      window.location.href = '/dashboard'
    } catch (error: any) {
      throw new Error(error.response?.data?.message || 'Đăng nhập thất bại')
    }
  }

  const logout = () => {
    localStorage.removeItem('token')
    setToken(null)
    setUser(null)
    window.location.href = '/login'
  }

  return (
    <AuthContext.Provider value={{ user, token, login, logout, loading }}>
      {children}
    </AuthContext.Provider>
  )
}

