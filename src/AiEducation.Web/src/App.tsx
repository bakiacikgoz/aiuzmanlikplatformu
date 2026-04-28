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
import { useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import { BrowserRouter, Link, Navigate, Route, Routes, useNavigate, useParams } from 'react-router-dom'
import { QueryClient, QueryClientProvider, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { AdminLessonForm, type AdminLessonPayload } from './features/admin/AdminLessonForm'
import { DashboardView } from './features/dashboard/DashboardView'
import { LeagueCard } from './features/leagues/LeagueCard'
import { LessonPlayer } from './features/lessonPlayer/LessonPlayer'
import { ExerciseSubmit } from './features/practice/ExerciseSubmit'
import { api, clearSession, getToken, post, saveSession, type AuthSession } from './shared/api'
import type { DashboardData, League, Lesson } from './shared/types'
import { Badge, Button, Card, CardHeader, Field, SecondaryButton, TextArea, TextInput } from './shared/ui'

const queryClient = new QueryClient()

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<AuthPage />} />
          <Route path="/*" element={<ProtectedApp />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}

function ProtectedApp() {
  if (!getToken()) {
    return <Navigate to="/login" replace />
  }

  return (
    <AppShell>
      <Routes>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/onboarding" element={<OnboardingPage />} />
        <Route path="/today" element={<DashboardPage />} />
        <Route path="/roadmap" element={<RoadmapPage />} />
        <Route path="/paths/:slug" element={<PathDetailPage />} />
        <Route path="/lessons/:slug" element={<LessonPage />} />
        <Route path="/practice" element={<PracticePage />} />
        <Route path="/quiz" element={<QuizPage />} />
        <Route path="/projects" element={<ProjectsPage />} />
        <Route path="/leagues" element={<LeaguesPage />} />
        <Route path="/quests" element={<QuestsPage />} />
        <Route path="/portfolio" element={<PortfolioPage />} />
        <Route path="/resources" element={<ResourcesPage />} />
        <Route path="/ai-mentor" element={<AiMentorPage />} />
        <Route path="/notifications" element={<NotificationsPage />} />
        <Route path="/admin" element={<AdminPage />} />
        <Route path="/admin/content" element={<AdminContentPage />} />
        <Route path="/admin/experiments" element={<AdminExperimentsPage />} />
        <Route path="/analytics" element={<AnalyticsPage />} />
      </Routes>
    </AppShell>
  )
}

function AppShell({ children }: { children: ReactNode }) {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const nav = [
    ['/', 'Ana Panel', Home],
    ['/today', 'Bugünkü AI Byte', BookOpen],
    ['/roadmap', 'Yol Haritası', Map],
    ['/practice', 'Pratik Alanı', ListChecks],
    ['/projects', 'Projeler', FolderKanban],
    ['/leagues', 'Ligler', Trophy],
    ['/quests', 'Görevler', Medal],
    ['/portfolio', 'Portföy', PanelLeft],
    ['/resources', 'Kaynaklar', Library],
    ['/ai-mentor', 'AI Mentor', Bot],
    ['/notifications', 'Bildirimler', Bell],
    ['/admin', 'Admin', Settings],
    ['/analytics', 'Analytics', BarChart3],
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
          {nav.map(([href, label, Icon]) => (
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

function AuthPage() {
  const navigate = useNavigate()
  const [isRegister, setRegister] = useState(true)
  const [form, setForm] = useState({ email: 'learner@example.com', password: 'Passw0rd!', displayName: 'Learner', timeZoneId: 'Europe/Istanbul' })
  const [error, setError] = useState('')

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    try {
      const session = await post<AuthSession>(isRegister ? '/auth/register' : '/auth/login', form)
      saveSession(session)
      navigate('/onboarding')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Giriş başarısız.')
    }
  }

  return (
    <div className="grid min-h-screen place-items-center bg-background p-4">
      <Card className="w-full max-w-md">
        <CardHeader title={isRegister ? 'Kayıt ol' : 'Giriş yap'} description="Günlük AI Byte akışına devam et." />
        <form onSubmit={submit} className="flex flex-col gap-4">
          {isRegister ? (
            <Field label="Ad">
              <TextInput value={form.displayName} onChange={(event) => setForm({ ...form, displayName: event.target.value })} />
            </Field>
          ) : null}
          <Field label="E-posta">
            <TextInput type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} />
          </Field>
          <Field label="Şifre">
            <TextInput type="password" value={form.password} onChange={(event) => setForm({ ...form, password: event.target.value })} />
          </Field>
          {error ? <p className="rounded-xl border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</p> : null}
          <Button type="submit">{isRegister ? 'Kayıt ol' : 'Giriş yap'}</Button>
          <SecondaryButton type="button" onClick={() => setRegister((value) => !value)}>
            {isRegister ? 'Zaten hesabım var' : 'Yeni hesap oluştur'}
          </SecondaryButton>
        </form>
      </Card>
    </div>
  )
}

function DashboardPage() {
  const { data, isLoading } = useQuery({ queryKey: ['dashboard'], queryFn: () => api<DashboardData>('/dashboard/me') })
  if (isLoading) return <Loading title="Dashboard yükleniyor" />
  return data ? <DashboardView data={data} /> : <Empty title="Dashboard verisi yok" />
}

function OnboardingPage() {
  const navigate = useNavigate()
  const [dailyXpGoal, setDailyXpGoal] = useState(20)
  const [learningPathSlug, setLearningPathSlug] = useState('beginner')
  const { data: paths = [] } = useQuery({ queryKey: ['paths'], queryFn: () => api<Array<{ slug: string; title: string; level: string }>>('/learning-paths') })
  const mutation = useMutation({
    mutationFn: () => post('/onboarding', { dailyXpGoal, learningPathSlug }),
    onSuccess: () => navigate('/'),
  })

  return (
    <Card>
      <CardHeader title="Günlük hedefini seç" description="Başlangıç için 20 XP dengeli bir tempodur." />
      <div className="grid gap-5 lg:grid-cols-2">
        <section className="flex flex-wrap gap-3">
          {[10, 20, 30, 40, 50].map((goal) => (
            <button key={goal} onClick={() => setDailyXpGoal(goal)} className={`rounded-xl border px-4 py-3 text-sm font-bold ${dailyXpGoal === goal ? 'border-primary bg-[#eef3ff] text-primary' : 'border-border bg-white text-ink'}`}>
              {goal} XP
            </button>
          ))}
        </section>
        <section className="grid gap-3">
          {paths.map((path) => (
            <button key={path.slug} onClick={() => setLearningPathSlug(path.slug)} className={`rounded-xl border p-4 text-left ${learningPathSlug === path.slug ? 'border-primary bg-[#eef3ff]' : 'border-border bg-white'}`}>
              <p className="font-semibold">{path.title}</p>
              <p className="text-sm text-muted">{path.level}</p>
            </button>
          ))}
        </section>
      </div>
      <Button className="mt-5" onClick={() => mutation.mutate()} disabled={mutation.isPending}>
        Başla
      </Button>
    </Card>
  )
}

function RoadmapPage() {
  const { data: paths = [] } = useQuery({ queryKey: ['paths'], queryFn: () => api<Array<{ slug: string; title: string; level: string; lessonCount: number; unitCount: number }>>('/learning-paths') })
  return (
    <Page title="Yol Haritası">
      <div className="grid gap-5 md:grid-cols-3">
        {paths.map((path) => (
          <Card key={path.slug}>
            <Badge tone={path.slug === 'beginner' ? 'primary' : path.slug === 'intermediate' ? 'secondary' : 'accent'}>{path.level}</Badge>
            <h2 className="mt-3 text-xl font-bold">{path.title}</h2>
            <p className="mt-2 text-sm text-muted">
              {path.unitCount} modül · {path.lessonCount} mikro ders
            </p>
            <Link to={`/paths/${path.slug}`} className="mt-4 inline-flex">
              <Button>İncele</Button>
            </Link>
          </Card>
        ))}
      </div>
    </Page>
  )
}

function PathDetailPage() {
  const { slug } = useParams()
  const { data } = useQuery({ queryKey: ['path', slug], queryFn: () => api<{ title: string; units: Array<{ slug: string; title: string; lessons: Array<{ slug: string; title: string; durationMinutes: number }> }> }>(`/learning-paths/${slug}`) })
  return (
    <Page title={data?.title ?? 'Modül Detay'}>
      <div className="grid gap-4">
        {data?.units.map((unit) => (
          <Card key={unit.slug}>
            <h2 className="text-lg font-bold">{unit.title}</h2>
            <div className="mt-3 grid gap-2">
              {unit.lessons.map((lesson) => (
                <Link key={lesson.slug} to={`/lessons/${lesson.slug}`} className="rounded-xl border border-border bg-white px-3 py-2 text-sm hover:border-primary">
                  {lesson.title} · {lesson.durationMinutes} dk
                </Link>
              ))}
            </div>
          </Card>
        ))}
      </div>
    </Page>
  )
}

function LessonPage() {
  const { slug } = useParams()
  const queryClient = useQueryClient()
  const { data } = useQuery({ queryKey: ['lesson', slug], queryFn: () => api<Lesson>(`/lessons/${slug}`), enabled: Boolean(slug) })
  const mutation = useMutation({
    mutationFn: () => post(`/lessons/${slug}/complete`, { exerciseSubmitted: true, answer: 'Tamamlandı' }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['dashboard'] }),
  })

  if (!data) return <Loading title="Ders yükleniyor" />
  return <LessonPlayer lesson={data} onComplete={async () => { await mutation.mutateAsync() }} />
}

function PracticePage() {
  return (
    <Page title="Pratik Alanı">
      <ExerciseSubmit prompt="Bugün öğrendiğin AI kavramını tek cümlede açıkla." onSubmit={async (answer) => {
        await post('/events/track', { eventName: 'ExerciseSubmitted', properties: { answerLength: answer.length } })
        return 'Pratik cevabın kaydedildi.'
      }} />
    </Page>
  )
}

function QuizPage() {
  const [score, setScore] = useState(80)
  const mutation = useMutation({ mutationFn: () => post('/quizzes/beginner-ai-okuryazarligi-01/attempts', { scorePercent: score, wrongAnswers: score >= 70 ? [] : ['q1'] }) })
  return (
    <Page title="Quiz Sonuç">
      <Card>
        <Field label="Skor">
          <TextInput type="number" value={score} onChange={(event) => setScore(Number(event.target.value))} />
        </Field>
        <Button className="mt-4" onClick={() => mutation.mutate()}>Quiz gönder</Button>
        {mutation.isSuccess ? <p className="mt-3 text-sm text-[#168c83]">Quiz sonucu kaydedildi.</p> : null}
      </Card>
    </Page>
  )
}

function ProjectsPage() {
  const [selected, setSelected] = useState<string | null>(null)
  const [repositoryUrl, setRepositoryUrl] = useState('')
  const { data: projects = [] } = useQuery({ queryKey: ['projects'], queryFn: () => api<Array<{ slug: string; title: string; level: string; xpReward: number }>>('/projects') })
  const mutation = useMutation({ mutationFn: () => post(`/projects/${selected}/submissions`, { repositoryUrl, notes: 'MVP teslimi' }) })
  return (
    <Page title="Projeler">
      <div className="grid gap-5 lg:grid-cols-[1fr_360px]">
        <div className="grid gap-4">
          {projects.map((project) => (
            <Card key={project.slug}>
              <Badge tone={project.level === 'beginner' ? 'primary' : 'accent'}>{project.level}</Badge>
              <h2 className="mt-3 text-lg font-bold">{project.title}</h2>
              <p className="text-sm text-muted">{project.xpReward} XP</p>
              <Button className="mt-4" onClick={() => setSelected(project.slug)}>Teslim et</Button>
            </Card>
          ))}
        </div>
        <Card>
          <CardHeader title="Proje Teslim" description={selected ?? 'Bir proje seç'} />
          <Field label="GitHub / kanıt linki">
            <TextInput value={repositoryUrl} onChange={(event) => setRepositoryUrl(event.target.value)} />
          </Field>
          <Button className="mt-4" disabled={!selected || !repositoryUrl} onClick={() => mutation.mutate()}>Teslim gönder</Button>
        </Card>
      </div>
    </Page>
  )
}

function LeaguesPage() {
  const { data } = useQuery({ queryKey: ['league'], queryFn: () => api<League>('/leagues/current') })
  return <Page title="Ligler">{data ? <LeagueCard league={data} /> : <Loading title="Lig yükleniyor" />}</Page>
}

function QuestsPage() {
  const { data: quests = [] } = useQuery({ queryKey: ['quests'], queryFn: () => api<Array<{ slug: string; title: string; completed: boolean; rewardXp: number }>>('/quests/daily') })
  return (
    <Page title="Görevler">
      <div className="grid gap-3">
        {quests.map((quest) => (
          <Card key={quest.slug} className="flex items-center justify-between gap-4">
            <div>
              <h2 className="font-bold">{quest.title}</h2>
              <p className="text-sm text-muted">{quest.rewardXp} XP</p>
            </div>
            <Badge tone={quest.completed ? 'secondary' : 'neutral'}>{quest.completed ? 'Tamamlandı' : 'Bekliyor'}</Badge>
          </Card>
        ))}
      </div>
    </Page>
  )
}

function PortfolioPage() {
  const { data: evidence = [] } = useQuery({ queryKey: ['portfolio'], queryFn: () => api<Array<{ title: string; evidenceUrl: string }>>('/portfolio/me') })
  return (
    <Page title="Portföy">
      <div className="grid gap-3">
        {evidence.map((item) => (
          <Card key={item.evidenceUrl}>
            <h2 className="font-bold">{item.title}</h2>
            <a className="text-sm text-primary" href={item.evidenceUrl}>{item.evidenceUrl}</a>
          </Card>
        ))}
      </div>
    </Page>
  )
}

function ResourcesPage() {
  const { data: resources = [] } = useQuery({ queryKey: ['resources'], queryFn: () => api<Array<{ slug: string; title: string; url: string; type: string }>>('/resources') })
  return (
    <Page title="Kaynaklar">
      <div className="grid gap-3 md:grid-cols-2">
        {resources.map((resource) => (
          <Card key={resource.slug}>
            <Badge>{resource.type}</Badge>
            <h2 className="mt-3 font-bold">{resource.title}</h2>
            <a className="text-sm text-primary" href={resource.url}>{resource.url}</a>
          </Card>
        ))}
      </div>
    </Page>
  )
}

function AiMentorPage() {
  const [message, setMessage] = useState('Bu dersi daha basit anlat.')
  const mutation = useMutation({ mutationFn: () => post<{ content: string }>('/ai/chat', { message }) })
  return (
    <Page title="AI Mentor">
      <Card>
        <Field label="Mesaj">
          <TextArea value={message} onChange={(event) => setMessage(event.target.value)} />
        </Field>
        <Button className="mt-4" onClick={() => mutation.mutate()}>Gönder</Button>
        {mutation.data ? <p className="mt-4 rounded-xl bg-[#eef3ff] p-4 text-sm leading-6">{mutation.data.content}</p> : null}
      </Card>
    </Page>
  )
}

function NotificationsPage() {
  const { data: notifications = [] } = useQuery({ queryKey: ['notifications'], queryFn: () => api<Array<{ id: string; status: string; template?: { title: string; body: string } }>>('/notifications') })
  return (
    <Page title="Bildirimler">
      <div className="grid gap-3">{notifications.map((item) => <Card key={item.id}><h2 className="font-bold">{item.template?.title ?? item.status}</h2><p className="text-sm text-muted">{item.template?.body}</p></Card>)}</div>
    </Page>
  )
}

function AdminPage() {
  return (
    <Page title="Admin Dashboard">
      <div className="grid gap-4 md:grid-cols-2">
        <Link to="/admin/content"><Card><h2 className="font-bold">İçerik Yönetimi</h2><p className="text-sm text-muted">Ders, kaynak, rozet ve görev ekle.</p></Card></Link>
        <Link to="/admin/experiments"><Card><h2 className="font-bold">Deney Yönetimi</h2><p className="text-sm text-muted">A/B testleri oluştur ve durdur.</p></Card></Link>
      </div>
    </Page>
  )
}

function AdminContentPage() {
  const mutation = useMutation({ mutationFn: (payload: AdminLessonPayload) => post('/admin/lessons', payload) })
  return <Page title="Admin İçerik Yönetimi"><AdminLessonForm onSubmit={async (payload) => { await mutation.mutateAsync(payload) }} /></Page>
}

function AdminExperimentsPage() {
  const [key, setKey] = useState('new_test')
  const mutation = useMutation({
    mutationFn: () => post('/admin/experiments', { key, name: key, hypothesis: 'Yeni hipotez', primaryMetric: 'D7 retention', guardrailMetric: 'completion', isActive: true, variants: ['a', 'b'] }),
  })
  return (
    <Page title="Admin Deney Yönetimi">
      <Card>
        <Field label="Experiment key">
          <TextInput value={key} onChange={(event) => setKey(event.target.value)} />
        </Field>
        <Button className="mt-4" onClick={() => mutation.mutate()}>Deney oluştur</Button>
      </Card>
    </Page>
  )
}

function AnalyticsPage() {
  const { data = [] } = useQuery({ queryKey: ['funnel'], queryFn: () => api<Array<{ eventName: string; count: number }>>('/admin/analytics/funnel') })
  return (
    <Page title="Analytics">
      <div className="grid gap-3">
        {data.map((event) => <Card key={event.eventName} className="flex justify-between"><span>{event.eventName}</span><Badge>{event.count}</Badge></Card>)}
      </div>
    </Page>
  )
}

function Page({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div className="flex flex-col gap-5">
      <header>
        <h1 className="text-2xl font-bold text-ink">{title}</h1>
      </header>
      {children}
    </div>
  )
}

function Loading({ title }: { title: string }) {
  return <Card><p className="text-sm text-muted">{title}</p></Card>
}

function Empty({ title }: { title: string }) {
  return <Card><p className="text-sm text-muted">{title}</p></Card>
}
