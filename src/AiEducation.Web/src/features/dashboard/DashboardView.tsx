import { ArrowRight, BadgeCheck, Bot, CalendarDays, Flame, Map, Sparkles, Trophy } from 'lucide-react'
import type { ReactNode } from 'react'
import type { DashboardData } from '../../shared/types'
import { Badge, Button, Card } from '../../shared/ui'

export function DashboardView({ data }: { data: DashboardData }) {
  const today = data.todayAiByte

  return (
    <div className="flex flex-col gap-6">
      <section className="grid gap-5 lg:grid-cols-[1.5fr_1fr]">
        <Card className="overflow-hidden border-transparent bg-[#eef3ff] shadow-none">
          <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div className="max-w-2xl">
              <Badge>Yapay Zeka Uzmanı Öğrenme Yol Haritası</Badge>
              <h1 className="mt-4 text-3xl font-bold tracking-normal text-ink">Sıfırdan Uygulamalı Yapay Zeka Mühendisliği</h1>
              <p className="mt-2 text-sm leading-6 text-muted">
                Bugün 3 dakikalık bir AI Byte ile serini koru. {data.user.displayName}, günlük hedefin {data.user.dailyXpGoal} XP.
              </p>
              <div className="mt-5 flex flex-wrap gap-3">
                {today ? (
                  <a href={`/lessons/${today.slug}`} className="inline-flex">
                    <Button>
                      AI Byte başlat
                      <ArrowRight aria-hidden="true" />
                    </Button>
                  </a>
                ) : null}
                <a href="/roadmap" className="inline-flex rounded-xl border border-primary px-4 py-2 text-sm font-bold text-primary">
                  Yol haritası
                </a>
              </div>
            </div>
            <div className="flex size-28 items-center justify-center rounded-[28px] bg-white text-primary shadow-soft">
              <Sparkles aria-hidden="true" className="size-12" />
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-start justify-between gap-4">
            <div>
              <p className="text-sm font-semibold text-muted">Bugünkü AI Byte</p>
              <h2 className="mt-2 text-xl font-bold text-ink">{today?.title ?? 'İlk dersi seç'}</h2>
              <p className="mt-2 text-sm leading-6 text-muted">{today?.learningObjective ?? 'Yol haritası seçerek başla.'}</p>
            </div>
            <Badge tone="accent">{today ? `${today.durationMinutes} dk` : 'Hazır'}</Badge>
          </div>
          {today ? (
            <a href={`/lessons/${today.slug}`} className="mt-5 inline-flex">
              <Button>
                Başla
                <ArrowRight aria-hidden="true" />
              </Button>
            </a>
          ) : null}
        </Card>
      </section>

      <section className="grid gap-5 md:grid-cols-3">
        <MetricCard icon={<Trophy aria-hidden="true" />} label="Toplam XP" value={`${data.totalXp} XP`} tone="primary" />
        <MetricCard icon={<Flame aria-hidden="true" />} label="Seri" value={`${data.currentStreakDays} gün`} tone="accent" />
        <MetricCard icon={<BadgeCheck aria-hidden="true" />} label="Portföy kanıtı" value={`${data.portfolioEvidenceCount}`} tone="secondary" />
      </section>

      <section className="grid gap-5 lg:grid-cols-3">
        {[
          ['AI Temelleri', 'Veri, model, prompt ve ürün düşüncesi.', 'primary'],
          ['Applied AI Developer', 'API, RAG, ajan akışı ve ölçüm.', 'secondary'],
          ['AI Engineer', 'Dağıtım, güvenlik, izleme ve optimizasyon.', 'accent'],
        ].map(([title, description, tone]) => (
          <Card key={title}>
            <Badge tone={tone as 'primary' | 'secondary' | 'accent'}>{title}</Badge>
            <p className="mt-3 text-sm leading-6 text-muted">{description}</p>
          </Card>
        ))}
      </section>

      <section className="grid gap-5 lg:grid-cols-[1.2fr_0.8fr]">
        <Card>
          <div className="flex items-center gap-3">
            <CalendarDays aria-hidden="true" className="size-5 text-primary" />
            <h2 className="text-lg font-bold text-ink">Haftalık dağılım</h2>
          </div>
          <div className="mt-4 grid gap-3 md:grid-cols-4">
            {['AI Byte', 'Pratik', 'Quiz', 'Proje'].map((item, index) => (
              <div key={item} className="rounded-xl border border-border bg-white p-3">
                <p className="text-sm font-bold text-ink">{item}</p>
                <div className="mt-3 h-2 rounded-full bg-slate-100">
                  <div className="h-2 rounded-full bg-[#19b8aa]" style={{ width: `${85 - index * 12}%` }} />
                </div>
              </div>
            ))}
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <Bot aria-hidden="true" className="size-5 text-[#b9620b]" />
            <h2 className="text-lg font-bold text-ink">AI Mentor önerisi</h2>
          </div>
          <p className="mt-3 text-sm leading-6 text-muted">Bugünkü cevabına bir karşı örnek ekle, sonra quizde yanlışlarını review listesine taşı.</p>
        </Card>
      </section>

      <section className="grid gap-5 lg:grid-cols-[0.9fr_1.1fr]">
        <Card>
          <h2 className="text-lg font-bold text-ink">Görevler ve lig</h2>
          <div className="mt-4 grid gap-3">
            {['Günlük AI Byte', 'Pratik cevabı', 'Hata tekrarı'].map((quest) => (
              <div key={quest} className="flex items-center justify-between rounded-xl border border-border p-3">
                <span className="text-sm font-semibold text-ink">{quest}</span>
                <Badge tone="neutral">Bugün</Badge>
              </div>
            ))}
          </div>
        </Card>

        <Card>
          <div className="flex items-center gap-3">
            <Map aria-hidden="true" className="size-5 text-primary" />
            <h2 className="text-lg font-bold text-ink">12 aylık timeline</h2>
          </div>
          <div className="mt-4 grid gap-2 md:grid-cols-4">
            {['Temel', 'Uygulama', 'Sistem', 'Uzmanlık'].map((phase) => (
              <div key={phase} className="rounded-xl bg-[#f9fbff] p-3 text-sm font-semibold text-ink">{phase}</div>
            ))}
          </div>
          <p className="mt-4 text-sm leading-6 text-muted">Uzmanlık alanları: RAG ürünleri, AI otomasyon, değerlendirme sistemleri ve üretim kalitesi. Portföy kanıtı sayın: {data.portfolioEvidenceCount}.</p>
        </Card>
      </section>
    </div>
  )
}

function MetricCard({ icon, label, value, tone }: { icon: ReactNode; label: string; value: string; tone: 'primary' | 'secondary' | 'accent' }) {
  const colors = {
    primary: 'bg-[#eef3ff] text-primary',
    secondary: 'bg-[#eafbfb] text-[#168c83]',
    accent: 'bg-[#fff3e2] text-[#b9620b]',
  }

  return (
    <Card className="flex items-center gap-4">
      <div className={`flex size-12 items-center justify-center rounded-2xl ${colors[tone]}`}>{icon}</div>
      <div>
        <p className="text-sm text-muted">{label}</p>
        <p className="text-2xl font-bold text-ink">{value}</p>
      </div>
    </Card>
  )
}
