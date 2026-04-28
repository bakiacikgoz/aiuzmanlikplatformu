import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { vi } from 'vitest'
import { LessonPlayer } from './LessonPlayer'

test('LessonPlayer keeps completion disabled until exercise submit succeeds', async () => {
  const user = userEvent.setup()
  const onComplete = vi.fn()
  render(
    <LessonPlayer
      lesson={{
        slug: 'ai-byte',
        title: 'AI Byte',
        durationMinutes: 4,
        xpReward: 10,
        learningObjective: 'Tek kavram öğren',
        exercise: {
          id: 'exercise-1',
          prompt: 'Kısa cevap yaz',
          type: 'ShortAnswer',
          requiresAiFeedback: true,
        },
        steps: [
          { key: 'goal', title: 'Bugünkü küçük hedef', body: 'Tek kavram öğren' },
          { key: 'exercise', title: 'Şimdi sen dene', body: 'Kısa cevap yaz' },
          { key: 'reward', title: 'Ödül', body: '10 XP kazanırsın' },
        ],
      }}
      onExerciseSubmit={async () => ({ feedback: 'Cevabın kaydedildi.', passedQualityGate: true, submissionId: 'submission-1' })}
      onComplete={async () => {
        onComplete()
      }}
    />,
  )

  expect(screen.getByText('Bugünkü küçük hedef')).toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: /sonraki/i }))
  expect(screen.getByText('Şimdi sen dene')).toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: /sonraki/i }))
  expect(screen.getByRole('button', { name: /alıştırmayı gönder/i })).toBeDisabled()

  await user.click(screen.getByRole('button', { name: /geri/i }))
  await user.type(screen.getByLabelText('Cevap'), 'Sabit kurallı yönlendirme AI değildir.')
  await user.click(screen.getByRole('button', { name: /gönder/i }))
  expect(await screen.findByText('Cevabın kaydedildi.')).toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: /sonraki/i }))
  await user.click(screen.getByRole('button', { name: /dersi tamamla/i }))
  expect(onComplete).toHaveBeenCalledTimes(1)
})
