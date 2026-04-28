import type { AdminResourceDto } from '../../../shared/types'
import { Card, CardHeader } from '../../../shared/ui'

export function ResourcePicker({
  resources,
  selectedIds,
  onChange,
}: {
  resources: AdminResourceDto[]
  selectedIds: string[]
  onChange: (ids: string[]) => void
}) {
  return (
    <Card>
      <CardHeader title="Kaynaklar" description="Dersi publish etmek icin en az bir kaynak sec." />
      {resources.length === 0 ? <p className="text-sm text-muted">Kaynak bulunmuyor.</p> : null}
      <div className="grid gap-2">
        {resources.map((resource) => {
          const checked = selectedIds.includes(resource.id)
          return (
            <label key={resource.id} className="flex items-start gap-3 rounded-xl border border-border bg-white p-3 text-sm">
              <input
                type="checkbox"
                checked={checked}
                onChange={() => onChange(checked ? selectedIds.filter((id) => id !== resource.id) : [...selectedIds, resource.id])}
              />
              <span>
                <span className="block font-semibold text-ink">{resource.title}</span>
                <span className="block text-muted">{resource.url}</span>
              </span>
            </label>
          )
        })}
      </div>
    </Card>
  )
}
