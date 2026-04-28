export type LessonStep = {
  key: string
  title: string
  body: string
}

export type Lesson = {
  slug: string
  title: string
  durationMinutes: number
  xpReward: number
  learningObjective: string
  steps: LessonStep[]
  exercise?: {
    id: string
    prompt: string
    type: string
    requiresAiFeedback: boolean
  } | null
  quizQuestions?: Array<{
    id: string
    prompt: string
    options: Array<{
      id: string
      text: string
    }>
  }>
}

export type ContentStatus = 'Draft' | 'ReadyForReview' | 'Published' | 'NeedsRevision' | 'Archived'

export type AdminLessonDto = {
  id: string
  slug: string
  title: string
  unitId: string | null
  unitSlug?: string | null
  status: ContentStatus
  isArchived: boolean
  learningObjective: string
  estimatedMinutes: number
  miniExplanation: string
  miniExample: string
  nextStep: string
  exercisePrompt?: string
  resourceIds?: string[]
  xpReward: number
  sortOrder: number
  qualityScore: number
}

export type AdminUnitDto = {
  id: string
  slug: string
  title: string
  learningPathSlug?: string
  sortOrder: number
}

export type AdminResourceDto = {
  id: string
  slug: string
  title: string
  url: string
  type: string
  summary?: string
}

export type QuizOptionDto = {
  id?: string
  text: string
  sortOrder: number
  isCorrect?: boolean
}

export type QuizQuestionDto = {
  id: string
  questionText: string
  questionType: 'multiple_choice'
  sortOrder: number
  explanation?: string
  options: QuizOptionDto[]
}

export type DashboardData = {
  user: {
    displayName: string
    dailyXpGoal: number
    selectedLearningPathSlug: string | null
  }
  todayAiByte: {
    slug: string
    title: string
    durationMinutes: number
    xpReward: number
    learningObjective: string
  } | null
  totalXp: number
  currentStreakDays: number
  portfolioEvidenceCount: number
}

export type League = {
  name: string
  tier: string
  startsAtUtc?: string
  endsAtUtc?: string
  participants: Array<{
    rank: number
    displayName: string
    weeklyXp: number
    isCurrentUser: boolean
  }>
}
