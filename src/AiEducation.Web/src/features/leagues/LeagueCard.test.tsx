import { render, screen } from '@testing-library/react'
import { LeagueCard } from './LeagueCard'

test('League card renders ranking', () => {
  render(
    <LeagueCard
      league={{
        name: 'Bronze 2026-04-27',
        tier: 'Bronze',
        participants: [
          { rank: 1, displayName: 'Ada', weeklyXp: 90, isCurrentUser: false },
          { rank: 2, displayName: 'Learner', weeklyXp: 45, isCurrentUser: true },
        ],
      }}
    />,
  )

  expect(screen.getByText('Bronze 2026-04-27')).toBeInTheDocument()
  expect(screen.getByText('#2')).toBeInTheDocument()
  expect(screen.getByText('45 XP')).toBeInTheDocument()
})
