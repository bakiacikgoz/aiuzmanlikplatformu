import { Archive, Eye, Pencil, Plus } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../../../shared/api'
import type { AdminLessonDto, ContentStatus } from '../../../shared/types'
import { Badge, Button, Card, CardHeader, SecondaryButton } from '../../../shared/ui'
import { ContentStatusBadge } from './ContentStatusBadge'

export function LessonList() {
  const queryClient = useQueryClient()
  const { data: lessons = [], isLoading } = useQuery({ queryKey: ['admin-lessons'], queryFn: () => api<AdminLessonDto[]>('/admin/lessons') })
  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: ContentStatus }) => api(`/admin/lessons/${id}/status`, { method: 'PATCH', body: JSON.stringify({ status }) }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-lessons'] }),
  })

  if (isLoading) {
    return <Card><p className="text-sm text-muted">Dersler yukleniyor.</p></Card>
  }

  return (
    <Card>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <CardHeader title="Dersler" description="Draft, review, published ve archived icerik durumlarini yonet." />
        <Link to="/admin/content/lessons/new">
          <Button>
            <Plus aria-hidden="true" className="size-4" />
            Yeni AI Byte
          </Button>
        </Link>
      </div>
      {lessons.length === 0 ? <p className="rounded-xl border border-border bg-white p-4 text-sm text-muted">Henuz ders yok.</p> : null}
      <div className="overflow-x-auto">
        <table className="w-full min-w-[760px] text-left text-sm">
          <thead className="text-xs uppercase text-muted">
            <tr>
              <th className="px-3 py-2">Ders</th>
              <th className="px-3 py-2">Unit</th>
              <th className="px-3 py-2">Durum</th>
              <th className="px-3 py-2">Kalite</th>
              <th className="px-3 py-2">Aksiyon</th>
            </tr>
          </thead>
          <tbody>
            {lessons.map((lesson) => (
              <tr key={lesson.id} className="border-t border-border">
                <td className="px-3 py-3">
                  <p className="font-semibold text-ink">{lesson.title}</p>
                  <p className="text-xs text-muted">{lesson.slug}</p>
                </td>
                <td className="px-3 py-3 text-muted">{lesson.unitSlug ?? lesson.unitId}</td>
                <td className="px-3 py-3"><ContentStatusBadge status={lesson.status} /></td>
                <td className="px-3 py-3"><Badge tone={lesson.qualityScore >= 70 ? 'secondary' : 'accent'}>{lesson.qualityScore}/100</Badge></td>
                <td className="px-3 py-3">
                  <div className="flex flex-wrap gap-2">
                    <Link to={`/admin/content/lessons/${lesson.id}/edit`}>
                      <SecondaryButton>
                        <Pencil aria-hidden="true" className="size-4" />
                        Duzenle
                      </SecondaryButton>
                    </Link>
                    <Link to={`/admin/content/lessons/${lesson.id}/preview`}>
                      <SecondaryButton>
                        <Eye aria-hidden="true" className="size-4" />
                        Preview
                      </SecondaryButton>
                    </Link>
                    <SecondaryButton disabled={lesson.status === 'Archived' || statusMutation.isPending} onClick={() => statusMutation.mutate({ id: lesson.id, status: 'Archived' })}>
                      <Archive aria-hidden="true" className="size-4" />
                      Arsivle
                    </SecondaryButton>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Card>
  )
}
