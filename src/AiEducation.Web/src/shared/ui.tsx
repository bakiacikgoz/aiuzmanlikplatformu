import type { ButtonHTMLAttributes, InputHTMLAttributes, PropsWithChildren, TextareaHTMLAttributes } from 'react'
import { cn } from './cn'

export function Card({ children, className }: PropsWithChildren<{ className?: string }>) {
  return <section className={cn('rounded-card border border-border bg-surface p-5 shadow-soft', className)}>{children}</section>
}

export function CardHeader({ title, description }: { title: string; description?: string }) {
  return (
    <header className="mb-4 flex flex-col gap-1">
      <h2 className="text-lg font-semibold text-ink">{title}</h2>
      {description ? <p className="text-sm text-muted">{description}</p> : null}
    </header>
  )
}

export function Button({ className, ...props }: ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button
      className={cn(
        'inline-flex min-h-10 items-center justify-center gap-2 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-white transition hover:bg-[#4367f7] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary disabled:cursor-not-allowed disabled:opacity-60',
        className,
      )}
      {...props}
    />
  )
}

export function SecondaryButton({ className, ...props }: ButtonHTMLAttributes<HTMLButtonElement>) {
  return (
    <button
      className={cn(
        'inline-flex min-h-10 items-center justify-center gap-2 rounded-xl border border-border bg-white px-4 py-2 text-sm font-semibold text-ink transition hover:border-primary focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary disabled:cursor-not-allowed disabled:opacity-60',
        className,
      )}
      {...props}
    />
  )
}

export function TextInput({ className, ...props }: InputHTMLAttributes<HTMLInputElement>) {
  return (
    <input
      className={cn(
        'min-h-10 rounded-xl border border-border bg-white px-3 py-2 text-sm text-ink outline-none transition placeholder:text-muted focus:border-primary focus:ring-2 focus:ring-[#5e7cfb22]',
        className,
      )}
      {...props}
    />
  )
}

export function TextArea({ className, ...props }: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return (
    <textarea
      className={cn(
        'min-h-28 rounded-xl border border-border bg-white px-3 py-2 text-sm text-ink outline-none transition placeholder:text-muted focus:border-primary focus:ring-2 focus:ring-[#5e7cfb22]',
        className,
      )}
      {...props}
    />
  )
}

export function Field({ label, children }: PropsWithChildren<{ label: string }>) {
  return (
    <label className="flex flex-col gap-2 text-sm font-medium text-ink">
      {label}
      {children}
    </label>
  )
}

export function Badge({ children, tone = 'primary' }: PropsWithChildren<{ tone?: 'primary' | 'secondary' | 'accent' | 'neutral' }>) {
  const tones = {
    primary: 'bg-[#eef3ff] text-[#4367f7]',
    secondary: 'bg-[#eafbfb] text-[#168c83]',
    accent: 'bg-[#fff3e2] text-[#b9620b]',
    neutral: 'bg-slate-100 text-slate-700',
  }
  return <span className={cn('inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold', tones[tone])}>{children}</span>
}
