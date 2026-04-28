import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QuizQuestionForm } from './QuizQuestionEditor'

test('Quiz editor requires one correct option', async () => {
  const user = userEvent.setup()
  const onSubmit = vi.fn(async () => undefined)
  render(<QuizQuestionForm onSubmit={onSubmit} />)

  await user.type(screen.getByLabelText('Soru metni'), 'Train test ayrimi neden yapilir?')
  await user.type(screen.getByLabelText('Secenek 1'), 'Genelleme performansini olcmek icin')
  await user.type(screen.getByLabelText('Secenek 2'), 'Modeli ezberletmek icin')
  await user.type(screen.getByLabelText('Secenek 3'), 'Veriyi silmek icin')
  await user.type(screen.getByLabelText('Secenek 4'), 'CSS yazmak icin')
  await user.click(screen.getByRole('button', { name: /soru ekle/i }))

  expect(await screen.findByRole('alert')).toHaveTextContent('Tam olarak bir dogru secenek secilmelidir.')
  expect(onSubmit).not.toHaveBeenCalled()
})
