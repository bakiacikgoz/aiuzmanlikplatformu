import { type FormEvent, useState } from 'react'
import { Button, Card, CardHeader, Field, TextArea, TextInput } from '../../shared/ui'

export type AdminLessonPayload = {
  unitSlug: string
  slug: string
  title: string
  learningObjective: string
  miniExplanation: string
  tinyExample: string
  exercisePrompt: string
  durationMinutes: number
  xpReward: number
  sortOrder: number
}

export function AdminLessonForm({ onSubmit }: { onSubmit: (payload: AdminLessonPayload) => Promise<void> }) {
  const [payload, setPayload] = useState<AdminLessonPayload>({
    unitSlug: '',
    slug: '',
    title: '',
    learningObjective: '',
    miniExplanation: '',
    tinyExample: '',
    exercisePrompt: '',
    durationMinutes: 4,
    xpReward: 10,
    sortOrder: 1,
  })
  const [saved, setSaved] = useState(false)

  async function submit(event: FormEvent) {
    event.preventDefault()
    await onSubmit(payload)
    setSaved(true)
  }

  return (
    <Card>
      <CardHeader title="Yeni mikro ders" description="Tek hedef, tek örnek ve tek alıştırma standardını koru." />
      <form onSubmit={submit} className="grid gap-4 md:grid-cols-2">
        <Field label="Modül slug">
          <TextInput value={payload.unitSlug} onChange={(event) => setPayload({ ...payload, unitSlug: event.target.value })} />
        </Field>
        <Field label="Ders slug">
          <TextInput value={payload.slug} onChange={(event) => setPayload({ ...payload, slug: event.target.value })} />
        </Field>
        <Field label="Başlık">
          <TextInput value={payload.title} onChange={(event) => setPayload({ ...payload, title: event.target.value })} />
        </Field>
        <Field label="Hedef">
          <TextInput value={payload.learningObjective} onChange={(event) => setPayload({ ...payload, learningObjective: event.target.value })} />
        </Field>
        <Field label="Mini açıklama">
          <TextArea value={payload.miniExplanation} onChange={(event) => setPayload({ ...payload, miniExplanation: event.target.value })} />
        </Field>
        <Field label="Mini örnek">
          <TextArea value={payload.tinyExample} onChange={(event) => setPayload({ ...payload, tinyExample: event.target.value })} />
        </Field>
        <div className="md:col-span-2">
          <Field label="Alıştırma">
            <TextArea value={payload.exercisePrompt} onChange={(event) => setPayload({ ...payload, exercisePrompt: event.target.value })} />
          </Field>
        </div>
        <div className="flex items-center gap-3 md:col-span-2">
          <Button type="submit">Ders ekle</Button>
          {saved ? <p className="text-sm font-medium text-[#168c83]">Ders kaydedildi.</p> : null}
        </div>
      </form>
    </Card>
  )
}
