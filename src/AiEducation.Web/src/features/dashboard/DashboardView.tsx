import { ArrowRight, BadgeCheck, Flame, Sparkles, Trophy } from 'lucide-react'
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
              <h1 className="mt-4 text-3xl font-bold tracking-normal text-ink">Bugün 3 dakikalık bir AI Byte bitir.</h1>
              <p className="mt-2 text-sm leading-6 text-muted">
                {data.user.displayName}, günlük hedefin {data.user.dailyXpGoal} XP. Küçük adımı tamamla, serini koru ve portföy yolunda ilerle.
              </p>
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
