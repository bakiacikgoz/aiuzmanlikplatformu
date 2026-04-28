import { CheckCircle2, CircleAlert } from 'lucide-react'
import { Card, CardHeader } from '../../../shared/ui'
import { calculateQualityScore, type QualityInput, wordCount } from './quality'

export function QualityChecklist({ input, score = calculateQualityScore(input) }: { input: QualityInput; score?: number }) {
  const items = [
    ['Tek ogrenme hedefi', Boolean(input.learningObjective.trim())],
    ['3-5 dakikalik mikro ders', input.estimatedMinutes >= 3 && input.estimatedMinutes <= 5],
    ['Kisa mini aciklama', Boolean(input.miniExplanation.trim()) && wordCount(input.miniExplanation) <= 180],
    ['Mini ornek', Boolean(input.miniExample.trim())],
    ['Aktif egzersiz', Boolean(input.hasExercise || input.exercisePrompt?.trim())],
    ['Feedback veya scoring net', Boolean(input.hasFeedback || input.exercisePrompt?.trim())],
    ['Kaynak baglantisi', Boolean(input.hasResource)],
    ['Sonraki adim', Boolean(input.nextStep.trim())],
    ['XP kalite kapisi', input.xpReward > 0],
  ] as const

  return (
    <Card>
      <CardHeader title="Kalite kontrol" description={`${score}/100 skor. Publish icin minimum 70.`} />
      <div className="grid gap-2">
        {items.map(([label, passed]) => (
          <div key={label} className="flex items-center gap-2 rounded-xl border border-border bg-white px-3 py-2 text-sm">
            {passed ? <CheckCircle2 aria-hidden="true" className="size-4 text-[#168c83]" /> : <CircleAlert aria-hidden="true" className="size-4 text-[#b9620b]" />}
            <span className={passed ? 'text-ink' : 'text-muted'}>{label}</span>
          </div>
        ))}
      </div>
    </Card>
  )
}
