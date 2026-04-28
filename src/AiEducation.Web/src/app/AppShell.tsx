import {
  BarChart3,
  Bell,
  BookOpen,
  Bot,
  FolderKanban,
  Home,
  Library,
  ListChecks,
  LogOut,
  Map,
  Medal,
  PanelLeft,
  Settings,
  Trophy,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQueryClient } from '@tanstack/react-query'
import { clearSession, getSession, post } from '../shared/api'
import { SecondaryButton } from '../shared/ui'

export function AppShell({ children }: { children: ReactNode }) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const roles = getSession()?.roles ?? []
  const isAdmin = roles.includes('Admin')
  const nav = [
    ['/', 'Ana Panel', Home, true],
    ['/today', 'Bugünkü AI Byte', BookOpen, true],
    ['/roadmap', 'Yol Haritası', Map, true],
    ['/practice', 'Pratik Alanı', ListChecks, true],
    ['/projects', 'Projeler', FolderKanban, true],
    ['/leagues', 'Ligler', Trophy, true],
    ['/quests', 'Görevler', Medal, true],
    ['/portfolio', 'Portföy', PanelLeft, true],
    ['/resources', 'Kaynaklar', Library, true],
    ['/ai-mentor', 'AI Mentor', Bot, true],
    ['/notifications', 'Bildirimler', Bell, true],
    ['/admin', 'Admin', Settings, isAdmin],
    ['/analytics', 'Analytics', BarChart3, isAdmin],
  ] as const

  async function logout() {
    try {
      await post('/auth/logout', {})
    } finally {
      clearSession()
      queryClient.clear()
      navigate('/login')
    }
  }

  return (
    <div className="min-h-screen bg-background text-ink lg:grid lg:grid-cols-[280px_1fr]">
      <aside className="border-b border-border bg-white p-4 lg:sticky lg:top-0 lg:h-screen lg:border-b-0 lg:border-r">
        <div className="mb-6 flex items-center gap-3">
          <div className="flex size-11 items-center justify-center rounded-2xl bg-primary text-lg font-bold text-white">AI</div>
          <div>
            <p className="text-sm font-bold text-ink">AI Byte</p>
            <p className="text-xs text-muted">Mikro öğrenme MVP</p>
          </div>
        </div>
        <nav className="flex gap-2 overflow-x-auto lg:flex-col lg:overflow-visible">
          {nav.filter(([, , , visible]) => visible).map(([href, label, Icon]) => (
            <Link key={href} to={href} className="inline-flex min-w-fit items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium text-muted transition hover:bg-[#eef3ff] hover:text-primary">
              <Icon aria-hidden="true" className="size-4" />
              {label}
            </Link>
          ))}
        </nav>
        <SecondaryButton onClick={logout} className="mt-6 w-full">
          <LogOut aria-hidden="true" />
          Çıkış
        </SecondaryButton>
      </aside>
      <main className="mx-auto flex w-full max-w-7xl flex-col gap-6 p-4 md:p-8">{children}</main>
    </div>
  )
}
