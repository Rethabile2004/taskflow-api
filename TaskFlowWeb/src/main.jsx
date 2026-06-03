//  THE FIXED MAIN.JSX
import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import AppRoutes from './routes/AppRoutes'
import { AuthProvider } from './auth/AuthContext' // 1. Added this import
import './index.css'

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider> {/* 2. Wrap AppRoutes here */}
        <AppRoutes />
      </AuthProvider> {/* 3. Close it here */}
    </BrowserRouter>
  </React.StrictMode>
)
