import { render, screen } from '@testing-library/react'
import { LeagueCard } from './LeagueCard'

test('League view shows season end state', () => {
  render(
    <LeagueCard
      league={{
        name: 'Bronze 2026-04-27',
        tier: 'Bronze',
        endsAtUtc: '2026-05-04T00:00:00Z',
        participants: [],
      }}
    />,
  )

  expect(screen.getByText(/sezon bitis/i)).toBeInTheDocument()
  expect(screen.getByText(/henuz katilimci yok/i)).toBeInTheDocument()
})
