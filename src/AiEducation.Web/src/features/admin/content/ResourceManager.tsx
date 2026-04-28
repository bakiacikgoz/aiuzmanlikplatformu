import { Plus } from 'lucide-react'
import { type FormEvent, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../../../shared/api'
import type { AdminResourceDto } from '../../../shared/types'
import { Badge, Button, Card, CardHeader, Field, TextArea, TextInput } from '../../../shared/ui'

const emptyResource = {
  slug: '',
  title: '',
  url: '',
  type: 'article',
  summary: '',
}

export function ResourceManager() {
  const queryClient = useQueryClient()
  const [form, setForm] = useState(emptyResource)
  const [error, setError] = useState('')
  const { data: resources = [] } = useQuery({ queryKey: ['admin-resources'], queryFn: () => api<AdminResourceDto[]>('/admin/resources') })
  const mutation = useMutation({
    mutationFn: () => api('/admin/resources', { method: 'POST', body: JSON.stringify(form) }),
    onSuccess: async () => {
      setForm(emptyResource)
      await queryClient.invalidateQueries({ queryKey: ['admin-resources'] })
    },
  })

  async function submit(event: FormEvent) {
    event.preventDefault()
    setError('')
    if (!form.slug.trim() || !form.title.trim() || !form.url.trim()) {
      setError('Slug, baslik ve URL zorunludur.')
      return
    }
    await mutation.mutateAsync()
  }

  return (
    <div className="grid gap-5 lg:grid-cols-[420px_1fr]">
      <Card>
        <CardHeader title="Kaynak ekle" description="http/https kaynak linki kullan." />
        <form onSubmit={submit} className="grid gap-4">
          <Field label="Slug">
            <TextInput value={form.slug} onChange={(event) => setForm({ ...form, slug: event.target.value })} />
          </Field>
          <Field label="Baslik">
            <TextInput value={form.title} onChange={(event) => setForm({ ...form, title: event.target.value })} />
          </Field>
          <Field label="URL">
            <TextInput value={form.url} onChange={(event) => setForm({ ...form, url: event.target.value })} />
          </Field>
          <Field label="Tip">
            <TextInput value={form.type} onChange={(event) => setForm({ ...form, type: event.target.value })} />
          </Field>
          <Field label="Ozet">
            <TextArea value={form.summary} onChange={(event) => setForm({ ...form, summary: event.target.value })} />
          </Field>
          {error ? <p role="alert" className="rounded-xl border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</p> : null}
          <Button type="submit" disabled={mutation.isPending}>
            <Plus aria-hidden="true" className="size-4" />
            Kaynak ekle
          </Button>
        </form>
      </Card>
      <Card>
        <CardHeader title="Kaynak listesi" description="Ders editorunde secilebilir kaynaklar." />
        {resources.length === 0 ? <p className="rounded-xl border border-border bg-white p-4 text-sm text-muted">Henuz kaynak yok.</p> : null}
        <div className="grid gap-3">
          {resources.map((resource) => (
            <div key={resource.id} className="rounded-xl border border-border bg-white p-4">
              <Badge tone="neutral">{resource.type}</Badge>
              <h3 className="mt-2 font-semibold text-ink">{resource.title}</h3>
              <a className="text-sm text-primary" href={resource.url}>{resource.url}</a>
              {resource.summary ? <p className="mt-2 text-sm text-muted">{resource.summary}</p> : null}
            </div>
          ))}
        </div>
      </Card>
    </div>
  )
}
