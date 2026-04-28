import { BookOpen, FileQuestion, Library, ListChecks } from 'lucide-react'
import { Link } from 'react-router-dom'
import { Card } from '../../../shared/ui'

const cards = [
  { href: '/admin/content/lessons', title: 'Dersler', body: 'Draft, review, published ve archived dersleri yonet.', icon: BookOpen },
  { href: '/admin/content/lessons/new', title: 'Yeni AI Byte', body: 'Kalite kontrollu mikro ders olustur.', icon: ListChecks },
  { href: '/admin/content/quizzes', title: 'Quiz Editor', body: 'Checkpoint sorulari ve secenekleri ekle.', icon: FileQuestion },
  { href: '/admin/content/resources', title: 'Kaynaklar', body: 'Guvenilir kaynak linklerini ekle ve duzenle.', icon: Library },
]

export function ContentDashboard() {
  return (
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
      {cards.map(({ href, title, body, icon: Icon }) => (
        <Link key={href} to={href}>
          <Card className="h-full transition hover:border-primary">
            <Icon aria-hidden="true" className="size-5 text-primary" />
            <h2 className="mt-3 font-bold">{title}</h2>
            <p className="mt-2 text-sm leading-6 text-muted">{body}</p>
          </Card>
        </Link>
      ))}
    </div>
  )
}
