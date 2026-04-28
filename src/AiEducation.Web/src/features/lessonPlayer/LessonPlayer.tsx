import { Check, ChevronLeft, ChevronRight } from 'lucide-react'
import { useState } from 'react'
import type { Lesson } from '../../shared/types'
import { Badge, Button, Card, SecondaryButton } from '../../shared/ui'
import { ExerciseSubmit } from '../practice/ExerciseSubmit'

export function LessonPlayer({ lesson, onComplete }: { lesson: Lesson; onComplete: () => Promise<void> }) {
  const [index, setIndex] = useState(0)
  const [status, setStatus] = useState<'idle' | 'saving' | 'done'>('idle')
  const step = lesson.steps[index]
  const isLast = index === lesson.steps.length - 1

  async function complete() {
    setStatus('saving')
    await onComplete()
    setStatus('done')
  }

  return (
    <Card className="mx-auto max-w-3xl">
      <div className="mb-5 flex flex-wrap items-start justify-between gap-3">
        <div>
          <Badge tone="secondary">
            {lesson.durationMinutes} dk · {lesson.xpReward} XP
          </Badge>
          <h1 className="mt-3 text-2xl font-bold text-ink">{lesson.title}</h1>
          <p className="mt-2 text-sm text-muted">{lesson.learningObjective}</p>
        </div>
        <p className="text-sm font-semibold text-muted">
          {index + 1}/{lesson.steps.length}
        </p>
      </div>

      <div className="mb-5 h-2 rounded-full bg-slate-100">
        <div className="h-2 rounded-full bg-primary transition-all" style={{ width: `${((index + 1) / lesson.steps.length) * 100}%` }} />
      </div>

      <article className="rounded-panel border border-border bg-[#f9fbff] p-5">
        <h2 className="text-xl font-semibold text-ink">{step.title}</h2>
        <p className="mt-3 leading-7 text-muted">{step.body}</p>
        {step.key === 'exercise' ? <ExerciseSubmit prompt={step.body} onSubmit={async () => 'Cevabın kaydedildi. Dersi bitirebilirsin.'} /> : null}
      </article>

      <div className="mt-6 flex items-center justify-between gap-3">
        <SecondaryButton disabled={index === 0} onClick={() => setIndex((value) => Math.max(0, value - 1))}>
          <ChevronLeft aria-hidden="true" />
          Geri
        </SecondaryButton>
        {isLast ? (
          <Button onClick={complete} disabled={status === 'saving' || status === 'done'}>
            <Check aria-hidden="true" />
            {status === 'done' ? 'Tamamlandı' : 'Dersi tamamla'}
          </Button>
        ) : (
          <Button onClick={() => setIndex((value) => Math.min(lesson.steps.length - 1, value + 1))}>
            Sonraki
            <ChevronRight aria-hidden="true" />
          </Button>
        )}
      </div>
    </Card>
  )
}
