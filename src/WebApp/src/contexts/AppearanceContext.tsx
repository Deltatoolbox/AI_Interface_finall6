import { createContext, useContext, useState, useEffect, ReactNode } from 'react'

interface AppearanceSettings {
  theme: 'light' | 'dark' | 'auto'
  fontSize: 'small' | 'medium' | 'large'
}

interface AppearanceContextType {
  settings: AppearanceSettings
  updateSettings: (newSettings: Partial<AppearanceSettings>) => void
  isDarkMode: boolean
}

const AppearanceContext = createContext<AppearanceContextType | undefined>(undefined)

const defaultSettings: AppearanceSettings = {
  theme: 'auto',
  fontSize: 'medium'
}

export function AppearanceProvider({ children }: { children: ReactNode }) {
  const [settings, setSettings] = useState<AppearanceSettings>(() => {
    const saved = localStorage.getItem('appearance-settings')
    return saved ? JSON.parse(saved) : defaultSettings
  })

  const [isDarkMode, setIsDarkMode] = useState(false)

  useEffect(() => {
    // Save settings to localStorage whenever they change
    localStorage.setItem('appearance-settings', JSON.stringify(settings))
  }, [settings])

  useEffect(() => {
    // Apply theme immediately
    const applyTheme = () => {
      const root = document.documentElement
      
      if (settings.theme === 'dark') {
        setIsDarkMode(true)
        root.classList.add('dark')
      } else if (settings.theme === 'light') {
        setIsDarkMode(false)
        root.classList.remove('dark')
      } else { // auto
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
        setIsDarkMode(prefersDark)
        if (prefersDark) {
          root.classList.add('dark')
        } else {
          root.classList.remove('dark')
        }
      }
    }

    // Apply theme immediately on mount and when settings change
    applyTheme()

    // Listen for system theme changes when in auto mode
    if (settings.theme === 'auto') {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
      const handleChange = () => applyTheme()
      mediaQuery.addEventListener('change', handleChange)
      return () => mediaQuery.removeEventListener('change', handleChange)
    }
  }, [settings.theme])

  useEffect(() => {
    // Apply font size immediately
    const root = document.documentElement
    const fontSizeMap = {
      small: '14px',
      medium: '16px',
      large: '18px'
    }
    root.style.fontSize = fontSizeMap[settings.fontSize]
    
    // Also apply to body for better inheritance
    document.body.style.fontSize = fontSizeMap[settings.fontSize]
  }, [settings.fontSize])

  const updateSettings = (newSettings: Partial<AppearanceSettings>) => {
    setSettings(prev => ({ ...prev, ...newSettings }))
  }

  return (
    <AppearanceContext.Provider value={{ settings, updateSettings, isDarkMode }}>
      {children}
    </AppearanceContext.Provider>
  )
}

export function useAppearance() {
  const context = useContext(AppearanceContext)
  if (context === undefined) {
    throw new Error('useAppearance must be used within an AppearanceProvider')
  }
  return context
}
