import { Crown } from 'lucide-react'
import type { League } from '../../shared/types'
import { Badge, Card, CardHeader } from '../../shared/ui'

export function LeagueCard({ league }: { league: League }) {
  return (
    <Card>
      <CardHeader title={league.name} description={`${league.tier} ligi · İlk 10 üst lige çıkar`} />
      <div className="flex flex-col gap-2">
        {league.participants.slice(0, 8).map((participant) => (
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
