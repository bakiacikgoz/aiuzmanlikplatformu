import { Crown } from 'lucide-react'
import type { League } from '../../shared/types'
import { Badge, Card, CardHeader } from '../../shared/ui'

export function LeagueCard({ league }: { league: League }) {
  const seasonEnd = league.endsAtUtc ? new Date(league.endsAtUtc).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short' }) : null
  const topParticipants = league.participants.slice(0, 8)
  const currentUser = league.participants.find((participant) => participant.isCurrentUser)
  const visibleParticipants = currentUser && !topParticipants.some((participant) => participant.isCurrentUser) ? [...topParticipants, currentUser] : topParticipants

  return (
    <Card>
      <CardHeader title={league.name} description={`${league.tier} ligi · Ilk 10 ust lige cikar${seasonEnd ? ` · Sezon bitis ${seasonEnd}` : ''}`} />
      {league.participants.length === 0 ? <p className="rounded-xl border border-border bg-white p-4 text-sm text-muted">Bu sezonda henuz katilimci yok.</p> : null}
      <div className="flex flex-col gap-2">
        {visibleParticipants.map((participant) => (
          <div
            key={`${participant.rank}-${participant.displayName}`}
            className={`flex items-center justify-between rounded-xl border px-3 py-2 ${
              participant.isCurrentUser ? 'border-primary bg-[#eef3ff]' : 'border-border bg-white'
            }`}
          >
            <div className="flex items-center gap-3">
              <span className="w-8 text-sm font-bold text-muted">#{participant.rank}</span>
              <span className="font-semibold text-ink">{participant.displayName}</span>
              {participant.rank === 1 ? <Crown aria-hidden="true" className="text-accent" /> : null}
            </div>
            <Badge tone={participant.isCurrentUser ? 'primary' : 'neutral'}>{participant.weeklyXp} XP</Badge>
          </div>
        ))}
      </div>
    </Card>
  )
}
