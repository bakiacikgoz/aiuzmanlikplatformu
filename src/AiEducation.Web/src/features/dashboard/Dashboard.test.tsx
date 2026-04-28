import { render, screen } from '@testing-library/react'
import { DashboardView } from './DashboardView'

test('Dashboard renders today card and progress cards', () => {
  render(
    <DashboardView
      data={{
        user: { displayName: 'Learner', dailyXpGoal: 20, selectedLearningPathSlug: 'beginner' },
        todayAiByte: {
          slug: 'beginner-ai-okuryazarligi-01',
          title: 'Yapay zeka nedir, ne değildir?',
          durationMinutes: 4,
          xpReward: 10,
          learningObjective: 'AI kavramını açıklamak',
        },
        totalXp: 45,
        currentStreakDays: 3,
        portfolioEvidenceCount: 1,
      }}
    />,
  )

  expect(screen.getByText('Bugünkü AI Byte')).toBeInTheDocument()
  expect(screen.getByText('Yapay zeka nedir, ne değildir?')).toBeInTheDocument()
  expect(screen.getByText('45 XP')).toBeInTheDocument()
  expect(screen.getByText('3 gün')).toBeInTheDocument()
})
