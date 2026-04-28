import { useQuery } from '@tanstack/react-query'
import { useParams } from 'react-router-dom'
import { api } from '../../../shared/api'
import type { AdminLessonDto, Lesson } from '../../../shared/types'
import { Card } from '../../../shared/ui'
import { LessonPlayer } from '../../lessonPlayer/LessonPlayer'

function toPreviewLesson(lesson: AdminLessonDto): Lesson {
  return {
    slug: lesson.slug,
    title: lesson.title,
    durationMinutes: lesson.estimatedMinutes,
    xpReward: lesson.xpReward,
    learningObjective: lesson.learningObjective,
    exercise: lesson.exercisePrompt ? { id: 'preview-exercise', prompt: lesson.exercisePrompt, type: 'short_answer', requiresAiFeedback: true } : null,
    steps: [
      { key: 'goal', title: 'Bugunku kucuk hedef', body: lesson.learningObjective },
      { key: 'mini-explanation', title: 'Mini aciklama', body: lesson.miniExplanation },
      { key: 'example', title: 'Mini ornek', body: lesson.miniExample },
      { key: 'exercise', title: 'Simdi sen dene', body: lesson.exercisePrompt ?? 'Bu ders icin egzersiz eklenmedi.' },
      { key: 'feedback', title: 'Aninda feedback', body: 'Preview modunda egzersiz cevabi kaydedilmez.' },
      { key: 'reward', title: 'Odul', body: `${lesson.xpReward} XP kazanilir.` },
      { key: 'next', title: 'Sonraki adim', body: lesson.nextStep },
    ],
  }
}

export function LessonPreview() {
  const { id } = useParams()
  const { data: lesson } = useQuery({ queryKey: ['admin-lesson', id], queryFn: () => api<AdminLessonDto>(`/admin/lessons/${id}`), enabled: Boolean(id) })

  if (!lesson) {
    return <Card><p className="text-sm text-muted">Preview yukleniyor.</p></Card>
  }

  return (
    <LessonPlayer
      lesson={toPreviewLesson(lesson)}
      onExerciseSubmit={async () => ({ feedback: 'Preview feedback basarili.', passedQualityGate: true })}
      onComplete={async () => undefined}
    />
  )
}
