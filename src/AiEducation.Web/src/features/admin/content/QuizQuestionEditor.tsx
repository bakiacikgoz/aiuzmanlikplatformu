import { Plus, Trash2 } from 'lucide-react'
import { type FormEvent, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../../../shared/api'
import type { AdminLessonDto, QuizOptionDto, QuizQuestionDto } from '../../../shared/types'
import { Button, Card, CardHeader, Field, SecondaryButton, TextArea, TextInput } from '../../../shared/ui'

type QuestionFormValues = {
  questionText: string
  questionType: 'multiple_choice'
  sortOrder: number
  explanation: string
  options: QuizOptionDto[]
}

const defaultQuestion: QuestionFormValues = {
  questionText: '',
  questionType: 'multiple_choice',
  sortOrder: 1,
  explanation: '',
  options: [1, 2, 3, 4].map((sortOrder) => ({ text: '', sortOrder, isCorrect: false })),
}

export function QuizQuestionForm({ onSubmit }: { onSubmit: (values: QuestionFormValues) => Promise<void> }) {
  const [values, setValues] = useState<QuestionFormValues>(defaultQuestion)
  const [error, setError] = useState('')
  const [saved, setSaved] = useState(false)

  function setOption(index: number, patch: Partial<QuizOptionDto>) {
    setValues((current) => ({
      ...current,
      options: current.options.map((option, optionIndex) => (optionIndex === index ? { ...option, ...patch } : option)),
    }))
  }

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    if (!values.questionText.trim()) {
      setError('Soru metni zorunludur.')
      return
    }
    if (values.options.some((option) => !option.text.trim())) {
      setError('Tum secenekler dolu olmalidir.')
      return
    }
    if (values.options.filter((option) => option.isCorrect).length !== 1) {
      setError('Tam olarak bir dogru secenek secilmelidir.')
      return
    }
    await onSubmit(values)
    setSaved(true)
    setValues(defaultQuestion)
  }

  return (
    <Card>
      <CardHeader title="Quiz sorusu" description="MVP icin 4 secenek ve tam olarak 1 dogru cevap kullan." />
      <form onSubmit={submit} className="grid gap-4">
        <Field label="Soru metni">
          <TextArea value={values.questionText} onChange={(event) => setValues({ ...values, questionText: event.target.value })} />
        </Field>
        <Field label="Aciklama">
          <TextArea value={values.explanation} onChange={(event) => setValues({ ...values, explanation: event.target.value })} />
        </Field>
        <Field label="Sira">
          <TextInput type="number" min={1} value={values.sortOrder} onChange={(event) => setValues({ ...values, sortOrder: Number(event.target.value) })} />
        </Field>
        <div className="grid gap-3">
          {values.options.map((option, index) => (
            <div key={option.sortOrder} className="grid gap-2 rounded-xl border border-border bg-white p-3 md:grid-cols-[1fr_auto]">
              <Field label={`Secenek ${index + 1}`}>
                <TextInput value={option.text} onChange={(event) => setOption(index, { text: event.target.value })} />
              </Field>
              <label className="flex items-center gap-2 text-sm font-semibold text-ink">
                <input
                  type="radio"
                  name="correct-option"
                  checked={Boolean(option.isCorrect)}
                  onChange={() => setValues((current) => ({ ...current, options: current.options.map((item, itemIndex) => ({ ...item, isCorrect: itemIndex === index })) }))}
                />
                Dogru
              </label>
            </div>
          ))}
        </div>
        {error ? <p role="alert" className="rounded-xl border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</p> : null}
        {saved ? <p className="text-sm font-semibold text-[#168c83]">Soru kaydedildi.</p> : null}
        <Button type="submit">
          <Plus aria-hidden="true" className="size-4" />
          Soru ekle
        </Button>
      </form>
    </Card>
  )
}

export function QuizQuestionEditor() {
  const queryClient = useQueryClient()
  const [lessonId, setLessonId] = useState('')
  const { data: lessons = [] } = useQuery({ queryKey: ['admin-lessons'], queryFn: () => api<AdminLessonDto[]>('/admin/lessons') })
  const selectedLessonId = lessonId || lessons[0]?.id || ''
  const { data: questions = [] } = useQuery({
    queryKey: ['admin-quiz-questions', selectedLessonId],
    queryFn: () => api<QuizQuestionDto[]>(`/admin/lessons/${selectedLessonId}/quiz-questions`),
    enabled: Boolean(selectedLessonId),
  })
  const createMutation = useMutation({
    mutationFn: (values: QuestionFormValues) => api(`/admin/lessons/${selectedLessonId}/quiz-questions`, { method: 'POST', body: JSON.stringify(values) }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-quiz-questions', selectedLessonId] }),
  })
  const deleteMutation = useMutation({
    mutationFn: (id: string) => api(`/admin/quiz-questions/${id}`, { method: 'DELETE' }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-quiz-questions', selectedLessonId] }),
  })

  return (
    <div className="grid gap-5 xl:grid-cols-[420px_1fr]">
      <div className="grid gap-5">
        <Card>
          <CardHeader title="Quiz dersi" description="Sorularin baglanacagi dersi sec." />
          <select className="min-h-10 rounded-xl border border-border bg-white px-3 text-sm" value={selectedLessonId} onChange={(event) => setLessonId(event.target.value)}>
            {lessons.map((lesson) => (
              <option key={lesson.id} value={lesson.id}>{lesson.title}</option>
            ))}
          </select>
        </Card>
        <QuizQuestionForm onSubmit={async (values) => { await createMutation.mutateAsync(values) }} />
      </div>
      <Card>
        <CardHeader title="Sorular" description="Student endpointinde dogru cevap alanlari gosterilmez." />
        {questions.length === 0 ? <p className="rounded-xl border border-border bg-white p-4 text-sm text-muted">Bu ders icin quiz sorusu yok.</p> : null}
        <div className="grid gap-3">
          {questions.map((question) => (
            <div key={question.id} className="rounded-xl border border-border bg-white p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <h3 className="font-semibold text-ink">{question.questionText}</h3>
                  <p className="mt-1 text-sm text-muted">{question.explanation}</p>
                </div>
                <SecondaryButton onClick={() => deleteMutation.mutate(question.id)}>
                  <Trash2 aria-hidden="true" className="size-4" />
                  Sil
                </SecondaryButton>
              </div>
              <div className="mt-3 grid gap-2">
                {question.options.map((option) => (
                  <p key={`${question.id}-${option.sortOrder}`} className="rounded-lg bg-slate-50 px-3 py-2 text-sm">
                    {option.sortOrder}. {option.text} {option.isCorrect ? '(dogru)' : ''}
                  </p>
                ))}
              </div>
            </div>
          ))}
        </div>
      </Card>
    </div>
  )
}
