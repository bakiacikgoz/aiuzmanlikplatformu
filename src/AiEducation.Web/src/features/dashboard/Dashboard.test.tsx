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

  expect(screen.getByText('Sıfırdan Uygulamalı Yapay Zeka Mühendisliği')).toBeInTheDocument()
  expect(screen.getByText(/Bugün 3 dakikalık bir AI Byte ile serini koru/)).toBeInTheDocument()
  expect(screen.getByText('AI Temelleri')).toBeInTheDocument()
  expect(screen.getByText('Applied AI Developer')).toBeInTheDocument()
  expect(screen.getByText('AI Engineer')).toBeInTheDocument()
  expect(screen.getByText('12 aylık timeline')).toBeInTheDocument()
  expect(screen.getByText('Yapay zeka nedir, ne değildir?')).toBeInTheDocument()
  expect(screen.getByText('45 XP')).toBeInTheDocument()
  expect(screen.getByText('3 gün')).toBeInTheDocument()
})
