import { Save, Send } from 'lucide-react'
import { type FormEvent, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate, useParams } from 'react-router-dom'
import { api, post } from '../../../shared/api'
import type { AdminLessonDto, AdminResourceDto, AdminUnitDto, ContentStatus } from '../../../shared/types'
import { Button, Card, CardHeader, Field, SecondaryButton, TextArea, TextInput } from '../../../shared/ui'
import { QualityChecklist } from './QualityChecklist'
import { calculateQualityScore } from './quality'
import { ResourcePicker } from './ResourcePicker'

export type LessonEditorValues = {
  unitSlug: string
  slug: string
  title: string
  learningObjective: string
  miniExplanation: string
  miniExample: string
  nextStep: string
  exercisePrompt: string
  durationMinutes: number
  xpReward: number
  sortOrder: number
  resourceIds: string[]
}

const emptyLesson: LessonEditorValues = {
  unitSlug: '',
  slug: '',
  title: '',
  learningObjective: '',
  miniExplanation: '',
  miniExample: '',
  nextStep: '',
  exercisePrompt: '',
  durationMinutes: 4,
  xpReward: 10,
  sortOrder: 1,
  resourceIds: [],
}

function valuesFromLesson(lesson?: AdminLessonDto): LessonEditorValues {
  if (!lesson) return emptyLesson
  return {
    unitSlug: lesson.unitSlug ?? '',
    slug: lesson.slug,
    title: lesson.title,
    learningObjective: lesson.learningObjective,
    miniExplanation: lesson.miniExplanation,
    miniExample: lesson.miniExample,
    nextStep: lesson.nextStep,
    exercisePrompt: lesson.exercisePrompt ?? '',
    durationMinutes: lesson.estimatedMinutes,
    xpReward: lesson.xpReward,
    sortOrder: lesson.sortOrder,
    resourceIds: lesson.resourceIds ?? [],
  }
}

export function LessonEditorForm({
  initialLesson,
  resources,
  onSave,
  onStatusChange,
}: {
  initialLesson?: AdminLessonDto
  resources: AdminResourceDto[]
  onSave: (values: LessonEditorValues) => Promise<void>
  onStatusChange?: (status: ContentStatus, values: LessonEditorValues) => Promise<void>
}) {
  const [values, setValues] = useState(() => valuesFromLesson(initialLesson))
  const [activeTab, setActiveTab] = useState('Temel')
  const [error, setError] = useState('')
  const [saved, setSaved] = useState(false)
  const qualityInput = {
    learningObjective: values.learningObjective,
    estimatedMinutes: values.durationMinutes,
    miniExplanation: values.miniExplanation,
    miniExample: values.miniExample,
    exercisePrompt: values.exercisePrompt,
    hasResource: values.resourceIds.length > 0,
    nextStep: values.nextStep,
    xpReward: values.xpReward,
  }
  const qualityScore = calculateQualityScore(qualityInput)
  const publishDisabled = qualityScore < 70

  function setField<K extends keyof LessonEditorValues>(key: K, value: LessonEditorValues[K]) {
    setValues((current) => ({ ...current, [key]: value }))
  }

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    if (!values.unitSlug.trim() || !values.slug.trim() || !values.title.trim() || !values.learningObjective.trim()) {
      setError('Unit, slug, baslik ve hedef zorunludur.')
      return
    }
    if (!values.exercisePrompt.trim()) {
      setError('En az bir egzersiz promptu zorunludur.')
      return
    }
    await onSave(values)
    setSaved(true)
  }

  async function changeStatus(status: ContentStatus) {
    if (!onStatusChange) return
    setError('')
    await onStatusChange(status, values)
    setSaved(true)
  }

  const tabs = ['Temel', 'Icerik', 'Egzersiz', 'Kaynaklar', 'Kalite']

  return (
    <div className="grid gap-5 xl:grid-cols-[1fr_360px]">
      <Card>
        <CardHeader title={initialLesson ? 'AI Byte duzenle' : 'Yeni AI Byte'} description="Tek hedefli, kisa ve publish kapisindan gecen mikro ders olustur." />
        <div className="mb-4 flex gap-2 overflow-x-auto">
          {tabs.map((tab) => (
            <button
              key={tab}
              type="button"
              className={`min-h-10 rounded-xl border px-4 text-sm font-semibold ${activeTab === tab ? 'border-primary bg-[#eef3ff] text-primary' : 'border-border bg-white text-muted'}`}
              onClick={() => setActiveTab(tab)}
            >
              {tab}
            </button>
          ))}
        </div>

        <form onSubmit={submit} className="grid gap-4">
          {activeTab === 'Temel' ? (
            <div className="grid gap-4 md:grid-cols-2">
              <Field label="Unit slug">
                <TextInput value={values.unitSlug} onChange={(event) => setField('unitSlug', event.target.value)} />
              </Field>
              <Field label="Ders slug">
                <TextInput value={values.slug} onChange={(event) => setField('slug', event.target.value)} disabled={Boolean(initialLesson)} />
              </Field>
              <Field label="Baslik">
                <TextInput value={values.title} onChange={(event) => setField('title', event.target.value)} />
              </Field>
              <Field label="Sure dakika">
                <TextInput type="number" min={1} max={10} value={values.durationMinutes} onChange={(event) => setField('durationMinutes', Number(event.target.value))} />
              </Field>
              <Field label="XP">
                <TextInput type="number" min={1} value={values.xpReward} onChange={(event) => setField('xpReward', Number(event.target.value))} />
              </Field>
              <Field label="Sira">
                <TextInput type="number" min={1} value={values.sortOrder} onChange={(event) => setField('sortOrder', Number(event.target.value))} />
              </Field>
            </div>
          ) : null}

          {activeTab === 'Icerik' ? (
            <div className="grid gap-4">
              <Field label="Ogrenme hedefi">
                <TextInput value={values.learningObjective} onChange={(event) => setField('learningObjective', event.target.value)} />
              </Field>
              <Field label="Mini aciklama">
                <TextArea value={values.miniExplanation} onChange={(event) => setField('miniExplanation', event.target.value)} />
              </Field>
              <Field label="Mini ornek">
                <TextArea value={values.miniExample} onChange={(event) => setField('miniExample', event.target.value)} />
              </Field>
              <Field label="Sonraki adim">
                <TextInput value={values.nextStep} onChange={(event) => setField('nextStep', event.target.value)} />
              </Field>
            </div>
          ) : null}

          {activeTab === 'Egzersiz' ? (
            <Field label="Egzersiz promptu">
              <TextArea value={values.exercisePrompt} onChange={(event) => setField('exercisePrompt', event.target.value)} />
            </Field>
          ) : null}

          {activeTab === 'Kaynaklar' ? <ResourcePicker resources={resources} selectedIds={values.resourceIds} onChange={(ids) => setField('resourceIds', ids)} /> : null}

          {activeTab === 'Kalite' ? <QualityChecklist input={qualityInput} score={qualityScore} /> : null}

          {error ? <p role="alert" className="rounded-xl border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</p> : null}
          {saved ? <p className="text-sm font-semibold text-[#168c83]">Degisiklikler kaydedildi.</p> : null}

          <div className="flex flex-wrap gap-3">
            <Button type="submit">
              <Save aria-hidden="true" className="size-4" />
              Draft kaydet
            </Button>
            <SecondaryButton type="button" disabled={!onStatusChange} onClick={() => changeStatus('ReadyForReview')}>
              Review yap
            </SecondaryButton>
            <Button type="button" disabled={publishDisabled || !onStatusChange} onClick={() => changeStatus('Published')}>
              <Send aria-hidden="true" className="size-4" />
              Publish
            </Button>
          </div>
        </form>
      </Card>
      <QualityChecklist input={qualityInput} score={qualityScore} />
    </div>
  )
}

