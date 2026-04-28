import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { LessonPlayer } from './LessonPlayer'

test('LessonPlayer navigates step by step', async () => {
  const user = userEvent.setup()
  render(
    <LessonPlayer
      lesson={{
        slug: 'ai-byte',
        title: 'AI Byte',
        durationMinutes: 4,
        xpReward: 10,
        learningObjective: 'Tek kavram öğren',
        steps: [
          { key: 'goal', title: 'Bugünkü küçük hedef', body: 'Tek kavram öğren' },
          { key: 'exercise', title: 'Şimdi sen dene', body: 'Kısa cevap yaz' },
          { key: 'reward', title: 'Ödül', body: '10 XP kazanırsın' },
        ],
      }}
      onComplete={async () => undefined}
    />,
  )

  expect(screen.getByText('Bugünkü küçük hedef')).toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: /sonraki/i }))
  expect(screen.getByText('Şimdi sen dene')).toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: /sonraki/i }))
  expect(screen.getByRole('button', { name: /dersi tamamla/i })).toBeInTheDocument()
})
