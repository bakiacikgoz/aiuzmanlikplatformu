import type { ContentStatus } from '../../../shared/types'
import { Badge } from '../../../shared/ui'

const labels: Record<ContentStatus, string> = {
  Draft: 'Draft',
  ReadyForReview: 'Review',
  Published: 'Published',
  NeedsRevision: 'Revision',
  Archived: 'Archived',
}

export function ContentStatusBadge({ status }: { status: ContentStatus }) {
  const tone = status === 'Published' ? 'secondary' : status === 'Archived' ? 'neutral' : status === 'NeedsRevision' ? 'accent' : 'primary'
  return <Badge tone={tone}>{labels[status]}</Badge>
}