export function LessonEditor() {
  const { id } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const isNew = !id
  const { data: lesson } = useQuery({ queryKey: ['admin-lesson', id], queryFn: () => api<AdminLessonDto>(`/admin/lessons/${id}`), enabled: Boolean(id) })
  const { data: resources = [] } = useQuery({ queryKey: ['admin-resources'], queryFn: () => api<AdminResourceDto[]>('/admin/resources') })
  const { data: units = [] } = useQuery({ queryKey: ['admin-units'], queryFn: () => api<AdminUnitDto[]>('/admin/units') })

  const normalizedLesson = useMemo(() => (lesson ? { ...lesson, unitSlug: lesson.unitSlug ?? units[0]?.slug ?? '' } : undefined), [lesson, units])
  const mutation = useMutation({
    mutationFn: async (values: LessonEditorValues) => {
      if (isNew) {
        const created = await post<AdminLessonDto>('/admin/lessons', {
          unitSlug: values.unitSlug,
          slug: values.slug,
          title: values.title,
          learningObjective: values.learningObjective,
          miniExplanation: values.miniExplanation,
          tinyExample: values.miniExample,
          nextStep: values.nextStep,
          exercisePrompt: values.exercisePrompt,
          durationMinutes: values.durationMinutes,
          xpReward: values.xpReward,
          sortOrder: values.sortOrder,
          status: 'Draft',
        })
        if (values.resourceIds.length > 0) {
          await api(`/admin/lessons/${created.id}`, { method: 'PATCH', body: JSON.stringify({ resourceIds: values.resourceIds }) })
        }
        navigate(`/admin/content/lessons/${created.id}/edit`)
        return
      }

      await api(`/admin/lessons/${id}`, {
        method: 'PATCH',
        body: JSON.stringify({
          title: values.title,
          learningObjective: values.learningObjective,
          miniExplanation: values.miniExplanation,
          tinyExample: values.miniExample,
          nextStep: values.nextStep,
          exercisePrompt: values.exercisePrompt,
          durationMinutes: values.durationMinutes,
          xpReward: values.xpReward,
          sortOrder: values.sortOrder,
          resourceIds: values.resourceIds,
        }),
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['admin-lessons'] })
      await queryClient.invalidateQueries({ queryKey: ['admin-lesson', id] })
    },
  })

  async function setStatus(status: ContentStatus, values: LessonEditorValues) {
    if (!id) {
      await mutation.mutateAsync(values)
      return
    }
    await api(`/admin/lessons/${id}/status`, { method: 'PATCH', body: JSON.stringify({ status }) })
    await queryClient.invalidateQueries({ queryKey: ['admin-lessons'] })
    await queryClient.invalidateQueries({ queryKey: ['admin-lesson', id] })
  }

  if (!isNew && !lesson) {
    return <Card><p className="text-sm text-muted">Ders yukleniyor.</p></Card>
  }

  return (
    <LessonEditorForm
      initialLesson={isNew ? undefined : normalizedLesson}
      resources={resources}
      onSave={(values) => mutation.mutateAsync(values)}
      onStatusChange={isNew ? undefined : setStatus}
    />
  )
}
