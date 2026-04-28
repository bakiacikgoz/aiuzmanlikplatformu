import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { LessonEditorForm } from './LessonEditor'

test('Lesson editor validates required fields', async () => {
  const user = userEvent.setup()
  const onSave = vi.fn(async () => undefined)

  render(<LessonEditorForm resources={[]} onSave={onSave} />)

  await user.click(screen.getByRole('button', { name: /draft kaydet/i }))

  expect(await screen.findByRole('alert')).toHaveTextContent('Unit, slug, baslik ve hedef zorunludur.')
  expect(onSave).not.toHaveBeenCalled()
})

test('Publish button is disabled when quality score is low', () => {
  render(<LessonEditorForm resources={[]} onSave={async () => undefined} onStatusChange={async () => undefined} />)

  expect(screen.getByRole('button', { name: /publish/i })).toBeDisabled()
})
