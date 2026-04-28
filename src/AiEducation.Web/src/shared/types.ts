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
  participants: Array<{
    rank: number
    displayName: string
    weeklyXp: number
    isCurrentUser: boolean
  }>
}
