import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AdminLessonForm } from './AdminLessonForm'

test('Admin lesson creation form submits lesson payload', async () => {
  const user = userEvent.setup()
  const submitted: unknown[] = []
  render(<AdminLessonForm onSubmit={async (payload) => { submitted.push(payload) }} />)

  await user.type(screen.getByLabelText('Modül slug'), 'ai-okuryazarligi')
  await user.type(screen.getByLabelText('Ders slug'), 'new-ai-byte')
  await user.type(screen.getByLabelText('Başlık'), 'Yeni AI Byte')
  await user.type(screen.getByLabelText('Hedef'), 'Tek hedef')
  await user.type(screen.getByLabelText('Alıştırma'), 'Kısa cevap yaz')
  await user.click(screen.getByRole('button', { name: /ders ekle/i }))

  expect(submitted).toHaveLength(1)
  expect(submitted[0]).toMatchObject({ unitSlug: 'ai-okuryazarligi', slug: 'new-ai-byte', title: 'Yeni AI Byte' })
})
