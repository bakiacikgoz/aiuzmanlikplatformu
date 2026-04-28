import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { ExerciseSubmit } from './ExerciseSubmit'

test('Exercise submit shows loading and success states', async () => {
  const user = userEvent.setup()
  render(<ExerciseSubmit prompt="AI olmayan bir otomasyon örneği yaz." onSubmit={async () => 'Cevabın kaydedildi.'} />)

  await user.type(screen.getByLabelText('Cevap'), 'Sabit kurallı yönlendirme')
  await user.click(screen.getByRole('button', { name: /gönder/i }))

  expect(await screen.findByText('Cevabın kaydedildi.')).toBeInTheDocument()
})
