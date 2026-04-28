import { Send } from 'lucide-react'
import { type FormEvent, useState } from 'react'
import { Button, Field, TextArea } from '../../shared/ui'

export function ExerciseSubmit({ prompt, onSubmit }: { prompt: string; onSubmit: (answer: string) => Promise<string> }) {
  const [answer, setAnswer] = useState('')
  const [message, setMessage] = useState('')
  const [isSubmitting, setSubmitting] = useState(false)

  async function submit(event: FormEvent) {
    event.preventDefault()
    if (!answer.trim()) return
    setSubmitting(true)
    try {
      setMessage(await onSubmit(answer))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={submit} className="mt-5 flex flex-col gap-3">
      <Field label="Cevap">
        <TextArea value={answer} onChange={(event) => setAnswer(event.target.value)} placeholder={prompt} />
      </Field>
      <div className="flex items-center gap-3">
        <Button disabled={isSubmitting || !answer.trim()} type="submit">
          <Send aria-hidden="true" />
          {isSubmitting ? 'Gönderiliyor' : 'Gönder'}
        </Button>
        {message ? <p className="text-sm font-medium text-[#168c83]">{message}</p> : null}
      </div>
    </form>
  )
}
