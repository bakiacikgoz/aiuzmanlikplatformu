export type QualityInput = {
  learningObjective: string
  estimatedMinutes: number
  miniExplanation: string
  miniExample: string
  exercisePrompt?: string
  hasExercise?: boolean
  hasFeedback?: boolean
  hasResource?: boolean
  nextStep: string
  xpReward: number
}

export function wordCount(value: string) {
  return value.trim().split(/\s+/).filter(Boolean).length
}

export function calculateQualityScore(input: QualityInput) {
  let score = 0
  if (input.learningObjective.trim()) score += 10
  if (input.estimatedMinutes >= 1 && input.estimatedMinutes <= 10) score += 10
  if (input.miniExplanation.trim() && wordCount(input.miniExplanation) <= 180) score += 15
  if (input.miniExample.trim()) score += 15
  if (input.hasExercise || input.exercisePrompt?.trim()) score += 20
  if (input.hasFeedback || input.exercisePrompt?.trim()) score += 10
  if (input.hasResource) score += 10
  if (input.nextStep.trim()) score += 10
  return score
}
